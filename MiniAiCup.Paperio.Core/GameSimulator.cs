using System.Collections.Generic;
using System.Linq;

namespace MiniAiCup.Paperio.Core
{
	public class GameSimulator
	{
		private Size _mapSize;

		private List<PlayerInternal> _players;

		private readonly IEnemyStrategy _enemyStrategy;

		public GameSimulator(IEnemyStrategy enemyStrategy = null)
		{
			_enemyStrategy = enemyStrategy;
		}

		public GameStateInternal Simulate(GameStateInternal state, Move move)
		{
			_mapSize = state.MapSize;

			int tickNumber = state.TickNumber + state.Speed;
			var bonuses = state.Bonuses;

			_players = new List<PlayerInternal>();
			foreach (var srcPlayer in state.Players.Values)
			{
				var player = ClonePlayer(srcPlayer);
				_players.Add(player);

				if (srcPlayer == state.Me)
				{
					MovePlayer(player, move);
				}
				else if (_enemyStrategy != null)
				{
					var enemyMove = _enemyStrategy.GetMove(state, srcPlayer);
					MovePlayer(player, enemyMove);
				}
			}

			var capturedPerPlayer = new Dictionary<PlayerInternal, PointsSet>();
			var tickScoresPerPlayer = _players.ToDictionary(p => p, p => 0);
			foreach (var player in _players)
			{
				UpdatePlayerLines(player);

				var capturer = new TerritoryCapturer(_mapSize, player.Territory);
				var capturedTerritory = capturer.Capture(player.Tail);
				capturedPerPlayer.Add(player, capturedTerritory);

				if (capturedTerritory.Count > 0)
				{
					player.Tail = Path.Empty;
					tickScoresPerPlayer[player] += Constants.NeutralTerritoryScore*capturedTerritory.Count;
				}
			}

			var losers = new List<PlayerInternal>();
			foreach (var player in _players)
			{
				bool isLoss = CheckIsLoss(player, tickScoresPerPlayer);
				if (isLoss)
				{
					losers.Add(player);
				}
			}

			capturedPerPlayer = CollisionResolution(capturedPerPlayer);

			foreach (var player in _players)
			{
				var isLossByPlayer = IsPlayerAte(player, capturedPerPlayer);
				if (isLossByPlayer.Item1)
				{
					losers.Add(player);
				}
			}

			foreach (var player in _players)
			{
				if (capturedPerPlayer[player].Count > 0)
				{
					player.Territory.UnionWith(capturedPerPlayer[player]);
					foreach (var anotherPlayer in _players.Where(p => p != player))
					{
						int srcCount = anotherPlayer.Territory.Count;
						anotherPlayer.Territory.ExceptWith(capturedPerPlayer[player]);
						int croppedCount = anotherPlayer.Territory.Count;
						tickScoresPerPlayer[player] += (Constants.EnemyTerritoryScore - Constants.NeutralTerritoryScore)*(srcCount - croppedCount);
					}
				}
			}

			foreach (var looser in losers)
			{
				_players.Remove(looser);
			}

			foreach (var player in _players)
			{
				player.Score += tickScoresPerPlayer[player];
			}

			return new GameStateInternal(tickNumber, _players, bonuses, state, move);
		}

		private Dictionary<PlayerInternal, PointsSet> CollisionResolution(Dictionary<PlayerInternal, PointsSet> capturedPerPlayer)
		{
			var p_to_c = capturedPerPlayer.Where(p => !IsPlayerAte(p.Key, capturedPerPlayer).Item1).ToDictionary(x => x.Key, x => x.Value);
			var res = new Dictionary<PlayerInternal, PointsSet>(p_to_c);

			foreach (var pair1 in p_to_c)
			{
				foreach (var pair2 in p_to_c)
				{
					if (pair1.Key != pair2.Key)
					{
						res[pair1.Key] = res[pair1.Key].ExceptWith(pair2.Value);
					}
				}
			}

			return res;
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

		private static void UpdatePlayerLines(PlayerInternal player)
		{
			if (!player.Territory.Contains(player.Position) || player.Tail.Length > 0)
			{
				player.Tail = player.Tail.Append(player.Position);
			}
		}

		private bool CheckIsLoss(PlayerInternal player, IDictionary<PlayerInternal, int> tickScoresPerPlayer)
		{
			bool result = false;

			// Выход за границы карты
			if (!_mapSize.ContainsPoint(player.Position))
			{
				result = true;
			}

			// Переезд хвоста
			var linesBeforeMove = player.Tail.RemoveFromEnd(1);
			foreach (var anotherPlayer in _players)
			{
				if (linesBeforeMove.Contains(anotherPlayer.Position))
				{
					result = true;
					if (anotherPlayer != player)
					{
						tickScoresPerPlayer[anotherPlayer] += Constants.LineKillScore;
					}
				}
			}

			// Столкновение "лоб в лоб"
			if (player.Tail.Length > 0)
			{
				foreach (var anotherPlayer in _players.Where(p => p != player))
				{
					if (player.Position == anotherPlayer.Position)
					{
						if (player.Tail.Length > anotherPlayer.Tail.Length)
						{
							result = true;
							break;
						}
					}
				}
			}

			// Остался без территории
			if (player.Territory.Count == 0)
			{
				result = true;
			}

			return result;
		}

		private (bool, PlayerInternal) IsPlayerAte(PlayerInternal player, Dictionary<PlayerInternal, PointsSet> capturedPerPlayer)
		{
			foreach (var capture in capturedPerPlayer.Where(x => x.Key != player))
			{
				if (capture.Value.Contains(player.Position))
				{
					return (true, capture.Key);
				}
			}

			return (false, null);
		}

		private static PlayerInternal ClonePlayer(PlayerInternal player)
		{
			return new PlayerInternal {
				Id = player.Id,
				Score = player.Score,
				Territory = new PointsSet(player.Territory),
				Position = player.Position,
				Tail = new Path(player.Tail),
				Bonuses = (ActiveBonusInfo[])player.Bonuses.Clone(),
				Direction = player.Direction
			};
		}
	}
}
