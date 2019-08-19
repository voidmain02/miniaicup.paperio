using System;
using System.Collections.Generic;
using System.Linq;
using MiniAiCup.Paperio.Core.Debug;

namespace MiniAiCup.Paperio.Core
{
	public class PlayerInternal : ICloneable
	{
		public string Id { get; }

		public int Score { get; set; }

		public PointsSet Territory { get; set; }

		public Point Position { get; set; }

		public int PathToNextPositionLength { get; set; }

		public int NitroStepsLeft { get; set; }

		public int SlowdownStepsLeft { get; set; }

		public Path Tail { get; set; }

		public Direction? Direction { get; set; }

		private readonly Lazy<Path> _pathToHome;

		public Path PathToHome => _pathToHome.Value;

		public DebugPlayerView DebugView => GetDebugView();

		private readonly Lazy<int[,]> _timeMap;

		public int[,] TimeMap => _timeMap.Value;

		private readonly Lazy<int> _timeToGetHome;

		public int TimeToGetHome => _timeToGetHome.Value;

		public PointsSet CapturedOnPathToHome { get; set; }

		private PlayerInternal(string id)
		{
			_pathToHome = new Lazy<Path>(BuildPathToHome);
			_timeMap = new Lazy<int[,]>(BuildTimeMap);
			_timeToGetHome = new Lazy<int>(() => Id == Constants.MyId ? GetTimeForPath(PathToHome.Length) : Territory.Min(p => TimeMap[p.X, p.Y]));

			Id = id;
		}

