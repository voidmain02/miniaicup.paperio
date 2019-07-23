using System;
using System.Collections.Generic;
using System.Linq;

namespace MiniAiCup.Paperio
{
	public class RandomGameLogic : IGameLogic
	{
		private readonly GameParams _gameParams;

		private readonly Random _random = new Random();

		private GameState _currentRealState;

		private GameState _currentLogicState;

		private PlayerInfo Me => _currentLogicState.Players.First(p => p.Id == "i");

		private IEnumerable<PlayerInfo> Enemies => _currentLogicState.Players.Where(p => p != Me);

		public RandomGameLogic(GameParams gameParams)
		{
			_gameParams = gameParams;
		}

		public Direction GetNextDirection(GameState state)
		{
			_currentRealState = state;
			_currentLogicState = ConvertRealGameStateToLogic(state, _gameParams.CellSize);

			var oppositeDirection = Me.Direction?.GetOpposite();
			var validDirections = Enum.GetValues(typeof(Direction)).Cast<Direction>().Where(c => c != oppositeDirection);
			var safeDirections = validDirections.Where(IsDirectionSafe).ToList();
			if (safeDirections.Count == 0)
			{
				return Direction.Left;
			}

			int index = _random.Next(0, safeDirections.Count);
			return safeDirections[index];
		}

		private bool IsDirectionSafe(Direction direction)
		{
			var nextPos = Me.Position.MoveLogic(direction);
			return _gameParams.MapLogicSize.ContainsPoint(nextPos) && !Me.Lines.Contains(nextPos) && GetMinPathLength(nextPos, Me.Territory, Me.Lines).HasValue;
		}

		private int? GetMinPathLength(Point startPoint, Point[] destination, Point[] obstacles)
		{
			var destinationHashSet = new HashSet<Point>(destination);
			var obstaclesHashSet = new HashSet<Point>(obstacles);
			var moves = new int[_gameParams.MapLogicSize.Width, _gameParams.MapLogicSize.Height];
			var isVisited = new bool[_gameParams.MapLogicSize.Width, _gameParams.MapLogicSize.Height];
			var queue = new Queue<Point>();
			queue.Enqueue(startPoint);
			isVisited[startPoint.X, startPoint.Y] = true;
			moves[startPoint.X, startPoint.Y] = 0;
			while (queue.Count > 0)
			{
				var point = queue.Dequeue();
				int currentPathLength = moves[point.X, point.Y];
				foreach (var neighbor in point.GetNeighbors())
				{
					if (destinationHashSet.Contains(neighbor))
					{
						return currentPathLength + 1;
					}

					if (_gameParams.MapLogicSize.ContainsPoint(neighbor) && !isVisited[neighbor.X, neighbor.Y] && !obstaclesHashSet.Contains(neighbor))
					{
						isVisited[neighbor.X, neighbor.Y] = true;
						moves[neighbor.X, neighbor.Y] = currentPathLength + 1;
						queue.Enqueue(neighbor);
					}
				}
			}

			return null;
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
