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
			Territory = new PointsSet(player.Territory.Select(p => p.ConvertToLogic(Game.Params.CellSize)));
			Position = player.Position.ConvertToLogic(Game.Params.CellSize);
			Tail = new Path(player.Lines.Select(p => p.ConvertToLogic(Game.Params.CellSize)));
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
				Bonuses = Bonuses.Select(b => (ActiveBonusInfo)b.Clone()).ToArray(),
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
				for (int y = 0; y < Game.Params.MapLogicSize.Height; y++)
				{
					for (int x = 0; x < Game.Params.MapLogicSize.Width; x++)
					{
						map[x, y] = Position.GetDistanceTo(new Point(x, y));
					}
				}
			}
			else
			{
				var srcArray = Game.NoTailDistanceMaps[(int)Direction.Value];
				Utils.CopyArrayPart(srcArray, Game.Params.MapLogicSize.Width*2 -1, Game.Params.MapLogicSize.Height*2 - 1,
					map, Game.Params.MapLogicSize.Width, Game.Params.MapLogicSize.Height,
					Game.Params.MapLogicSize.Width - Position.X - 1, Game.Params.MapLogicSize.Height - Position.Y - 1);
			}

			return map;
		}

		private int[,] BuildOutsideDistanceMap()
		{
			var homePoints = new List<(Point Point, Direction SourceDirection, int PathLength)>();

			var map = Game.GetNewMap<int>();
			Utils.FastCopyArray(Game.NoEnemiesDangerousMap, map, Game.Params.MapLogicSize.Width*Game.Params.MapLogicSize.Height);

			var visited = Game.GetNewMap<bool>();

			map[Position.X, Position.Y] = 0;
			visited[Position.X, Position.Y] = true;

			var queue = new Queue<Point>(Game.Params.MapLogicSize.Width*Game.Params.MapLogicSize.Height);
			queue.Enqueue(Position);

			while (queue.Count > 0)
			{
				var currentPoint = queue.Dequeue();
				int currentLength = map[currentPoint.X, currentPoint.Y];
				foreach (var direction in EnumValues.GetAll<Direction>())
				{
					var neighbor = currentPoint.MoveLogic(direction);
					if (!Game.Params.MapLogicSize.ContainsPoint(neighbor) || Tail.AsPointsSet().Contains(neighbor))
					{
						continue;
					}

					if (Territory.Contains(neighbor) && !Territory.Contains(currentPoint))
					{
						homePoints.Add((neighbor, direction, currentLength + 1));
					}

					if (visited[neighbor.X, neighbor.Y])
					{
						continue;
					}

					map[neighbor.X, neighbor.Y] = currentLength + 1;
					visited[neighbor.X, neighbor.Y] = true;
					queue.Enqueue(neighbor);
				}
			}

			if (homePoints.Count == 0)
			{
				return map;
			}

			var mapAfterHome = Game.GetNewMap<int>();
			var visitedAfterHome = Game.GetNewMap<bool>();
			foreach (var tailPoint in Tail)
			{
				mapAfterHome[tailPoint.X, tailPoint.Y] = homePoints.Min(x => x.Point.GetDistanceTo(tailPoint, x.SourceDirection) + x.PathLength);
				visitedAfterHome[tailPoint.X, tailPoint.Y] = true;
				queue.Enqueue(tailPoint);
			}

			while (queue.Count > 0)
			{
				var currentPoint = queue.Dequeue();
				int currentLength = mapAfterHome[currentPoint.X, currentPoint.Y];
				foreach (var neighbor in currentPoint.GetNeighbors())
				{
					if (!Game.Params.MapLogicSize.ContainsPoint(neighbor) || visitedAfterHome[neighbor.X, neighbor.Y])
					{
						continue;
					}

					mapAfterHome[neighbor.X, neighbor.Y] = currentLength + 1;
					visitedAfterHome[neighbor.X, neighbor.Y] = true;
					queue.Enqueue(neighbor);
				}
			}

			for (int y = 0; y < Game.Params.MapLogicSize.Height; y++)
			{
				for (int x = 0; x < Game.Params.MapLogicSize.Width; x++)
				{
					map[x, y] = Math.Min(map[x, y], mapAfterHome[x, y]);
				}
			}

			return map;
		}
	}
}
