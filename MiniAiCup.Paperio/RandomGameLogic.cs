using System;
using System.Collections.Generic;
using System.Linq;

namespace MiniAiCup.Paperio
{
	public class RandomGameLogic : IGameLogic
	{
		private readonly GameParams _gameParams;

		private readonly Random _random = new Random();

		private GameState _currentState;

		private Direction? _prevDirection;

		private PlayerInfo Me => _currentState.Players.First(p => p.Id == "i");

		private IEnumerable<PlayerInfo> Enemies => _currentState.Players.Where(p => p != Me);

		public RandomGameLogic(GameParams gameParams)
		{
			_gameParams = gameParams;
		}

		public Direction GetNextDirection(GameState state)
		{
			_currentState = state;

			var oppositePrevCommand = _prevDirection.HasValue
				? GetOppositeDirection(_prevDirection.Value)
				: (Direction?)null;

			var safeCommands = Enum.GetValues(typeof(Direction)).Cast<Direction>().Where(c => c != oppositePrevCommand && IsCommandSafe(c)).ToList();
			if (safeCommands.Count == 0)
			{
				return Direction.Left;
			}

			int index = _random.Next(0, safeCommands.Count);
			_prevDirection = safeCommands[index];
			return _prevDirection.Value;
		}

		private static Direction GetOppositeDirection(Direction direction)
		{
			switch (direction)
			{
				case Direction.Left: return Direction.Right;
				case Direction.Right: return Direction.Left;
				case Direction.Up: return Direction.Down;
				case Direction.Down: return Direction.Up;
				default:
					throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
			}
		}

		private bool IsCommandSafe(Direction direction)
		{
			var nextPos = GetNextPosition(direction);
			return !IsPointOutsideOfMap(nextPos) && !Me.Lines.Contains(nextPos) && GetMinPathLength(nextPos, Me.Territory, Me.Lines).HasValue;
		}

		private bool IsPointOutsideOfMap(Point point)
		{
			int delta = _gameParams.CellSize/2;
			return point.X < delta || point.X > _gameParams.MapLogicSize.Width*_gameParams.CellSize + delta ||
				point.Y < delta || point.Y > _gameParams.MapLogicSize.Height*_gameParams.CellSize + delta;
		}

		private Point GetNextPosition(Direction direction)
		{
			switch (direction)
			{
				case Direction.Left: return new Point(Me.Position.X - _gameParams.CellSize, Me.Position.Y);
				case Direction.Right: return new Point(Me.Position.X + _gameParams.CellSize, Me.Position.Y);
				case Direction.Up: return new Point(Me.Position.X, Me.Position.Y + _gameParams.CellSize);
				case Direction.Down: return new Point(Me.Position.X, Me.Position.Y - _gameParams.CellSize);
				default:
					throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
			}
		}

		private static Point ConvertPointToLogical(Point realPoint, int cellSize)
		{
			return new Point(realPoint.X/cellSize, realPoint.Y/cellSize);
		}

		private int? GetMinPathLength(Point startPoint, Point[] destination, Point[] obstacles)
		{
			var logicalStartPoint = ConvertPointToLogical(startPoint, _gameParams.CellSize);

			var destinationHashSet = new HashSet<Point>(destination.Select(p => ConvertPointToLogical(p, _gameParams.CellSize)));
			var obstaclesHashSet = new HashSet<Point>(obstacles.Select(p => ConvertPointToLogical(p, _gameParams.CellSize)));
			var moves = new int[_gameParams.MapLogicSize.Width, _gameParams.MapLogicSize.Height];
			var isVisited = new bool[_gameParams.MapLogicSize.Width, _gameParams.MapLogicSize.Height];
			var queue = new Queue<Point>();
			queue.Enqueue(logicalStartPoint);
			isVisited[logicalStartPoint.X, logicalStartPoint.Y] = true;
			moves[logicalStartPoint.X, logicalStartPoint.Y] = 0;
			while (queue.Count > 0)
			{
				var point = queue.Dequeue();
				int currentPathLength = moves[point.X, point.Y];
				foreach (var neighbor in GetNeighbors(point))
				{
					if (destinationHashSet.Contains(neighbor))
					{
						return currentPathLength + 1;
					}

					if (IsPointInSize(neighbor, _gameParams.MapLogicSize) && !isVisited[neighbor.X, neighbor.Y] && !obstaclesHashSet.Contains(neighbor))
					{
						isVisited[neighbor.X, neighbor.Y] = true;
						moves[neighbor.X, neighbor.Y] = currentPathLength + 1;
						queue.Enqueue(neighbor);
					}
				}
			}

			return null;
		}

		private static IEnumerable<Point> GetNeighbors(Point point)
		{
			yield return new Point(point.X - 1, point.Y);
			yield return new Point(point.X + 1, point.Y);
			yield return new Point(point.X, point.Y - 1);
			yield return new Point(point.X, point.Y + 1);
		}

		private static bool IsPointInSize(Point point, Size size)
		{
			return point.X >= 0 && point.Y >= 0 && point.X < size.Width && point.Y < size.Height;
		}
	}
}
