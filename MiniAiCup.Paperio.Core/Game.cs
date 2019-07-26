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

		private readonly Dictionary<Direction, Point[]> _pathsToHome;

		public Game(GameParams gameParams)
		{
			_gameParams = gameParams;
			_pathsToHome = Enum.GetValues(typeof(Direction)).Cast<Direction>().ToDictionary(x => x, y => (Point[])null);
		}

		private void UpdatePathsToHome()
		{
			foreach (var direction in Enum.GetValues(typeof(Direction)).Cast<Direction>())
			{
				_pathsToHome[direction] = IsDirectionValid(Me, direction)
					? GetShortestPathToHome(direction)
					: null;
			}
		}

		private Point[] GetShortestPathToHome(Direction direction)
		{
			var nextPos = Me.Position.MoveLogic(direction);
			var territoryExceptCurrentPosition = Me.Territory.Where(p => p != Me.Position).ToArray();
			var linesWithCurrentPositionList = Me.Lines.ToList();
			linesWithCurrentPositionList.Add(Me.Position);
			var linesWithCurrentPosition = linesWithCurrentPositionList.Distinct().ToArray();
			return PathFinder.GetShortestPath(nextPos, territoryExceptCurrentPosition, linesWithCurrentPosition, _gameParams.MapLogicSize);
		}

		public Direction GetNextDirection(GameState state)
		{
			_currentRealState = state;
			_currentLogicState = ConvertRealGameStateToLogic(state, _gameParams.CellSize);

			UpdatePathsToHome();

			var safeDirections = _pathsToHome.Where(x => x.Value != null).Where(x => IsDirectionSafeForMe(x.Key)).ToList();
			return safeDirections.Count == 0
				? _pathsToHome.OrderBy(p => p.Value?.Length ?? Int32.MaxValue).First().Key
				: safeDirections[_random.Next(0, safeDirections.Count)].Key;
		}

		private IEnumerable<Direction> GetValidDirections(PlayerInfo player)
		{
			return Enum.GetValues(typeof(Direction)).Cast<Direction>().Where(d => IsDirectionValid(player, d));
		}

		private bool IsDirectionValid(PlayerInfo player, Direction direction)
		{
			if (direction == player.Direction?.GetOpposite())
			{
				return false;
			}

			var nextPos = player.Position.MoveLogic(direction);
			return _gameParams.MapLogicSize.ContainsPoint(nextPos) && !player.Lines.Contains(nextPos);
		}

		private bool IsDirectionSafeForMe(Direction direction)
		{
			var nextPos = Me.Position.MoveLogic(direction);
			if (Me.Territory.Contains(nextPos))
			{
				return true;
			}

			var minPathToTerritoryLength = _pathsToHome[direction]?.Length;
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
			enemyTargetList.AddRange(_pathsToHome[direction]);
			var enemyTarget = enemyTargetList.ToArray();

			int minPathFromEnemyToMyLinesLength = Enemies.SelectMany(p => GetValidDirections(p).Select(d => new { Player = p, Direction = d })).Select(x => {
				var nextEnemyPos = x.Player.Position.MoveLogic(x.Direction);
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
