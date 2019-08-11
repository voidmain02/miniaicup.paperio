using System.Linq;
using MiniAiCup.Paperio.Core.Debug;

namespace MiniAiCup.Paperio.Core
{
	public class GameStateInternal
	{
		public BonusInfo[] Bonuses { get; }

		public int TickNumber { get; }

		public GameStateInternal PreviousState { get; }

		public PlayerInternal Me { get; }

		public PlayerInternal[] Enemies { get; }

		private int[,] _dangerousMap;

		public int[,] DangerousMap => _dangerousMap ?? (_dangerousMap = BuildDangerousMap());

		public DebugStateView DebugView => GetDebugView();

		public GameStateInternal(GameState state)
		{
			TickNumber = state.TickNumber;

			Enemies = new PlayerInternal[state.Players.Length - 1];
			int enemyIndex = 0;

			foreach (var player in state.Players)
			{
				if (player.Id == Constants.MyId)
				{
					Me = new PlayerInternal(player);
				}
				else
				{
					Enemies[enemyIndex++] = new PlayerInternal(player);
				}
			}

			Bonuses = state.Bonuses.Select(b => new BonusInfo {
				Type = b.Type,
				Position = b.Position.ConvertToLogic(Game.Params.CellSize)
			}).ToArray();

			PreviousState = null;
		}

		public GameStateInternal(GameState state, GameStateInternal previousState) : this(state)
		{
			PreviousState = previousState;
		}

		public GameStateInternal(int tickNumber, PlayerInternal me, PlayerInternal[] enemies, BonusInfo[] bonuses, GameStateInternal previousState, int[,] dangerousMap)
		{
			_dangerousMap = dangerousMap;

			TickNumber = tickNumber;
			Me = me;
			Enemies = enemies;
			Bonuses = bonuses;

			PreviousState = previousState;
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
				Players = Enemies.Append(Me).Select(p => p.DebugView).ToArray()
			};
		}
	}
}
