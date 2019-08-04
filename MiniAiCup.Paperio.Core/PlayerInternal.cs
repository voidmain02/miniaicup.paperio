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
				foreach (var neighbor in currentPoint.GetNeighbors())
				{
					if (!_mapSize.ContainsPoint(neighbor) || visited[neighbor.X, neighbor.Y] || Tail.AsPointsSet().Contains(neighbor))
					{
						continue;
					}

					map[neighbor.X, neighbor.Y] = currentLength + 1;
					visited[neighbor.X, neighbor.Y] = true;
					queue.Enqueue(neighbor);
				}
			}

			return map;
		}
	}
}
