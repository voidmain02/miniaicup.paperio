using System;
using System.Collections.Generic;
using System.Linq;

namespace MiniAiCup.Paperio.Core
{
	public class GameStateInternal
	{
		public Size MapSize { get; }

		public int CellSize { get; }

		public int Speed { get; }

		public BonusInfo[] Bonuses { get; }

		public int TickNumber { get; }

		public Dictionary<string, PlayerInternal> Players { get; }

		public GameStateInternal PreviousState { get; }

		public Move PreviousMove { get; }

		private readonly Lazy<PlayerInternal> _me;

		public PlayerInternal Me => _me.Value;

		private readonly Lazy<PlayerInternal[]> _enemies;

		public PlayerInternal[] Enemies => _enemies.Value;

		private readonly Lazy<Path> _pathToHome;

		public Path PathToHome => _pathToHome.Value;

		private GameStateInternal(Size mapSize, int cellSize, int speed)
		{
			MapSize = mapSize;
			CellSize = cellSize;
			Speed = speed;

			_me = new Lazy<PlayerInternal>(() => Players.ContainsKey(Constants.MyId) ? Players[Constants.MyId] : null);
			_enemies = new Lazy<PlayerInternal[]>(() => Players.Values.Where(p => p.Id != Constants.MyId).ToArray());
			_pathToHome = new Lazy<Path>(BuildPathToHome);
		}

		private GameStateInternal(GameState state, Size mapSize, int cellSize, int speed) : this(mapSize, cellSize, speed)
		{
			TickNumber = state.TickNumber;
			Players = state.Players.Select(p => BuildInternalPlayer(p, CellSize)).ToDictionary(p => p.Id);
			Bonuses = state.Bonuses.Select(b => new BonusInfo {
				Type = b.Type,
				Position = b.Position.ConvertToLogic(CellSize)
			}).ToArray();
		}

		public GameStateInternal(GameState state, GameParams gameParams) : this(state, gameParams.MapLogicSize, gameParams.CellSize, gameParams.Speed)
		{
			PreviousMove = Move.Forward;
			PreviousState = null;
		}

		public GameStateInternal(GameState state, GameStateInternal previousState, Move previousMove) : this(state, previousState.MapSize, previousState.CellSize, previousState.Speed)
		{
			PreviousMove = previousMove;
			PreviousState = previousState;
		}

		public GameStateInternal(int tickNumber, IEnumerable<PlayerInternal> players, IEnumerable<BonusInfo> bonuses, GameStateInternal previousState, Move previousMove)
			: this(previousState.MapSize, previousState.CellSize, previousState.Speed)
		{
			TickNumber = tickNumber;
			Players = players.ToDictionary(p => p.Id);
			Bonuses = bonuses.ToArray();

			PreviousState = previousState;
			PreviousMove = previousMove;
		}

		private static PlayerInternal BuildInternalPlayer(PlayerInfo player, int cellSize)
		{
			return new PlayerInternal {
				Id = player.Id,
				Score = player.Score,
				Territory = new HashSet<Point>(player.Territory.Select(p => p.ConvertToLogic(cellSize))),
				Position = player.Position.ConvertToLogic(cellSize),
				Tail = new Path(player.Lines.Select(p => p.ConvertToLogic(cellSize))),
				Bonuses = player.Bonuses,
				Direction = player.Direction
			};
		}

		private Path BuildPathToHome()
		{
			if (Me == null)
			{
				return null;
			}
			return PathFinder.GetShortestPath(Me.Position, Me.Territory, Me.Tail.HashSet, MapSize);
		}

		public int Score()
		{
			if (Me == null)
			{
				return -1000;
			}

			if (Me.Direction == null)
			{
				return -800;
			}

			if (Me.Territory.Count == MapSize.Width*MapSize.Height)
			{
				return 0;
			}

			if (Me.Tail.Length == 0)
			{
				var freeTerritory = new HashSet<Point>(GetAllPoints(MapSize));
				freeTerritory.ExceptWith(Me.Territory);
				var obstacles = new HashSet<Point> { Me.Position.MoveLogic(Me.Direction.Value.GetOpposite()) };
				var pathToOutside = PathFinder.GetShortestPath(Me.Position, freeTerritory, obstacles, MapSize);

				int pathToOutsidePenalty = 1 - pathToOutside.Length;
				int backToHomeBonus = PreviousState?.Me.Tail.Length ?? 0;
				return backToHomeBonus + pathToOutsidePenalty;
			}

			if (PathToHome == null)
			{
				return -900;
			}

			var myTailWithShortestPathToHome = new HashSet<Point>(Me.Tail);
			myTailWithShortestPathToHome.UnionWith(PathToHome.Take(PathToHome.Length - 1));
			int minPathFromEnemyToMyTail = Enemies.Length == 0
				? Int32.MaxValue
				: Enemies.Select(e => PathFinder.GetShortestPath(e.Position, myTailWithShortestPathToHome, e.Tail.HashSet, MapSize)?.Length ?? Int32.MaxValue).Min() - 1;

			if (minPathFromEnemyToMyTail <= PathToHome.Length)
			{
				return (minPathFromEnemyToMyTail - PathToHome.Length - 2)*10;
			}

			int outsideBonus = 10;
			int longPathPenalty = Enemies.Length > 0 ? Math.Min(20 - Me.Tail.Length, 0) : 0;
			int longPathToHomePenalty = Enemies.Length > 0 ? Math.Min(6 - PathToHome.Length, 0) : 0;
			int forwardMoveBonus = PreviousMove == Move.Forward ? 1 : 0;
			int movesLeft = (Constants.MaxTickCount - TickNumber)/(CellSize/Speed);
			int notEnoughTimePenalty = Math.Min((movesLeft - PathToHome.Length)*10, 0);
			return outsideBonus + longPathPenalty + longPathToHomePenalty + forwardMoveBonus + notEnoughTimePenalty;
		}

		public static Point[] GetAllPoints(Size size)
		{
			var points = new Point[size.Width*size.Height];
			for (int y = 0; y < size.Height; y++)
			{
				for (int x = 0; x < size.Width; x++)
				{
					points[size.Width*y + x] = new Point(x, y);
				}
			}

			return points;
		}
	}
}
