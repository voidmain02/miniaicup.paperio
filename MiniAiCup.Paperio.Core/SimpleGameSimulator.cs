using System;
using System.Collections.Generic;
using System.Linq;
using MiniAiCup.Paperio.Core.Debug;

namespace MiniAiCup.Paperio.Core
{
	public class SimpleGameSimulator
	{
		private readonly ITerritoryCapturer _territoryCapturer;

		public SimpleGameSimulator()
		{
			_territoryCapturer = new BfsTerritoryCapturer();
		}

		public GameStateInternal Simulate(GameStateInternal state, int currentDepth, Move move)
		{
#if DEBUG
			GameDebugData.Current.SimulationsCount++;
#endif

			int nextTickNumber = state.TickNumber + Game.Params.CellSize/Game.Params.Speed;
			var nextBonuses = state.Bonuses;

			var me = (PlayerInternal)state.Me.Clone();

			me.Direction = me.Direction?.GetMoved(move);
			if (me.Direction != null)
			{
				me.Position = me.Position.MoveLogic(me.Direction.Value);
			}

			if (!Game.Params.MapLogicSize.ContainsPoint(me.Position) || // Выехал за пределы карты
				me.Tail.Contains(me.Position)) // Наехал сам себе на хвост
			{
				return GetDeadState();
			}

			var enemies = (PlayerInternal[])state.Enemies.Clone();

			if (me.Territory.Contains(me.Position))
			{
				if (me.Tail.Length > 0) // Заезд на свою территорию
				{
					if (enemies.Any(enemy => (enemy.DistanceMap[state.Me.Position.X, state.Me.Position.Y] <= currentDepth + 1 ||
						enemy.DistanceMap[me.Position.X, me.Position.Y] <= currentDepth + 1) && enemy.Tail.Length <= me.Tail.Length)) // Лобовое столкновение с противником с меньшим хвостом
					{
						return GetDeadState();
					}

					var capturedTerritory = _territoryCapturer.Capture(me.Territory, me.Tail.Append(me.Position)); // TODO: Надо бы переделать Capturer, чтобы не приходилось добавлять в конец текущее положение
					me.Tail = Path.Empty;
					me.Territory = me.Territory.UnionWith(capturedTerritory);
					me.Score += capturedTerritory.Count*Constants.NeutralTerritoryScore;
					for (int i = 0; i < enemies.Length; i++)
					{
						var enemy = enemies[i];
						int srcCount = enemy.Territory.Count;
						var enemyCroppedTerritory = enemy.Territory.ExceptWith(capturedTerritory);
						int croppedCount = enemyCroppedTerritory.Count;
						if (srcCount != croppedCount)
						{
							var clonedEnemy = (PlayerInternal)enemy.Clone();
							clonedEnemy.Territory = enemyCroppedTerritory;
							enemies[i] = clonedEnemy;
							me.Score += (srcCount - croppedCount)*(Constants.EnemyTerritoryScore - Constants.NeutralTerritoryScore);
						}
					}
				}
			}
			else
			{
				me.Tail = me.Tail.Append(me.Position);
				if (me.PathToHome == null) // Зашел в тупик
				{
					return GetDeadState();
				}
				if (me.Tail.Any(p => state.DangerousMap[p.X, p.Y] <= currentDepth + me.PathToHome.Length)) // Потенциально могут наехать на мой хвост
				{
					return GetDeadState();
				}
			}

			var losers = new List<PlayerInternal>();
			foreach (var enemy in enemies)
			{
				if (enemy.Tail.AsPointsSet().Contains(me.Position) && enemy.Territory.Min(p => enemy.DistanceMap[p.X, p.Y]) > currentDepth)
				{
					losers.Add(enemy);
					me.Score += Constants.LineKillScore;
					break;
				}
			}

			var players = new PlayerInternal[enemies.Length - losers.Count + 1];
			players[0] = me;
			int index = 1;
			foreach (var enemy in enemies.Except(losers))
			{
				players[index++] = enemy;
			}

			return new GameStateInternal(nextTickNumber, players, nextBonuses, state, move, losers.Any() ? null : state.DangerousMap);

			GameStateInternal GetDeadState() => new GameStateInternal(nextTickNumber, state.Enemies, state.Bonuses, state, move, state.DangerousMap);
		}
	}
}
