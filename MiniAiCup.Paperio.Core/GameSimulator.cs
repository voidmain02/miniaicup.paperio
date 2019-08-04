using System.Collections.Generic;
using System.Linq;

namespace MiniAiCup.Paperio.Core
{
	public class GameSimulator
	{
		private Size _mapSize;

		private List<PlayerInternal> _players;

		private Dictionary<PlayerInternal, PointsSet> _capturedTerritoryPerPlayer;

		private Dictionary<PlayerInternal, int> _scoresGainedPerPlayer;

		private readonly IEnemyStrategy _enemyStrategy;

		public GameSimulator(IEnemyStrategy enemyStrategy = null)
		{
			_enemyStrategy = enemyStrategy;
		}

		public GameStateInternal Simulate(GameStateInternal state, Move move)
		{
			_mapSize = state.MapSize;

			int tickNumber = state.TickNumber + state.CellSize/state.Speed;
			var bonuses = state.Bonuses;

			_players = new List<PlayerInternal>();
			foreach (var srcPlayer in state.Players.Values)
			{
				var player = (PlayerInternal)srcPlayer.Clone();
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

			_capturedTerritoryPerPlayer = new Dictionary<PlayerInternal, PointsSet>();
			_scoresGainedPerPlayer = _players.ToDictionary(p => p, p => 0);
			foreach (var player in _players)
			{
				UpdatePlayerTail(player);

				var capturer = new TerritoryCapturer(_mapSize, player.Territory);
				var capturedTerritory = capturer.Capture(player.Tail);
				_capturedTerritoryPerPlayer.Add(player, capturedTerritory);

				if (capturedTerritory.Count > 0)
				{
					player.Tail = Path.Empty;
					_scoresGainedPerPlayer[player] += Constants.NeutralTerritoryScore*capturedTerritory.Count;
				}
			}

			var losers = new List<PlayerInternal>();
			foreach (var player in _players)
			{
				if (CheckIsLoss(player))
				{
					losers.Add(player);
				}
			}

			ResolveCollisions();

			foreach (var player in _players)
			{
				if (CheckIsPlayerAte(player))
				{
					losers.Add(player);
				}
			}

			foreach (var player in _players)
			{
				if (_capturedTerritoryPerPlayer[player].Count <= 0)
				{
					continue;
				}

				player.Territory = player.Territory.UnionWith(_capturedTerritoryPerPlayer[player]);
				foreach (var anotherPlayer in _players.Where(p => p != player))
				{
					int srcCount = anotherPlayer.Territory.Count;
					anotherPlayer.Territory = anotherPlayer.Territory.ExceptWith(_capturedTerritoryPerPlayer[player]);
					int croppedCount = anotherPlayer.Territory.Count;
					_scoresGainedPerPlayer[player] += (Constants.EnemyTerritoryScore - Constants.NeutralTerritoryScore)*(srcCount - croppedCount);
				}
			}

			foreach (var looser in losers)
			{
				_players.Remove(looser);
			}

			foreach (var player in _players)
			{
				player.Score += _scoresGainedPerPlayer[player];
			}

			return new GameStateInternal(tickNumber, _players, bonuses, state, move);
		}

		private void ResolveCollisions()
		{
			var capturedTerritoryPerNotAtePlayer = _capturedTerritoryPerPlayer.Where(p => !CheckIsPlayerAte(p.Key))
				.ToDictionary(x => x.Key, x => x.Value);
			var result = new Dictionary<PlayerInternal, PointsSet>(capturedTerritoryPerNotAtePlayer);

			foreach (var pair1 in capturedTerritoryPerNotAtePlayer)
			{
				foreach (var pair2 in capturedTerritoryPerNotAtePlayer)
				{
					if (pair1.Key != pair2.Key)
					{
						result[pair1.Key] = result[pair1.Key].ExceptWith(pair2.Value);
					}
				}
			}

			_capturedTerritoryPerPlayer = result;
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

		private static void UpdatePlayerTail(PlayerInternal player)
		{
			if (!player.Territory.Contains(player.Position) || player.Tail.Length > 0)
			{
				player.Tail = player.Tail.Append(player.Position);
			}
		}

		private bool CheckIsLoss(PlayerInternal player)
		{
			// Переезд хвоста
			bool isTailCutted = false;
			var tailBeforeMove = player.Tail.RemoveFromEnd(1);
			foreach (var anotherPlayer in _players)
			{
				if (tailBeforeMove.AsPointsSet().Contains(anotherPlayer.Position))
				{
					isTailCutted = true;
					if (anotherPlayer != player)
					{
						_scoresGainedPerPlayer[anotherPlayer] += Constants.LineKillScore;
					}
				}
			}

			if (isTailCutted)
			{
				return true;
			}

			// Выход за границы карты
			if (!_mapSize.ContainsPoint(player.Position))
			{
				return true;
			}

			// Столкновение "лоб в лоб"
			if (player.Tail.Length > 0)
			{
				foreach (var anotherPlayer in _players.Where(p => p != player))
				{
					if (player.Position == anotherPlayer.Position && player.Tail.Length > anotherPlayer.Tail.Length)
					{
						return true;
					}
				}
			}

			// Остался без территории
			if (player.Territory.Count == 0)
			{
				return true;
			}

			return false;
		}

		private bool CheckIsPlayerAte(PlayerInternal player)
		{
			return _capturedTerritoryPerPlayer.Where(x => x.Key != player).Any(capture => capture.Value.Contains(player.Position));
		}
	}
}
