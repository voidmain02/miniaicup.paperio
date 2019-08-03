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

		private readonly Lazy<PointsSet> _allMapPoints;

		public PointsSet AllMapPoints => _allMapPoints.Value;

		public DebugStateView DebugStateView => GetDebugStateView();

		private GameStateInternal(Size mapSize, int cellSize, int speed)
		{
			MapSize = mapSize;
			CellSize = cellSize;
			Speed = speed;

			_me = new Lazy<PlayerInternal>(() => Players.ContainsKey(Constants.MyId) ? Players[Constants.MyId] : null);
			_enemies = new Lazy<PlayerInternal[]>(() => Players.Values.Where(p => p.Id != Constants.MyId).ToArray());
			_pathToHome = new Lazy<Path>(BuildPathToHome);
			_allMapPoints = new Lazy<PointsSet>(() => MapSize.GetAllLogicPoints());
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

			_allMapPoints = previousState._allMapPoints;
		}

		public GameStateInternal(int tickNumber, IEnumerable<PlayerInternal> players, IEnumerable<BonusInfo> bonuses, GameStateInternal previousState, Move previousMove)
			: this(previousState.MapSize, previousState.CellSize, previousState.Speed)
		{
			TickNumber = tickNumber;
			Players = players.ToDictionary(p => p.Id);
			Bonuses = bonuses.ToArray();

			PreviousState = previousState;
			PreviousMove = previousMove;

			_allMapPoints = previousState._allMapPoints;
		}

		private static PlayerInternal BuildInternalPlayer(PlayerInfo player, int cellSize)
		{
			return new PlayerInternal {
				Id = player.Id,
				Score = player.Score,
				Territory = new PointsSet(player.Territory.Select(p => p.ConvertToLogic(cellSize))),
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
			return PathFinder.GetShortestPath(Me.Position, Me.Territory, Me.Tail.AsPointsSet(), MapSize);
		}

		private DebugStateView GetDebugStateView()
		{
			return new DebugStateView {
				Size = MapSize,
				CellSize = CellSize,
				Players = Players.Values.Select(GetDebugPlayerView).ToArray()
			};
		}

		private static DebugPlayerView GetDebugPlayerView(PlayerInternal player)
		{
			return new DebugPlayerView {
				Id = player.Id,
				Territory = player.Territory.ToArray(),
				Tail = player.Tail.ToArray(),
				Position = player.Position
			};
		}
	}
}
