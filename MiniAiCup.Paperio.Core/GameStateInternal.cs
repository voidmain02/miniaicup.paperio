using System;
using System.Collections.Generic;
using System.Linq;

namespace MiniAiCup.Paperio.Core
{
	public class GameStateInternal
	{
		public BonusInfo[] Bonuses { get; }

		public int TickNumber { get; }

		public Dictionary<string, PlayerInternal> Players { get; }

		public GameStateInternal PreviousState { get; }

		public Move PreviousMove { get; }

		private readonly Lazy<PlayerInternal> _me;

		public PlayerInternal Me => _me.Value;

		private readonly Lazy<PlayerInternal[]> _enemies;

		public PlayerInternal[] Enemies => _enemies.Value;

		private readonly Lazy<int[,]> _dangerousMap;

		public int[,] DangerousMap => _dangerousMap.Value;

		public DebugStateView DebugView => GetDebugView();

		private GameStateInternal()
		{
			_me = new Lazy<PlayerInternal>(() => Players.ContainsKey(Constants.MyId) ? Players[Constants.MyId] : null);
			_enemies = new Lazy<PlayerInternal[]>(() => Players.Values.Where(p => p.Id != Constants.MyId).ToArray());
			_dangerousMap = new Lazy<int[,]>(BuildDangerousMap);
		}

		public GameStateInternal(GameState state) : this()
		{
			TickNumber = state.TickNumber;
			Players = state.Players.Select(p => new PlayerInternal(p)).ToDictionary(p => p.Id);
			Bonuses = state.Bonuses.Select(b => new BonusInfo {
				Type = b.Type,
				Position = b.Position.ConvertToLogic(Game.Params.CellSize)
			}).ToArray();

			PreviousMove = Move.Forward;
			PreviousState = null;
		}

		public GameStateInternal(GameState state, GameStateInternal previousState, Move previousMove) : this(state)
		{
			PreviousMove = previousMove;
			PreviousState = previousState;
		}

		public GameStateInternal(int tickNumber, IEnumerable<PlayerInternal> players, IEnumerable<BonusInfo> bonuses, GameStateInternal previousState, Move previousMove) : this()
		{
			TickNumber = tickNumber;
			Players = players.ToDictionary(p => p.Id);
			Bonuses = bonuses.ToArray();

			PreviousState = previousState;
			PreviousMove = previousMove;
		}

		private int[,] BuildDangerousMap()
		{
			if (!Enemies.Any())
			{
				return Game.NoEnemiesDangerousMap;
			}

			var map = Game.GetNewMap<int>();

			for (int y = 0; y < Game.Params.MapLogicSize.Height; y++)
			{
				for (int x = 0; x < Game.Params.MapLogicSize.Width; x++)
				{
					map[x, y] = Enemies.Min(e => e.DistanceMap[x, y]);
				}
			}

			return map;
		}

		private DebugStateView GetDebugView()
		{
			return new DebugStateView {
				Size = Game.Params.MapLogicSize,
				CellSize = Game.Params.CellSize,
				Players = Players.Values.Select(p => p.DebugView).ToArray()
			};
		}
	}
}
