using System;
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

		private PlayerInternal(string id, Size mapSize)
		{
			_mapSize = mapSize;
			_pathToHome = new Lazy<Path>(BuildPathToHome);

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
	}
}
