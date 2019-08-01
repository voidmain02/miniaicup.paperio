using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace MiniAiCup.Paperio.Core
{
	public class Territory
	{
		private readonly Size _mapSize;
		private readonly PointsSet _points;

		public Territory(Size mapSize, PointsSet points)
		{
			_mapSize = mapSize;
			_points = points;
		}

		public PointsSet Capture(Path tail)
		{
			var list = new List<Point>();

			if (tail.Length > 1)
			{
				if (_points.Contains(tail.Last()))
				{
					var voids = GetVoidsBetweenLinesAndTerritory(_points, tail);
					list.AddRange(capture_voids_between_lines(tail));
					foreach (var point in tail)
					{
						if (!_points.Contains(point))
						{
							list.Add(point);
						}
					}

					foreach (var @void in voids)
					{
						list.AddRange(_capture(@void));
					}
				}
			}

			return new PointsSet(list);
		}

		private IEnumerable<Point> _capture(IEnumerable<Point> boundary)
		{
			var poligon_x_arr = boundary.Select(p => p.X).ToList();
			var poligon_y_arr = boundary.Select(p => p.Y).ToList();

			int max_x = poligon_x_arr.Max();
			int max_y = poligon_y_arr.Max();
			int min_x = poligon_x_arr.Min();
			int min_y = poligon_y_arr.Min();

			var captured = new List<Point>();
			int x = max_x;
			while (x > min_x)
			{
				int y = max_y;
				while (y > min_y)
				{
					var point = new Point(x, y);
					if (!_points.Contains(point) && in_polygon(x, y, poligon_x_arr, poligon_y_arr))
					{
						captured.Add(point);
					}

					y--;
				}

				x--;
			}

			return captured;
		}

		private static bool in_polygon(int x, int y, List<int> xp, List<int> yp)
		{
			bool c = false;
			for (int i = 0; i < xp.Count; i++)
			{
				int j = (i + xp.Count - 1)%xp.Count;
				if (((yp[i] <= y && y < yp[j]) || (yp[j] <= y && y < yp[i])) &&
					(x > (xp[j] - xp[i])*(y - yp[i])/(yp[j] - yp[i]) + xp[i]))
				{
					c = !c;
				}
			}

			return c;
		}

		private IEnumerable<Point> capture_voids_between_lines(Path lines)
		{
			var captured = new List<Point>();
			for (int index = 0; index < lines.Length; index++)
			{
				var cur = lines[index];
				foreach (var point in cur.GetNeighbors())
				{
					if (lines.AsPointsSet().Contains(point))
					{
						int end_index = lines.ToList().IndexOf(point);
						var path = lines.Skip(index).Take(end_index - index + 1).ToList();
						if (path.Count >= 8)
						{
							captured.AddRange(_capture(path));
						}
					}
				}
			}

			return captured;
		}

		private List<List<Point>> GetVoidsBetweenLinesAndTerritory(PointsSet territory, Path tail)
		{
			var boundary = territory.GetBoundary();
			var voids = new List<List<Point>>();

			for(int i_lp1 = 0; i_lp1 < tail.Length; i_lp1++)
			{
				var lp1 = tail[i_lp1];
				foreach (var point in lp1.GetEightNeighbors())
				{
					if (boundary.Contains(point))
					{
						Point? prev = null;
						for (int i_lp2 = 0; i_lp2 < i_lp1 + 1; i_lp2++)
						{
							var lp2 = tail[i_lp2];
							var startPoint = get_nearest_boundary(lp2, boundary);
							if (startPoint != null)
							{
								if (prev.HasValue && (is_siblings(prev.Value, startPoint.Value) || prev == startPoint))
								{
									prev = startPoint;
									continue;
								}

								var path = (Path)GetPath(startPoint.Value, point, boundary);
								if (path == null)
								{
									continue;
								}

								/*
								if (path.Length > 1 && path.First() == path.Last())
								{
									path = path.RemoveFromStart(1);
								}
								*/

								var voidPoints = tail.Skip(i_lp2 - 1).Take(i_lp1 - i_lp2 + 1).ToList();
								voidPoints.AddRange(path);
								voids.Add(voidPoints);
							}

							prev = startPoint;
						}
					}
				}
			}

			return voids;
		}

		private Path GetPath(Point startPoint, Point endPoint, PointsSet boundary)
		{
			var allPoints = new List<Point>(_mapSize.Width * _mapSize.Height);
			for (int x = 0; x < _mapSize.Width; x++)
			{
				for (int y = 0; y < _mapSize.Height; y++)
				{
					allPoints.Add(new Point(x, y));
				}
			}
			var obstacles = new PointsSet(allPoints).ExceptWith(boundary);
			return PathFinder.GetShortestPath(startPoint, new PointsSet(new[] { endPoint }), obstacles, _mapSize);
		}

		private static bool is_siblings(Point p1, Point p2)
		{
			return p1.GetNeighbors().Contains(p2);
		}

		private static Point? get_nearest_boundary(Point point, PointsSet boundary)
		{
			foreach (var neighbor in point.GetEightNeighbors())
			{
				if (boundary.Contains(neighbor))
				{
					return neighbor;
				}
			}

			return null;
		}
	}

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

				var territory = new Territory(_mapSize, player.Territory);
				var capturedTerritory = territory.Capture(player.Tail);
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
