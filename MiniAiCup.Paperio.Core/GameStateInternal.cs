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

		private readonly Lazy<PointsSet> _allMapPoints;

		public PointsSet AllMapPoints => _allMapPoints.Value;

		private readonly Lazy<int[,]> _dangerousMap;

		public int[,] DangerousMap => _dangerousMap.Value;

		public DebugStateView DebugView => GetDebugView();

		private GameStateInternal(Size mapSize, int cellSize, int speed)
		{
			MapSize = mapSize;
			CellSize = cellSize;
			Speed = speed;

			_me = new Lazy<PlayerInternal>(() => Players.ContainsKey(Constants.MyId) ? Players[Constants.MyId] : null);
			_enemies = new Lazy<PlayerInternal[]>(() => Players.Values.Where(p => p.Id != Constants.MyId).ToArray());
			_allMapPoints = new Lazy<PointsSet>(() => MapSize.GetAllLogicPoints());
			_dangerousMap = new Lazy<int[,]>(BuildDangerousMap);
		}

		private int[,] BuildDangerousMap()
		{
			var map = new int[MapSize.Width, MapSize.Height];
			for (int y = 0; y < MapSize.Height; y++)
			{
				for (int x = 0; x < MapSize.Width; x++)
				{
					map[x, y] = Enemies.Min(e => e.DistanceMap[x, y]);
				}
			}

			return map;
		}

		private GameStateInternal(GameState state, Size mapSize, int cellSize, int speed) : this(mapSize, cellSize, speed)
		{
			TickNumber = state.TickNumber;
			Players = state.Players.Select(p => new PlayerInternal(p, mapSize, cellSize)).ToDictionary(p => p.Id);
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

		private DebugStateView GetDebugView()
		{
			return new DebugStateView {
				Size = MapSize,
				CellSize = CellSize,
				Players = Players.Values.Select(p => p.DebugView).ToArray()
			};
		}
	}
}