		public PlayerInternal(PlayerInfo player) : this(player.Id)
		{
			Score = player.Score;
			Territory = new PointsSet(player.Territory.Select(p => p.ConvertToLogic(GameParams.CellSize)));
			Tail = new Path(player.Lines.Select(p => p.ConvertToLogic(GameParams.CellSize)));
			Direction = player.Direction;

			foreach (var bonus in player.Bonuses)
			{
				switch (bonus.Type)
				{
					case BonusType.Nitro:
						NitroStepsLeft = bonus.RemainingSteps;
						break;
					case BonusType.Slowdown:
						SlowdownStepsLeft = bonus.RemainingSteps;
						break;
					case BonusType.Saw:
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}

			switch (player.Direction)
			{
				case Core.Direction.Left:
					int leftPath = (player.Position.X - GameParams.CellSize/2)%GameParams.CellSize;
					PathToNextPositionLength = leftPath == 0 ? 0 : GameParams.CellSize - leftPath;
					Position = new Point(player.Position.X + PathToNextPositionLength, player.Position.Y).ConvertToLogic(GameParams.CellSize);
					break;
				case Core.Direction.Up:
					PathToNextPositionLength = (player.Position.Y - GameParams.CellSize/2)%GameParams.CellSize;
					Position = new Point(player.Position.X, player.Position.Y - PathToNextPositionLength).ConvertToLogic(GameParams.CellSize);
					break;
				case Core.Direction.Right:
					PathToNextPositionLength = (player.Position.X - GameParams.CellSize/2)%GameParams.CellSize;
					Position = new Point(player.Position.X - PathToNextPositionLength, player.Position.Y).ConvertToLogic(GameParams.CellSize);
					break;
				case Core.Direction.Down:
					int downPath = (player.Position.Y - GameParams.CellSize/2)%GameParams.CellSize;
					PathToNextPositionLength = downPath == 0 ? 0 : GameParams.CellSize - downPath;
					Position = new Point(player.Position.X, player.Position.Y + PathToNextPositionLength).ConvertToLogic(GameParams.CellSize);
					break;
				case null:
					Position = player.Position.ConvertToLogic(GameParams.CellSize);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		private DebugPlayerView GetDebugView()
		{
			return new DebugPlayerView {
				Id = Id,
				Territory = Territory.ToArray(),
				Tail = Tail.ToArray(),
				Position = Position,
				Direction = Direction,
				PathToNextPositionLength = PathToNextPositionLength
			};
		}

		public object Clone()
		{
			return new PlayerInternal(Id) {
				Score = Score,
				Territory = Territory,
				Position = Position,
				Tail = Tail,
				Direction = Direction,
				PathToNextPositionLength = PathToNextPositionLength,
				NitroStepsLeft = NitroStepsLeft,
				SlowdownStepsLeft = SlowdownStepsLeft,
				CapturedOnPathToHome = CapturedOnPathToHome
			};
		}

		private Path BuildPathToHome()
		{
			var startPosition = PathToNextPositionLength == 0 ? Position : Position.MoveLogic(Direction.Value);
			var path = PathFinder.GetShortestPath(startPosition, Territory, Tail.AsPointsSet());
			return PathToNextPositionLength != 0 ? path.Prepend(startPosition) : path;
		}

		private int[,] BuildTimeMap()
		{
			if (PathToNextPositionLength == 0)
			{
				return Tail.Length > 1 ? BuildOutsideTimeMap() : BuildInsideTimeMap();
			}

			if (Tail.Length == 0 || Territory.Contains(Position.MoveLogic(Direction.Value))) // Возможно, когда ход закончится следующая клетка будет уже не территорией игрока, но мы выбираем худший для нас вариант
			{
				return BuildInsideTimeMap();
			}

			return BuildOutsideTimeMap();
		}

		private unsafe int[,] BuildInsideTimeMap()
		{
			var timeMap = Game.GetNewMap<int>();

			if (Direction == null)
			{
				for (int y = 0; y < GameParams.MapSize.Height; y++)
				{
					for (int x = 0; x < GameParams.MapSize.Width; x++)
					{
						timeMap[x, y] = Position.GetDistanceTo(new Point(x, y))*GameParams.CellSize/GameParams.Speed;
					}
				}

				return timeMap;
			}

			if (PathToNextPositionLength == 0 && NitroStepsLeft == SlowdownStepsLeft)
			{
				var srcArray = Game.NoTailStandardSpeedTimeMaps[(int)Direction.Value];
				Utils.CopyArrayPart(srcArray, GameParams.MapSize.Width*2 -1, GameParams.MapSize.Height*2 - 1,
					timeMap, GameParams.MapSize.Width, GameParams.MapSize.Height,
					GameParams.MapSize.Width - Position.X - 1, GameParams.MapSize.Height - Position.Y - 1);
				return timeMap;
			}

			Utils.FastCopyArray(Game.NoEnemiesDangerousMap, timeMap, GameParams.MapSize.Width*GameParams.MapSize.Height);
			var visited = stackalloc bool[GameParams.MapSize.Width*GameParams.MapSize.Height];
			var distanceMap = stackalloc int[GameParams.MapSize.Width*GameParams.MapSize.Height];
			var queue = stackalloc int[GameParams.MapSize.Width*GameParams.MapSize.Height];
			int queueHead = 0;
			int queueTail = 0;

			Point startPoint;
			int currentTime;
			int currentDistance;

			if (PathToNextPositionLength > 0)
			{
				startPoint = Position.MoveLogic(Direction.Value);
				currentTime = (GameParams.CellSize - PathToNextPositionLength)/GetSpeed(0);
				currentDistance = 1;
			}
			else
			{
				startPoint = Position;
				currentTime = 0;
				currentDistance = 0;
			}

			int startPointCoord = startPoint.X + startPoint.Y*GameParams.MapSize.Width;
			visited[startPointCoord] = true;
			timeMap[startPoint.X, startPoint.Y] = currentTime;
			distanceMap[startPointCoord] = currentDistance;
			queue[queueHead++] = startPoint.X + startPoint.Y*GameParams.MapSize.Width;

			while (queueTail != queueHead)
			{
				int coord = queue[queueTail++];
				var currentPoint = new Point(coord%GameParams.MapSize.Width, coord/GameParams.MapSize.Width);
				currentDistance = distanceMap[coord];
				currentTime = timeMap[currentPoint.X, currentPoint.Y];
				foreach (var neighbor in currentPoint.GetNeighbors())
				{
					int neighborCoord = neighbor.X + neighbor.Y*GameParams.MapSize.Width;
					if (!GameParams.MapSize.ContainsPoint(neighbor) || visited[neighborCoord] ||
						coord == startPointCoord && neighbor == startPoint.MoveLogic(Direction.Value.GetOpposite()))
					{
						continue;
					}

					visited[neighborCoord] = true;
					distanceMap[neighborCoord] = currentDistance + 1;
					timeMap[neighbor.X, neighbor.Y] = currentTime + GameParams.CellSize/GetSpeed(currentDistance);
					queue[queueHead++] = neighborCoord;
				}
			}

			return timeMap;
		}

		public int GetSpeed(int depth)
		{
			if (NitroStepsLeft > depth && SlowdownStepsLeft <= depth)
			{
				return GameParams.NitroSpeed;
			}

			if (SlowdownStepsLeft > depth && NitroStepsLeft <= depth)
			{
				return GameParams.SlowDownSpeed;
			}

			return GameParams.Speed;
		}

		public int GetTimeForPath(int pathLength)
		{
			if (NitroStepsLeft == SlowdownStepsLeft)
			{
				return pathLength*GameParams.CellSize/GameParams.Speed;
			}

			int startChangedSpeed;
			int endChangedSpeed;
			int changedSpeed;
			if (NitroStepsLeft > SlowdownStepsLeft)
			{
				startChangedSpeed = SlowdownStepsLeft;
				endChangedSpeed = NitroStepsLeft;
				changedSpeed = GameParams.NitroSpeed;
			}
			else
			{
				startChangedSpeed = NitroStepsLeft;
				endChangedSpeed = SlowdownStepsLeft;
				changedSpeed = GameParams.SlowDownSpeed;
			}

			if (pathLength < startChangedSpeed)
			{
				return pathLength*GameParams.CellSize/GameParams.Speed;
			}

			if (pathLength < endChangedSpeed)
			{
				return startChangedSpeed*GameParams.CellSize/GameParams.Speed + (pathLength - startChangedSpeed)*GameParams.CellSize/changedSpeed;
			}

			return (startChangedSpeed + pathLength - endChangedSpeed)*GameParams.CellSize/GameParams.Speed + (endChangedSpeed - startChangedSpeed)*GameParams.CellSize/changedSpeed;
		}

		public int GetPathLengthForTime(int time)
		{
			if (NitroStepsLeft == SlowdownStepsLeft)
			{
				return time/(GameParams.CellSize/GameParams.Speed);
			}

			int startChangedSpeed;
			int endChangedSpeed;
			int changedSpeed;
			if (NitroStepsLeft > SlowdownStepsLeft)
			{
				startChangedSpeed = SlowdownStepsLeft;
				endChangedSpeed = NitroStepsLeft;
				changedSpeed = GameParams.NitroSpeed;
			}
			else
			{
				startChangedSpeed = NitroStepsLeft;
				endChangedSpeed = SlowdownStepsLeft;
				changedSpeed = GameParams.SlowDownSpeed;
			}

			int startChangedSpeedTime = startChangedSpeed*(GameParams.CellSize/GameParams.Speed);
			if (time <= startChangedSpeedTime)
			{
				return time/(GameParams.CellSize/GameParams.Speed);
			}

			int endChangedSpeedTime = startChangedSpeedTime + (endChangedSpeed - startChangedSpeedTime)*(GameParams.CellSize/changedSpeed);
			if (time <= endChangedSpeedTime)
			{
				return startChangedSpeed + (time - startChangedSpeedTime)/(GameParams.CellSize/changedSpeed);
			}

			return endChangedSpeedTime + (time - endChangedSpeedTime)/(GameParams.CellSize/GameParams.Speed);
		}

		private unsafe int[,] BuildOutsideTimeMap()
		{
			var timeMap = Game.GetNewMap<int>();
			Utils.FastCopyArray(Game.NoEnemiesDangerousMap, timeMap, GameParams.MapSize.Width*GameParams.MapSize.Height);
			var distanceMap = stackalloc int[GameParams.MapSize.Width*GameParams.MapSize.Height];
			var visited = stackalloc bool[GameParams.MapSize.Width*GameParams.MapSize.Height];
			var timeMapAfterHome = stackalloc int[GameParams.MapSize.Width*GameParams.MapSize.Height];
			var distanceMapAfterHome = stackalloc int[GameParams.MapSize.Width*GameParams.MapSize.Height];
			var visitedAfterHome = stackalloc bool[GameParams.MapSize.Width*GameParams.MapSize.Height];

			Point startPoint;
			int currentTime;
			int currentDistance;

			if (PathToNextPositionLength > 0)
			{
				startPoint = Position.MoveLogic(Direction.Value);
				currentTime = (GameParams.CellSize - PathToNextPositionLength)/GetSpeed(0);
				currentDistance = 1;
			}
			else
			{
				startPoint = Position;
				currentTime = 0;
				currentDistance = 0;
			}

			int startPointCoord = startPoint.X + startPoint.Y*GameParams.MapSize.Width;
			timeMap[startPoint.X, startPoint.Y] = currentTime;
			distanceMap[startPointCoord] = currentDistance;
			visited[startPointCoord] = true;

			var queue = new Queue<(Point Point, bool AfterHome, Direction? VisitHomeDirection)>(GameParams.MapSize.Width*GameParams.MapSize.Height);
			queue.Enqueue((startPoint, false, null));

			bool visitHome = false;
			while (queue.Count > 0)
			{
				(var currentPoint, bool afterHome, var visitHomeDirection) = queue.Dequeue();

				int currentCoord = currentPoint.X + currentPoint.Y*GameParams.MapSize.Width;

				if (!afterHome)
				{
					currentTime = timeMap[currentPoint.X, currentPoint.Y];
					currentDistance = distanceMap[currentCoord];
					foreach (var direction in EnumValues.GetAll<Direction>())
					{
						var neighbor = currentPoint.MoveLogic(direction);
						int neighborCoord = neighbor.X + neighbor.Y*GameParams.MapSize.Width;
						if (!GameParams.MapSize.ContainsPoint(neighbor) || Tail.AsPointsSet().Contains(neighbor))
						{
							continue;
						}

						if (Territory.Contains(neighbor) && !Territory.Contains(currentPoint) && !visitedAfterHome[neighborCoord])
						{
							queue.Enqueue((neighbor, true, direction));
							distanceMapAfterHome[neighborCoord] = currentDistance + 1;
							timeMapAfterHome[neighborCoord] = currentTime + GameParams.CellSize/GetSpeed(currentDistance);
							visitHome = true;
						}

						if (visited[neighborCoord])
						{
							continue;
						}

						distanceMap[neighborCoord] = currentDistance + 1;
						timeMap[neighbor.X, neighbor.Y] = currentTime + GameParams.CellSize/GetSpeed(currentDistance);
						visited[neighborCoord] = true;
						queue.Enqueue((neighbor, false, null));
					}
				}
				else
				{
					currentTime = timeMapAfterHome[currentCoord];
					currentDistance = distanceMapAfterHome[currentCoord];
					foreach (var direction in EnumValues.GetAll<Direction>())
					{
						var neighbor = currentPoint.MoveLogic(direction);
						int neighborCoord = neighbor.X + neighbor.Y*GameParams.MapSize.Width;
						if (!GameParams.MapSize.ContainsPoint(neighbor) || visitedAfterHome[neighborCoord] || visitHomeDirection == direction.GetOpposite())
						{
							continue;
						}

						distanceMapAfterHome[neighborCoord] = currentDistance + 1;
						timeMapAfterHome[neighborCoord] = currentTime + GameParams.CellSize/GetSpeed(currentDistance);
						visitedAfterHome[neighborCoord] = true;
						queue.Enqueue((neighbor, true, null));
					}
				}
			}

			if (!visitHome)
			{
				return timeMap;
			}

			for (int y = 0; y < GameParams.MapSize.Height; y++)
			{
				for (int x = 0; x < GameParams.MapSize.Width; x++)
				{
					timeMap[x, y] = Math.Min(timeMap[x, y], timeMapAfterHome[x + y*GameParams.MapSize.Width]);
				}
			}

			return timeMap;
		}
	}
}
