using System;
using System.Collections.Generic;
using System.Linq;

namespace MiniAiCup.Paperio.Core
{
	public class GameStateInternal
	{
		public Size MapSize { get; private set; }

		public int CellSize { get; private set; }

		public BonusInfo[] Bonuses { get; private set; }

		public int TickNumber { get; private set; }

		public Dictionary<string, PlayerInternal> Players { get; private set; }

		public GameStateInternal PreviousState { get; private set; }

		public Move PreviousMove { get; private set; }

		private readonly Lazy<PlayerInternal> _me;

		public PlayerInternal Me => _me.Value;

		private readonly Lazy<PlayerInternal[]> _enemies;

		public PlayerInternal[] Enemies => _enemies.Value;

		private readonly Lazy<Point[]> _pathToHome;

		public Point[] PathToHome => _pathToHome.Value;

		private GameStateInternal()
		{
			_me = new Lazy<PlayerInternal>(() => Players.ContainsKey("i") ? Players["i"] : null);
			_enemies = new Lazy<PlayerInternal[]>(() => Players.Values.Where(p => p.Id != "i").ToArray());
			_pathToHome = new Lazy<Point[]>(BuildPathToHome);
		}

		public GameStateInternal(GameState state, GameParams gameParams) : this()
		{
			PreviousMove = Move.Forward;
			MapSize = gameParams.MapLogicSize;
			CellSize = gameParams.CellSize;
			ApplyState(state);
		}

		public GameStateInternal(GameState state, GameStateInternal previousState, Move previousMove) : this()
		{
			PreviousMove = previousMove;
			PreviousState = previousState;
			MapSize = previousState.MapSize;
			CellSize = previousState.CellSize;
			ApplyState(state);
		}

		private void ApplyState(GameState state)
		{
			TickNumber = state.TickNumber;
			Bonuses = state.Bonuses.Select(b => new BonusInfo {
				Type = b.Type,
				Position = b.Position.ConvertToLogic(CellSize)
			}).ToArray();
			Players = state.Players.Select(p => BuildInternalPlayer(p, CellSize)).ToDictionary(p => p.Id);
		}

		private static PlayerInternal BuildInternalPlayer(PlayerInfo player, int cellSize)
		{
			return new PlayerInternal {
				Id = player.Id,
				Score = player.Score,
				Territory = new HashSet<Point>(player.Territory.Select(p => p.ConvertToLogic(cellSize))),
				Position = player.Position.ConvertToLogic(cellSize),
				Lines = new HashSet<Point>(player.Lines.Select(p => p.ConvertToLogic(cellSize))),
				Bonuses = player.Bonuses,
				Direction = player.Direction.Value
			};
		}

		private Point[] BuildPathToHome()
		{
			if (Me == null)
			{
				return null;
			}
			return PathFinder.GetShortestPath(Me.Position, Me.Territory, Me.Lines, MapSize);
		}

		public float Score()
		{
			if (Me == null)
			{
				return -1.0f;
			}

			if (Me.Territory.Contains(Me.Position))
			{
				var freeTerritory = new HashSet<Point>(Me.Territory.SelectMany(x => x.GetNeighbors()).Distinct().Where(x => MapSize.ContainsPoint(x) && !Me.Territory.Contains(x)));
				var obstacles = new HashSet<Point> { Me.Position.MoveLogic(Me.Direction.GetOpposite()) };
				var pathToOutside = PathFinder.GetShortestPath(Me.Position, freeTerritory, obstacles, MapSize);

				var scoring = new InsideScoring {
					MinPathToOutsideLength = pathToOutside.Length
				};
				return scoring.Calc();
			}
			else
			{
				if (PathToHome == null)
				{
					return -1.0f;
				}

				int minPathFromEnemyToMyLines = Enemies.Length == 0
					? Int32.MaxValue
					: Enemies.Select(e => PathFinder.GetShortestPath(e.Position, Me.Lines, e.Lines, MapSize)?.Length ?? Int32.MaxValue).Min();

				var scoring = new OutsideScoring {
					MinPathToHomeLength = PathToHome.Length,
					MinPathFromEnemyToMyLinesLength = minPathFromEnemyToMyLines,
					IsForwardMove = PreviousMove == Move.Forward
				};
				return scoring.Calc();
			}
		}


		private class OutsideScoring
		{
			public int MinPathToHomeLength { get; set; }

			public int MinPathFromEnemyToMyLinesLength { get; set; }

			public bool IsForwardMove { get; set; }

			public float Calc()
			{
				if (MinPathFromEnemyToMyLinesLength < MinPathToHomeLength + 1)
				{
					return -0.5f;
				}

				float longPathToHomePenalty = Math.Min(10 - MinPathToHomeLength, 0)*0.1f;
				float forwardMoveBonus = IsForwardMove ? 0.3f : 0.0f;
				float score = longPathToHomePenalty + forwardMoveBonus;
				return EnsureInRange(score, -1.0f, 1.0f);
			}

			private static float EnsureInRange(float value, float min, float max)
			{
				if (value < min)
				{
					return min;
				}

				if (value > max)
				{
					return max;
				}

				return value;
			}
		}

		private class InsideScoring
		{
			public int MinPathToOutsideLength { get; set; }

			public float Calc()
			{
				return EnsureInRange((2 - MinPathToOutsideLength)/10.0f, -1.0f, 1.0f);
			}

			private static float EnsureInRange(float value, float min, float max)
			{
				if (value < min)
				{
					return min;
				}

				if (value > max)
				{
					return max;
				}

				return value;
			}
		}

		public GameStateInternal Simulate(Move move)
		{
			return new GameStateInternal {
				PreviousMove = move,
				MapSize = MapSize,
				Players = Players.Values.Select(p => Simulate(p, p == Me ? move : Move.Forward)).Where(p => p != null).ToDictionary(p => p.Id)
			};
		}

		private PlayerInternal Simulate(PlayerInternal player, Move move)
		{
			var nextDirection = player.Direction.GetMoved(move);
			var nextPos = player.Position.MoveLogic(nextDirection);
			if (!MapSize.ContainsPoint(nextPos))
			{
				return null;
			}

			if (Players[player.Id].Lines.Contains(nextPos))
			{
				return null;
			}

			var lines = new HashSet<Point>(player.Lines);
			if (!player.Territory.Contains(nextPos))
			{
				lines.Add(nextPos);
			}

			return new PlayerInternal {
				Direction = nextDirection,
				Lines = lines,
				Position = nextPos,
				Territory = new HashSet<Point>(player.Territory),
				Id = player.Id,
				Bonuses = player.Bonuses,
				Score = player.Score
			};
		}
	}
}
