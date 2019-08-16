using System;
using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;
using MiniAiCup.Paperio.Core;
using Newtonsoft.Json.Linq;

namespace MiniAiCup.Paperio.Benchmark
{
	[ClrJob, MonoJob]
	[RankColumn]
	public class BuildOutsideDistanceMapBenchmark
	{
		private readonly PointsSet _territory;

		private readonly Point _position;

		private readonly Path _tail;

		public BuildOutsideDistanceMapBenchmark()
		{
			Game.Initialize();

			string json = "{\"1\": {\"score\": 22, \"direction\": \"left\", \"territory\": [[645, 585], [645, 615], [645, 645], [645, 675], [645, 705], [645, 735], [645, 765], " +
				"[675, 585], [675, 615], [675, 645], [675, 675], [675, 705], [675, 735], [675, 765], [705, 585], [705, 615], [705, 645], [705, 675], [705, 705], [705, 735], " +
				"[735, 615], [735, 645], [735, 675], [735, 705], [735, 735], [765, 645], [765, 675], [765, 705], [795, 645], [795, 675], [795, 705]], \"lines\": [[675, 555], " +
				"[675, 525], [675, 495], [705, 495], [735, 495], [765, 495], [795, 495], [795, 525], [795, 555], [795, 585], [765, 585]], \"position\": [765, 585], \"bonuses\": []}}";

			var jPlayer = (JProperty)JObject.Parse(json).First;
			var player = ParsePlayer(jPlayer);

			const int cellSize = 30;
			_position = player.Position.ConvertToLogic(cellSize);
			_territory = new PointsSet(player.Territory.Select(p => p.ConvertToLogic(cellSize)));
			_tail = new Path(player.Lines.Select(p => p.ConvertToLogic(cellSize)));
		}

		[Benchmark]
		public int[,] Initial()
		{
			var homePoints = new List<(Point Point, Direction SourceDirection, int PathLength)>();

			var map = Game.GetNewMap<int>();
			Utils.FastCopyArray(Game.NoEnemiesDangerousMap, map, GameParams.MapSize.Width*GameParams.MapSize.Height);

			var visited = Game.GetNewMap<bool>();

			map[_position.X, _position.Y] = 0;
			visited[_position.X, _position.Y] = true;

			var queue = new Queue<Point>(GameParams.MapSize.Width*GameParams.MapSize.Height);
			queue.Enqueue(_position);

			while (queue.Count > 0)
			{
				var currentPoint = queue.Dequeue();
				int currentLength = map[currentPoint.X, currentPoint.Y];
				foreach (var direction in EnumValues.GetAll<Direction>())
				{
					var neighbor = currentPoint.MoveLogic(direction);
					if (!GameParams.MapSize.ContainsPoint(neighbor) || _tail.AsPointsSet().Contains(neighbor))
					{
						continue;
					}

					if (_territory.Contains(neighbor) && !_territory.Contains(currentPoint))
					{
						homePoints.Add((neighbor, direction, currentLength + 1));
					}

					if (visited[neighbor.X, neighbor.Y])
					{
						continue;
					}

					map[neighbor.X, neighbor.Y] = currentLength + 1;
					visited[neighbor.X, neighbor.Y] = true;
					queue.Enqueue(neighbor);
				}
			}

			if (homePoints.Count == 0)
			{
				return map;
			}

			for (int y = 0; y < GameParams.MapSize.Height; y++)
			{
				for (int x = 0; x < GameParams.MapSize.Width; x++)
				{
					map[x, y] = Math.Min(map[x, y], homePoints.Min(p => p.Point.GetDistanceTo(new Point(x, y), p.SourceDirection)));
				}
			}

			return map;
		}

		[Benchmark]
		public int[,] BfsFromTail()
		{
			var homePoints = new List<(Point Point, Direction SourceDirection, int PathLength)>();

			var map = Game.GetNewMap<int>();
			Utils.FastCopyArray(Game.NoEnemiesDangerousMap, map, GameParams.MapSize.Width*GameParams.MapSize.Height);

			var visited = Game.GetNewMap<bool>();

			map[_position.X, _position.Y] = 0;
			visited[_position.X, _position.Y] = true;

			var queue = new Queue<Point>(GameParams.MapSize.Width*GameParams.MapSize.Height);
			queue.Enqueue(_position);

			while (queue.Count > 0)
			{
				var currentPoint = queue.Dequeue();
				int currentLength = map[currentPoint.X, currentPoint.Y];
				foreach (var direction in EnumValues.GetAll<Direction>())
				{
					var neighbor = currentPoint.MoveLogic(direction);
					if (!GameParams.MapSize.ContainsPoint(neighbor) || _tail.AsPointsSet().Contains(neighbor))
					{
						continue;
					}

					if (_territory.Contains(neighbor) && !_territory.Contains(currentPoint))
					{
						homePoints.Add((neighbor, direction, currentLength + 1));
					}

					if (visited[neighbor.X, neighbor.Y])
					{
						continue;
					}

					map[neighbor.X, neighbor.Y] = currentLength + 1;
					visited[neighbor.X, neighbor.Y] = true;
					queue.Enqueue(neighbor);
				}
			}

			if (homePoints.Count == 0)
			{
				return map;
			}

			var mapAfterHome = Game.GetNewMap<int>();
			var visitedAfterHome = Game.GetNewMap<bool>();
			foreach (var tailPoint in _tail)
			{
				mapAfterHome[tailPoint.X, tailPoint.Y] = homePoints.Min(x => x.Point.GetDistanceTo(tailPoint, x.SourceDirection) + x.PathLength);
				visitedAfterHome[tailPoint.X, tailPoint.Y] = true;
				queue.Enqueue(tailPoint);
			}

			while (queue.Count > 0)
			{
				var currentPoint = queue.Dequeue();
				int currentLength = mapAfterHome[currentPoint.X, currentPoint.Y];
				foreach (var neighbor in currentPoint.GetNeighbors())
				{
					if (!GameParams.MapSize.ContainsPoint(neighbor) || visitedAfterHome[neighbor.X, neighbor.Y])
					{
						continue;
					}

					mapAfterHome[neighbor.X, neighbor.Y] = currentLength + 1;
					visitedAfterHome[neighbor.X, neighbor.Y] = true;
					queue.Enqueue(neighbor);
				}
			}

			for (int y = 0; y < GameParams.MapSize.Height; y++)
			{
				for (int x = 0; x < GameParams.MapSize.Width; x++)
				{
					map[x, y] = Math.Min(map[x, y], mapAfterHome[x, y]);
				}
			}

			return map;
		}

