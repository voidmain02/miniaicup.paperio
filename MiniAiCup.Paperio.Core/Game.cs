using System;
using System.Collections.Generic;
using System.Linq;

namespace MiniAiCup.Paperio.Core
{
	public class Game
	{
		private readonly GameParams _gameParams;

		private readonly Dictionary<Move, Point[]> _pathsToHome;

		private int? _shortestPathToHomeLength;

		private GameState _currentRealState;

		private GameState _currentLogicState;

		private Point[] _pathToOccupate;

		private HashSet<Point> _myTerritory;

		private PlayerInfo Me => _currentLogicState.Players.First(p => p.Id == "i");

		private IEnumerable<PlayerInfo> Enemies => _currentLogicState.Players.Where(p => p != Me);

		private const int MaxPathToHomeLength = 8;

		public Game(GameParams gameParams)
		{
			_gameParams = gameParams;
			_pathsToHome = Enum.GetValues(typeof(Move)).Cast<Move>().ToDictionary(x => x, y => (Point[])null);
		}

		public Direction GetNextDirection(GameState state, out GameDebugData debugData)
		{
			_currentRealState = state;
			_currentLogicState = ConvertRealGameStateToLogic(state, _gameParams.CellSize);

			if (Me.Direction == null)
			{
				debugData = null;
				return GetStartDirection();
			}

			var bestMove = Move.Forward;
			_myTerritory = new HashSet<Point>(Me.Territory);
			if (_myTerritory.Contains(Me.Position))
			{
				UpdatePathToOccupate();
				foreach (var move in (Move[])Enum.GetValues(typeof(Move)))
				{
					if (Me.Position.MoveLogic(Me.Direction.Value.GetMoved(move)) == _pathToOccupate[0])
					{
						bestMove = move;
						break;
					}
				}
			}
			else
			{
				UpdatePathsToHome();
				var scoresDictionary = new Dictionary<Move, int>();
				foreach (var move in (Move[])Enum.GetValues(typeof(Move)))
				{
					scoresDictionary[move] = ScoreMove(move);
				}

				bestMove = scoresDictionary.OrderByDescending(p => p.Value).First().Key;
			}

			var nextDirection = Me.Direction.Value.GetMoved(bestMove);
			var realPathToHome = _pathsToHome[bestMove] == null
				? null
				: _pathsToHome[bestMove].Select(p => p.ConvertToReal(_gameParams.CellSize)).ToArray();

			debugData = new GameDebugData {
				Direction = nextDirection,
				PathToHome = realPathToHome
			};

			return nextDirection;
		}

		private int ScoreMove(Move move)
		{
			if (_pathsToHome[move] == null)
			{
				return -100;
			}

			if (!IsMoveSafeForMe(move))
			{
				return _pathsToHome[move].Length == _shortestPathToHomeLength ? -10 : -50;
			}

			int longPathToHomePenalty = Math.Min(MaxPathToHomeLength - _pathsToHome[move].Length, 0)*10;
			int forwardMoveBonus = move == Move.Forward ? 20 : 0;

			return Math.Min(longPathToHomePenalty + forwardMoveBonus, 100);
		}

		private void UpdatePathToOccupate()
		{
			var freeTerritory = Me.Territory.SelectMany(x => x.GetNeighbors()).Distinct()
				.Where(x => _gameParams.MapLogicSize.ContainsPoint(x) && !_myTerritory.Contains(x)).ToArray();
			_pathToOccupate = PathFinder.GetShortestPath(Me.Position, freeTerritory, new Point[] { Me.Position.MoveLogic(Me.Direction.Value.GetOpposite()) },
				_gameParams.MapLogicSize);
		}

		private Direction GetStartDirection()
		{
			int maxDistance = 0;
			var maxDistanceDirection = Direction.Left;
			foreach (var direction in (Direction[])Enum.GetValues(typeof(Direction)))
			{
				int distance = GetDistanceToBorder(Me.Position, direction);
				if (distance > maxDistance)
				{
					maxDistance = distance;
					maxDistanceDirection = direction;
				}
			}

			return maxDistanceDirection;
		}

		private int GetDistanceToBorder(Point point, Direction direction)
		{
			switch (direction)
			{
				case Direction.Left: return point.X;
				case Direction.Up: return _gameParams.MapLogicSize.Height - point.Y - 1;
				case Direction.Right: return _gameParams.MapLogicSize.Width - point.X - 1;
				case Direction.Down: return point.Y;
				default:
					throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
			}
		}

