using System;
using System.Collections.Generic;
using System.Linq;

namespace MiniAiCup.Paperio.Core
{
	public class Game
	{
		private readonly GameParams _gameParams;

		private readonly Random _random = new Random();

		private GameState _currentRealState;

		private GameState _currentLogicState;

		private PlayerInfo Me => _currentLogicState.Players.First(p => p.Id == "i");

		private IEnumerable<PlayerInfo> Enemies => _currentLogicState.Players.Where(p => p != Me);

		private readonly Dictionary<Move, Point[]> _pathsToHome;

		public Game(GameParams gameParams)
		{
			_gameParams = gameParams;
			_pathsToHome = Enum.GetValues(typeof(Move)).Cast<Move>().ToDictionary(x => x, y => (Point[])null);
		}

		private void UpdatePathsToHome()
		{
			foreach (var move in Enum.GetValues(typeof(Move)).Cast<Move>())
			{
				_pathsToHome[move] = IsMoveValid(Me, move)
					? GetShortestPathToHome(move)
					: null;
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

		public Direction GetNextDirection(GameState state, out GameDebugData debugData)
		{
			_currentRealState = state;
			_currentLogicState = ConvertRealGameStateToLogic(state, _gameParams.CellSize);

			UpdatePathsToHome();

			var safeMoves = _pathsToHome.Where(x => x.Value != null).Where(x => IsMoveSafeForMe(x.Key)).ToList();

			var movePair = safeMoves.Count == 0
				? _pathsToHome.OrderBy(p => p.Value?.Length ?? Int32.MaxValue).First()
				: safeMoves[_random.Next(0, safeMoves.Count)];

			var direction = Me.Direction.Value.GetMoved(movePair.Key);
			var realPathToHome = movePair.Value.Select(p => p.ConvertToReal(_gameParams.CellSize)).ToArray();
			debugData = new GameDebugData {
				Direction = direction,
				PathToHome = realPathToHome
			};

			return direction;
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
