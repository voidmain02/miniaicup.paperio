using System;
using System.Collections.Generic;
using System.Linq;

namespace MiniAiCup.Paperio.Core
{
	public class PlayerInternal : ICloneable
	{
		private readonly Size _mapSize;

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

		private PlayerInternal(string id, Size mapSize)
		{
			_mapSize = mapSize;
			_pathToHome = new Lazy<Path>(BuildPathToHome);
			_distanceMap = new Lazy<int[,]>(BuildDistanceMap);

			Id = id;
		}

		public PlayerInternal(PlayerInfo player, Size mapSize, int cellSize) : this(player.Id, mapSize)
		{
			Score = player.Score;
			Territory = new PointsSet(player.Territory.Select(p => p.ConvertToLogic(cellSize)));
			Position = player.Position.ConvertToLogic(cellSize);
			Tail = new Path(player.Lines.Select(p => p.ConvertToLogic(cellSize)));
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
			return new PlayerInternal(Id, _mapSize) {
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
			return PathFinder.GetShortestPath(Position, Territory, Tail.AsPointsSet(), _mapSize);
		}

		private int[,] BuildDistanceMap()
		{
			return Tail.Length > 1 ? BuildOutsideDistanceMap() : BuildInsideDistanceMap();
		}

		private int[,] BuildInsideDistanceMap()
		{
			var map = new int[_mapSize.Width, _mapSize.Height];
			for (int y = 0; y < _mapSize.Height; y++)
			{
				for (int x = 0; x < _mapSize.Width; x++)
				{
					map[x, y] = GetDistanceBetweenPoints(Position, new Point(x, y));
				}
			}

			if (Direction == null)
			{
				return map;
			}

			switch (Direction.Value)
			{
				case Core.Direction.Left:
					for (int x = Position.X + 1; x < _mapSize.Width; x++)
					{
						map[x, Position.Y] += 2;
					}
					break;
				case Core.Direction.Up:
					for (int y = Position.Y - 1; y >= 0; y--)
					{
						map[Position.X, y] += 2;
					}
					break;
				case Core.Direction.Right:
					for (int x = Position.X - 1; x >= 0; x--)
					{
						map[x, Position.Y] += 2;
					}
					break;
				case Core.Direction.Down:
					for (int y = Position.X + 1; y < _mapSize.Height; y++)
					{
						map[Position.X, y] += 2;
					}
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			return map;
		}

		private int[,] BuildOutsideDistanceMap()
		{
			var homePoints = new List<(Point Point, Direction SourceDirection, int PathLength)>();

			var map = new int[_mapSize.Width, _mapSize.Height];
			for (int y = 0; y < _mapSize.Height; y++)
			{
				for (int x = 0; x < _mapSize.Width; x++)
				{
					map[x, y] = Int32.MaxValue;
				}
			}
			var visited = new bool[_mapSize.Width, _mapSize.Height];

			map[Position.X, Position.Y] = 0;
			visited[Position.X, Position.Y] = true;

			var queue = new Queue<Point>(_mapSize.Width*_mapSize.Height);
			queue.Enqueue(Position);

			while (queue.Count > 0)
			{
				var currentPoint = queue.Dequeue();
				int currentLength = map[currentPoint.X, currentPoint.Y];
				foreach (var direction in EnumValues.GetAll<Direction>())
				{
					var neighbor = currentPoint.MoveLogic(direction);
					if (!_mapSize.ContainsPoint(neighbor) || Tail.AsPointsSet().Contains(neighbor))
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

			var mapAfterHome = new int[_mapSize.Width, _mapSize.Height];
			var visitedAfterHome = new bool[_mapSize.Width, _mapSize.Height];
			foreach (var tailPoint in Tail)
			{
				mapAfterHome[tailPoint.X, tailPoint.Y] = homePoints.Min(x => GetDistanceBetweenPoints(x.Point, tailPoint, x.SourceDirection) + x.PathLength);
				visitedAfterHome[tailPoint.X, tailPoint.Y] = true;
				queue.Enqueue(tailPoint);
			}

			while (queue.Count > 0)
			{
				var currentPoint = queue.Dequeue();
				int currentLength = mapAfterHome[currentPoint.X, currentPoint.Y];
				foreach (var neighbor in currentPoint.GetNeighbors())
				{
					if (!_mapSize.ContainsPoint(neighbor) || visitedAfterHome[neighbor.X, neighbor.Y])
					{
						continue;
					}

					mapAfterHome[neighbor.X, neighbor.Y] = currentLength + 1;
					visitedAfterHome[neighbor.X, neighbor.Y] = true;
					queue.Enqueue(neighbor);
				}
			}

			for (int y = 0; y < _mapSize.Height; y++)
			{
				for (int x = 0; x < _mapSize.Width; x++)
				{
					map[x, y] = Math.Min(map[x, y], mapAfterHome[x, y]);
				}
			}

			return map;
		}

		private static int GetDistanceBetweenPoints(Point src, Point dst)
		{
			return Math.Abs(src.X - dst.X) + Math.Abs(src.Y - dst.Y);
		}

		private static int GetDistanceBetweenPoints(Point src, Point dst, Direction? direction)
		{
			int distance = GetDistanceBetweenPoints(src, dst);

			if (dst.X == src.X && (dst.Y > src.Y && direction == Core.Direction.Down || dst.Y < src.Y && direction == Core.Direction.Up) ||
				dst.Y == src.Y && (dst.X > src.X && direction == Core.Direction.Left || dst.X < src.X && direction == Core.Direction.Right))
			{
				distance += 2;
			}

			return distance;
		}
	}
}
