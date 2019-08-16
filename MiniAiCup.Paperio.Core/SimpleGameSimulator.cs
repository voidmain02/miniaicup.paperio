using System.Linq;
using MiniAiCup.Paperio.Core.Debug;

namespace MiniAiCup.Paperio.Core
{
	public class SimpleGameSimulator
	{
		private readonly BfsTerritoryCapturer _territoryCapturer;

		public SimpleGameSimulator(BfsTerritoryCapturer territoryCapturer)
		{
			_territoryCapturer = territoryCapturer;
		}

		public GameStateInternal Simulate(GameStateInternal state, int simulationTicks, Move move)
		{
#if DEBUG
			GameDebugData.Current.SimulationsCount++;
#endif

			int timeToNextPos = GameParams.CellSize/state.Me.GetSpeed(0);
			int nextTickNumber = state.TickNumber + timeToNextPos;
			var nextBonuses = state.Bonuses;

			var me = (PlayerInternal)state.Me.Clone();
			if (me.NitroStepsLeft > 0)
			{
				me.NitroStepsLeft--;
			}

			if (me.SlowdownStepsLeft > 0)
			{
				me.SlowdownStepsLeft--;
			}

			me.Direction = me.Direction?.GetMoved(move);
			if (me.Direction != null)
			{
				me.Position = me.Position.MoveLogic(me.Direction.Value);
			}

			if (!GameParams.MapSize.ContainsPoint(me.Position) || // Выехал за пределы карты
				me.Tail.Contains(me.Position)) // Наехал сам себе на хвост
			{
				return null;
			}

			var enemies = (PlayerInternal[])state.Enemies.Clone();

			if (me.Territory.Contains(me.Position))
			{
				if (me.Tail.Length > 0) // Заезд на свою территорию
				{
					if (enemies.Any(enemy => CheckIsCollisionPossible(me, enemy, state.Me.Position, simulationTicks, timeToNextPos) &&
						enemy.Tail.Length <= me.Tail.Length)) // Лобовое столкновение с противником с меньшим хвостом
					{
						return null;
					}

					var capturedTerritory = _territoryCapturer.Capture(me.Territory, me.Tail);
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
					return null;
				}
				if (me.Tail.Any(p => state.DangerousMap[p.X, p.Y] < simulationTicks + timeToNextPos + me.GetTimeForPath(me.PathToHome.Length))) // Потенциально могут наехать на мой хвост
				{
					return null;
				}
			}

			bool hasLosers = false;
			for (int i = 0; i < enemies.Length; i++)
			{
				var enemy = enemies[i];
				if (enemy.Tail.AsPointsSet().Contains(me.Position) && enemy.Territory.Count > 0 &&
					enemy.Territory.Min(p => enemy.TimeMap[p.X, p.Y]) > simulationTicks + 1) // Противник умирает, только если мы переехали его хвост и он гарантированно не успел вернуться домой
				{
					hasLosers = true;
					enemies[i] = null;
					me.Score += Constants.LineKillScore;
					break;
				}
			}

			enemies = hasLosers ? enemies.Where(e => e != null).ToArray() : enemies;

			return new GameStateInternal(nextTickNumber, me, enemies, nextBonuses, state, hasLosers ? null : state.DangerousMap);
		}

		private static bool CheckIsCollisionPossible(PlayerInternal me, PlayerInternal enemy, Point myPrevPosition, int simulationTicks, int timeForMove)
		{
			// TODO В будущем можно оптимизировать: вычислять соседей только один раз, а не для каждого врага
			for (int i = -1; i <= 1; i++)
			{
				var direction = (Direction)(((int)me.Direction.Value + i + 4)%4);
				var pointToCheck = me.Position.MoveLogic(direction);
				if (IsCollision(pointToCheck))
				{
					return true;
				}

				pointToCheck = myPrevPosition.MoveLogic(direction);
				if (IsCollision(pointToCheck))
				{
					return true;
				}
			}

			var prevPoint = myPrevPosition.MoveLogic(me.Direction.Value.GetOpposite());
			if (IsCollision(prevPoint))
			{
				return enemy.TimeMap[myPrevPosition.X, myPrevPosition.Y] - enemy.TimeMap[prevPoint.X, prevPoint.Y] < timeForMove;
			}

			return false;

			bool IsCollision(Point point)
			{
				return GameParams.MapSize.ContainsPoint(point) && enemy.TimeMap[point.X, point.Y] < simulationTicks + timeForMove;
			}
		}
	}
}
