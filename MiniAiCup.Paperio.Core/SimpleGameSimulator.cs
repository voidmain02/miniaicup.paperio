using System.Collections.Generic;
using System.Linq;

namespace MiniAiCup.Paperio.Core
{
	public class SimpleGameSimulator
	{
		private readonly Size _mapSize;
		private readonly ITerritoryCapturer _territoryCapturer;

		public SimpleGameSimulator(Size mapSize)
		{
			_mapSize = mapSize;
			_territoryCapturer = new BfsTerritoryCapturer(mapSize);
		}

		public GameStateInternal Simulate(GameStateInternal state, int currentDepth, Move move)
		{
			int nextTickNumber = state.TickNumber + state.CellSize/state.Speed;
			var nextBonuses = state.Bonuses;

			var me = (PlayerInternal)state.Me.Clone();
			var enemies = new List<PlayerInternal>(state.Enemies);
			MovePlayer(me, move);

			if (!_mapSize.ContainsPoint(me.Position) || // Выехал за пределы карты
				me.Tail.Contains(me.Position)) // Наехал сам себе на хвост
			{
				return new GameStateInternal(nextTickNumber, enemies, nextBonuses, state, move);
			}

			if (me.Territory.Contains(me.Position))
			{
				if (me.Tail.Length > 0)
				{
					var capturedTerritory = _territoryCapturer.Capture(me.Territory, me.Tail.Append(me.Position)); // TODO: Надо бы переделать Capturer, чтобы не приходилось добавлять в конец текущее положение
					me.Tail = Path.Empty;
					me.Territory = me.Territory.UnionWith(capturedTerritory);
					me.Score += capturedTerritory.Count*Constants.NeutralTerritoryScore;
					foreach (var enemy in enemies)
					{
						int srcCount = enemy.Territory.Count;
						enemy.Territory = enemy.Territory.ExceptWith(capturedTerritory);
						int croppedCount = enemy.Territory.Count;
						me.Score += (srcCount - croppedCount)*(Constants.EnemyTerritoryScore - Constants.NeutralTerritoryScore);
					}
				}
			}
			else
			{
				me.Tail = me.Tail.Append(me.Position);
				if (me.PathToHome == null) // Зашел в тупик
				{
					return new GameStateInternal(nextTickNumber, enemies, nextBonuses, state, move);
				}
				if (me.Tail.Any(p => state.DangerousMap[p.X, p.Y] <= currentDepth + me.PathToHome.Length + 1)) // Потенциально могут наехать на мой хвост
				{
					return new GameStateInternal(nextTickNumber, enemies, nextBonuses, state, move);
				}
			}

			var losers = new List<PlayerInternal>();
			foreach (var enemy in enemies)
			{
				if (enemy.Tail.AsPointsSet().Contains(me.Position))
				{
					losers.Add(enemy);
					me.Score += Constants.LineKillScore;
				}
			}

			enemies.Add(me);
			var players = enemies.Except(losers);

			return new GameStateInternal(nextTickNumber, players, nextBonuses, state, move);
		}

		private static void MovePlayer(PlayerInternal player, Move move)
		{
			var nextDirection = player.Direction?.GetMoved(move);
			var nextPos = nextDirection == null
				? player.Position
				: player.Position.MoveLogic(nextDirection.Value);

			player.Direction = nextDirection;
			player.Position = nextPos;
		}
	}
}