		[Benchmark]
		public int[,] DoubleSides()
		{
			var map = Game.GetNewMap<int>();
			Utils.FastCopyArray(Game.NoEnemiesDangerousMap, map, GameParams.MapSize.Width*GameParams.MapSize.Height);
			var visited = Game.GetNewMap<bool>();

			var mapAfterHome = Game.GetNewMap<int>();
			var visitedAfterHome = Game.GetNewMap<bool>();

			map[_position.X, _position.Y] = 0;
			visited[_position.X, _position.Y] = true;

			var queue = new Queue<(Point Point, bool AfterHome, Direction? VisitHomeDirection)>(GameParams.MapSize.Width*GameParams.MapSize.Height);
			queue.Enqueue((_position, false, null));

			bool visitHome = false;
			while (queue.Count > 0)
			{
				(var currentPoint, bool afterHome, var visitHomeDirection) = queue.Dequeue();

				if (!afterHome)
				{
					int currentLength = map[currentPoint.X, currentPoint.Y];
					foreach (var direction in EnumValues.GetAll<Direction>())
					{
						var neighbor = currentPoint.MoveLogic(direction);
						if (!GameParams.MapSize.ContainsPoint(neighbor) || _tail.AsPointsSet().Contains(neighbor))
						{
							continue;
						}

						if (_territory.Contains(neighbor) && !_territory.Contains(currentPoint) && !visitedAfterHome[neighbor.X, neighbor.Y])
						{
							queue.Enqueue((neighbor, true, direction));
							mapAfterHome[neighbor.X, neighbor.Y] = currentLength + 1;
							visitHome = true;
						}

						if (visited[neighbor.X, neighbor.Y])
						{
							continue;
						}

						map[neighbor.X, neighbor.Y] = currentLength + 1;
						visited[neighbor.X, neighbor.Y] = true;
						queue.Enqueue((neighbor, false, null));
					}
				}
				else
				{
					int currentLength = mapAfterHome[currentPoint.X, currentPoint.Y];
					foreach (var direction in EnumValues.GetAll<Direction>())
					{
						var neighbor = currentPoint.MoveLogic(direction);
						if (!GameParams.MapSize.ContainsPoint(neighbor) || visitedAfterHome[neighbor.X, neighbor.Y] || visitHomeDirection == direction.GetOpposite())
						{
							continue;
						}

						mapAfterHome[neighbor.X, neighbor.Y] = currentLength + 1;
						visitedAfterHome[neighbor.X, neighbor.Y] = true;
						queue.Enqueue((neighbor, true, null));
					}
				}
			}

			if (!visitHome)
			{
				return map;
			}

			for (int y = 0; y < GameParams.MapSize.Height; y++)
			{
				for (int x = 0; x < GameParams.MapSize.Width; x++)
				{
					map[x, y] = Math.Min(map[x, y], mapAfterHome[x, y]);
				}
			}

			return map;
		}

		private static PlayerInfo ParsePlayer(JProperty jIdentityPlayer)
		{
			string id = jIdentityPlayer.Name;
			var jPlayer = jIdentityPlayer.Value;
			return new PlayerInfo {
				Id = id,
				Score = (int)jPlayer["score"],
				Territory = jPlayer["territory"].Cast<JArray>().Select(ParsePoint).ToArray(),
				Position = ParsePoint((JArray)jPlayer["position"]),
				Lines = jPlayer["lines"].Cast<JArray>().Select(ParsePoint).ToArray(),
				Bonuses = jPlayer["bonuses"].Cast<JObject>().Select(ParseActiveBonus).ToArray(),
				Direction = ParseDirection((string)jPlayer["direction"])
			};
		}

		private static Direction? ParseDirection(string sDirection)
		{
			switch (sDirection)
			{
				case null: return null;
				case "left": return Direction.Left;
				case "right": return Direction.Right;
				case "up": return Direction.Up;
				case "down": return Direction.Down;
				default: throw new ArgumentOutOfRangeException(nameof(sDirection), sDirection, null);
			}
		}

		private static ActiveBonusInfo ParseActiveBonus(JObject jActiveBonus)
		{
			return new ActiveBonusInfo {
				Type = ParseBonusType((string)jActiveBonus["type"]),
				RemainingSteps = (int)jActiveBonus["ticks"]
			};
		}

		private static BonusType ParseBonusType(string sType)
		{
			switch (sType)
			{
				case "n": return BonusType.Nitro;
				case "s": return BonusType.Slowdown;
				case "saw": return BonusType.Saw;
				default: throw new ArgumentOutOfRangeException(nameof(sType), sType, null);
			}
		}

		private static Point ParsePoint(JArray jPointArray)
		{
			return new Point((int)jPointArray[0], (int)jPointArray[1]);
		}
	}
}
