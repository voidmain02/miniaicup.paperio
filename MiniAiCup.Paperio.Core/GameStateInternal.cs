using System;
using System.Collections.Generic;
using System.Linq;

namespace MiniAiCup.Paperio.Core
{
	public class GameStateInternal
	{
		public Size MapSize { get; private set; }

		public int CellSize { get; private set; }

		public int Speed { get; private set; }

		public BonusInfo[] Bonuses { get; private set; }

		public int TickNumber { get; private set; }

		public Dictionary<string, PlayerInternal> Players { get; private set; }

		public GameStateInternal PreviousState { get; private set; }

		public Move PreviousMove { get; private set; }

		private readonly Lazy<PlayerInternal> _me;

		public PlayerInternal Me => _me.Value;

		private readonly Lazy<PlayerInternal[]> _enemies;

		public PlayerInternal[] Enemies => _enemies.Value;

		private readonly Lazy<Point[]> _pathToHome;

		public Point[] PathToHome => _pathToHome.Value;

		private GameStateInternal()
		{
			_me = new Lazy<PlayerInternal>(() => Players.ContainsKey(Constants.MyId) ? Players[Constants.MyId] : null);
			_enemies = new Lazy<PlayerInternal[]>(() => Players.Values.Where(p => p.Id != Constants.MyId).ToArray());
			_pathToHome = new Lazy<Point[]>(BuildPathToHome);
		}

		public GameStateInternal(GameState state, GameParams gameParams) : this()
		{
			PreviousMove = Move.Forward;
			MapSize = gameParams.MapLogicSize;
			CellSize = gameParams.CellSize;
			Speed = gameParams.Speed;
			ApplyState(state);
		}

		public GameStateInternal(GameState state, GameStateInternal previousState, Move previousMove) : this()
		{
			PreviousMove = previousMove;
			PreviousState = previousState;
			MapSize = previousState.MapSize;
			CellSize = previousState.CellSize;
			Speed = previousState.Speed;
			ApplyState(state);
		}

		private void ApplyState(GameState state)
		{
			TickNumber = state.TickNumber;
			Bonuses = state.Bonuses.Select(b => new BonusInfo {
				Type = b.Type,
				Position = b.Position.ConvertToLogic(CellSize)
			}).ToArray();
			Players = state.Players.Select(p => BuildInternalPlayer(p, CellSize)).ToDictionary(p => p.Id);
		}

		private static PlayerInternal BuildInternalPlayer(PlayerInfo player, int cellSize)
		{
			return new PlayerInternal {
				Id = player.Id,
				Score = player.Score,
				Territory = new HashSet<Point>(player.Territory.Select(p => p.ConvertToLogic(cellSize))),
				Position = player.Position.ConvertToLogic(cellSize),
				Lines = new HashSet<Point>(player.Lines.Select(p => p.ConvertToLogic(cellSize))),
				Bonuses = player.Bonuses,
				Direction = player.Direction
			};
		}

		private Point[] BuildPathToHome()
		{
			if (Me == null)
			{
				return null;
			}
			return PathFinder.GetShortestPath(Me.Position, Me.Territory, Me.Lines, MapSize);
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

			if (Me.Lines.Count == 0)
			{
				var freeTerritory = new HashSet<Point>(GetAllPoints(MapSize));
				freeTerritory.ExceptWith(Me.Territory);
				var obstacles = new HashSet<Point> { Me.Position.MoveLogic(Me.Direction.Value.GetOpposite()) };
				var pathToOutside = PathFinder.GetShortestPath(Me.Position, freeTerritory, obstacles, MapSize);

				int pathToOutsidePenalty = 1 - pathToOutside.Length;
				int backToHomeBonus = PreviousState?.Me.Lines.Count ?? 0;
				return backToHomeBonus + pathToOutsidePenalty;
			}

			if (PathToHome == null)
			{
				return -900;
			}

			var myLinesWithShortestPathToHome = new HashSet<Point>(Me.Lines);
			myLinesWithShortestPathToHome.UnionWith(PathToHome.Take(PathToHome.Length - 1));
			int minPathFromEnemyToMyLines = Enemies.Length == 0
				? Int32.MaxValue
				: Enemies.Select(e => PathFinder.GetShortestPath(e.Position, myLinesWithShortestPathToHome, e.Lines, MapSize)?.Length ?? Int32.MaxValue).Min() - 1;

			if (minPathFromEnemyToMyLines <= PathToHome.Length)
			{
				return (minPathFromEnemyToMyLines - PathToHome.Length - 2)*10;
			}

			int outsideBonus = 10;
			int longPathPenalty = Enemies.Length > 0 ? Math.Min(20 - Me.Lines.Count, 0) : 0;
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

		public GameStateInternal Simulate(Move move)
		{
			return new GameStateInternal {
				PreviousMove = move,
				PreviousState = this,
				MapSize = MapSize,
				CellSize = CellSize,
				Speed = Speed,
				TickNumber = TickNumber + Speed,
				Players = Players.Values.Select(p => p == Me ? Simulate(Me, move) : p).Where(p => p != null).ToDictionary(p => p.Id)
			};
		}

		private PlayerInternal Simulate(PlayerInternal player, Move move)
		{
			var nextDirection = player.Direction?.GetMoved(move);
			var nextPos = nextDirection == null
				? player.Position
				: player.Position.MoveLogic(nextDirection.Value);

			if (!MapSize.ContainsPoint(nextPos))
			{
				return null;
			}

			if (Players[player.Id].Lines.Contains(nextPos))
			{
				return null;
			}

			var lines = !player.Territory.Contains(nextPos)
				? new HashSet<Point>(player.Lines) { nextPos }
				: new HashSet<Point>();

			return new PlayerInternal {
				Direction = nextDirection,
				Lines = lines,
				Position = nextPos,
				Territory = new HashSet<Point>(player.Territory),
				Id = player.Id,
				Bonuses = player.Bonuses,
				Score = player.Score
			};
		}
	}
}