		private void UpdatePathsToHome()
		{
			_shortestPathToHomeLength = Int32.MaxValue;
			bool pathFound = false;
			foreach (var move in Enum.GetValues(typeof(Move)).Cast<Move>())
			{
				if (!IsMoveValid(Me, move))
				{
					_pathsToHome[move] = null;
					continue;
				}

				var path = GetShortestPathToHome(move);
				if (path == null)
				{
					_pathsToHome[move] = null;
					continue;
				}

				pathFound = true;
				_pathsToHome[move] = path;
				if (path.Length < _shortestPathToHomeLength)
				{
					_shortestPathToHomeLength = path.Length;
				}
			}

			if (!pathFound)
			{
				_shortestPathToHomeLength = null;
			}
		}

		private Point[] GetShortestPathToHome(Move move)
		{
			var direction = Me.Direction.Value.GetMoved(move);
			var nextPos = Me.Position.MoveLogic(direction);
			var territoryExceptCurrentPosition = Me.Territory.Where(p => p != Me.Position).ToArray();
			var linesWithCurrentPositionList = Me.Lines.ToList();
			linesWithCurrentPositionList.Add(Me.Position);
			var linesWithCurrentPosition = linesWithCurrentPositionList.Distinct().ToArray();
			return PathFinder.GetShortestPath(nextPos, territoryExceptCurrentPosition, linesWithCurrentPosition, _gameParams.MapLogicSize);
		}

		private IEnumerable<Move> GetValidMoves(PlayerInfo player)
		{
			return Enum.GetValues(typeof(Move)).Cast<Move>().Where(d => IsMoveValid(player, d));
		}

		private bool IsMoveValid(PlayerInfo player, Move move)
		{
			var nextPos = player.Position.MoveLogic(player.Direction.Value.GetMoved(move));
			return _gameParams.MapLogicSize.ContainsPoint(nextPos) && !player.Lines.Contains(nextPos);
		}

		private bool IsMoveSafeForMe(Move move)
		{
			var direction = Me.Direction.Value.GetMoved(move);
			var nextPos = Me.Position.MoveLogic(direction);
			if (Me.Territory.Contains(nextPos))
			{
				return true;
			}

			var minPathToTerritoryLength = _pathsToHome[move]?.Length;
			if (minPathToTerritoryLength == null)
			{
				return false;
			}

			if (!Enemies.Any())
			{
				return true;
			}

			var enemyTargetList = new List<Point>();
			enemyTargetList.AddRange(Me.Lines);
			enemyTargetList.Add(nextPos);
			enemyTargetList.AddRange(_pathsToHome[move]);
			var enemyTarget = enemyTargetList.ToArray();

			int minPathFromEnemyToMyLinesLength = Enemies.SelectMany(p => GetValidMoves(p).Select(m => new { Player = p, Move = m })).Select(x => {
				var enemyDirection = x.Player.Direction.Value.GetMoved(x.Move);
				var nextEnemyPos = x.Player.Position.MoveLogic(enemyDirection);
				var obstaclesList = x.Player.Lines.ToList();
				obstaclesList.Add(x.Player.Position);
				var obstacles = obstaclesList.Distinct().ToArray();
				return PathFinder.GetShortestPath(nextEnemyPos, enemyTarget, obstacles, _gameParams.MapLogicSize)?.Length ?? Int32.MaxValue;
			}).DefaultIfEmpty(Int32.MaxValue).Min();

			return minPathFromEnemyToMyLinesLength > minPathToTerritoryLength;
		}

		private static GameState ConvertRealGameStateToLogic(GameState gameState, int cellSize)
		{
			return new GameState {
				Players = gameState.Players.Select(p => ConvertRealPlayerToLogic(p, cellSize)).ToArray(),
				Bonuses = gameState.Bonuses,
				TickNumber = gameState.TickNumber
			};
		}

		private static PlayerInfo ConvertRealPlayerToLogic(PlayerInfo player, int cellSize)
		{
			return new PlayerInfo {
				Id = player.Id,
				Score = player.Score,
				Territory = player.Territory.Select(p => p.ConvertToLogic(cellSize)).ToArray(),
				Position = player.Position.ConvertToLogic(cellSize),
				Lines = player.Lines.Select(p => p.ConvertToLogic(cellSize)).ToArray(),
				Bonuses = player.Bonuses,
				Direction = player.Direction
			};
		}
	}
}
