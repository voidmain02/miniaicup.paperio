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

		public Path Tail { get; set; }

		public ActiveBonusInfo[] Bonuses { get; set; }

		public Direction? Direction { get; set; }

		private readonly Lazy<Path> _pathToHome;

		public Path PathToHome => _pathToHome.Value;

		public DebugPlayerView DebugView => GetDebugView();

		private readonly Lazy<int[,]> _distanceMap;

		public int[,] DistanceMap => _distanceMap.Value;

		private PlayerInternal(string id)
		{
			_pathToHome = new Lazy<Path>(BuildPathToHome);
			_distanceMap = new Lazy<int[,]>(BuildDistanceMap);

			Id = id;
		}

		public PlayerInternal(PlayerInfo player) : this(player.Id)
		{
			Score = player.Score;
			Territory = new PointsSet(player.Territory.Select(p => p.ConvertToLogic(GameParams.CellSize)));
			Position = player.Position.ConvertToLogic(GameParams.CellSize);
			Tail = new Path(player.Lines.Select(p => p.ConvertToLogic(GameParams.CellSize)));
			Bonuses = player.Bonuses;
			Direction = player.Direction;
		}

		private DebugPlayerView GetDebugView()
		{
			return new DebugPlayerView {
				Id = Id,
				Territory = Territory.ToArray(),
				Tail = Tail.ToArray(),
				Position = Position
			};
		}

		public object Clone()
		{
			return new PlayerInternal(Id) {
				Score = Score,
				Territory = Territory,
				Position = Position,
				Tail = Tail,
				Bonuses = Bonuses,
				Direction = Direction
			};
		}

		private Path BuildPathToHome()
		{
			return PathFinder.GetShortestPath(Position, Territory, Tail.AsPointsSet());
		}

		private int[,] BuildDistanceMap()
		{
			return Tail.Length > 1 ? BuildOutsideDistanceMap() : BuildInsideDistanceMap();
		}

		private int[,] BuildInsideDistanceMap()
		{
			var map = Game.GetNewMap<int>();
			if (Direction == null)
			{
				for (int y = 0; y < GameParams.MapSize.Height; y++)
				{
					for (int x = 0; x < GameParams.MapSize.Width; x++)
					{
						map[x, y] = Position.GetDistanceTo(new Point(x, y));
					}
				}
			}
			else
			{
				var srcArray = Game.NoTailDistanceMaps[(int)Direction.Value];
				Utils.CopyArrayPart(srcArray, GameParams.MapSize.Width*2 -1, GameParams.MapSize.Height*2 - 1,
					map, GameParams.MapSize.Width, GameParams.MapSize.Height,
					GameParams.MapSize.Width - Position.X - 1, GameParams.MapSize.Height - Position.Y - 1);
			}

			return map;
		}

		private unsafe int[,] BuildOutsideDistanceMap()
		{
			var map = Game.GetNewMap<int>();
			Utils.FastCopyArray(Game.NoEnemiesDangerousMap, map, GameParams.MapSize.Width*GameParams.MapSize.Height);
			var visited = stackalloc bool[GameParams.MapSize.Width*GameParams.MapSize.Height];
			var mapAfterHome = stackalloc int[GameParams.MapSize.Width*GameParams.MapSize.Height];
			var visitedAfterHome = stackalloc bool[GameParams.MapSize.Width*GameParams.MapSize.Height];

			map[Position.X, Position.Y] = 0;
			visited[Position.X + Position.Y*GameParams.MapSize.Width] = true;

			var queue = new Queue<(Point Point, bool AfterHome, Direction? VisitHomeDirection)>(GameParams.MapSize.Width*GameParams.MapSize.Height);
			queue.Enqueue((Position, false, null));

			bool visitHome = false;
			while (queue.Count > 0)
			{
				(var currentPoint, bool afterHome, var visitHomeDirection) = queue.Dequeue();

				if (!afterHome)
				{
					int currentLength = map[currentPoint.X, currentPoint.Y];
					foreach (var direction in EnumValues.GetAll<Direction>())
					{
						var neighbor = currentPoint.MoveLogic(direction);
						if (!GameParams.MapSize.ContainsPoint(neighbor) || Tail.AsPointsSet().Contains(neighbor))
						{
							continue;
						}

						if (Territory.Contains(neighbor) && !Territory.Contains(currentPoint) && !visitedAfterHome[neighbor.X + neighbor.Y*GameParams.MapSize.Width])
						{
							queue.Enqueue((neighbor, true, direction));
							mapAfterHome[neighbor.X + neighbor.Y*GameParams.MapSize.Width] = currentLength + 1;
							visitHome = true;
						}

						if (visited[neighbor.X + neighbor.Y*GameParams.MapSize.Width])
						{
							continue;
						}

						map[neighbor.X, neighbor.Y] = currentLength + 1;
						visited[neighbor.X + neighbor.Y*GameParams.MapSize.Width] = true;
						queue.Enqueue((neighbor, false, null));
					}
				}
				else
				{
					int currentLength = mapAfterHome[currentPoint.X + currentPoint.Y*GameParams.MapSize.Width];
					foreach (var direction in EnumValues.GetAll<Direction>())
					{
						var neighbor = currentPoint.MoveLogic(direction);
						if (!GameParams.MapSize.ContainsPoint(neighbor) || visitedAfterHome[neighbor.X + neighbor.Y*GameParams.MapSize.Width] || visitHomeDirection == direction.GetOpposite())
						{
							continue;
						}

						mapAfterHome[neighbor.X + neighbor.Y*GameParams.MapSize.Width] = currentLength + 1;
						visitedAfterHome[neighbor.X + neighbor.Y*GameParams.MapSize.Width] = true;
						queue.Enqueue((neighbor, true, null));
					}
				}
			}

			if (!visitHome)
			{
				return map;
			}

			for (int y = 0; y < GameParams.MapSize.Height; y++)
			{
				for (int x = 0; x < GameParams.MapSize.Width; x++)
				{
					map[x, y] = Math.Min(map[x, y], mapAfterHome[x + y*GameParams.MapSize.Width]);
				}
			}

			return map;
		}
	}
}
