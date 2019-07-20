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

		private Command? _prevCommand;

		private PlayerInfo Me => _currentState.Players.First(p => p.Id == "i");

		private IEnumerable<PlayerInfo> Enemies => _currentState.Players.Where(p => p != Me);

		public RandomGameLogic(GameParams gameParams)
		{
			_gameParams = gameParams;
		}

		public Command GetNextCommand(GameState state)
		{
			_currentState = state;

			var oppositePrevCommand = _prevCommand.HasValue
				? GetOppositeCommand(_prevCommand.Value)
				: (Command?)null;

			var safeCommands = Enum.GetValues(typeof(Command)).Cast<Command>().Where(c => c != oppositePrevCommand && IsCommandSafe(c)).ToList();
			if (safeCommands.Count == 0)
			{
				return Command.Left;
			}

			int index = _random.Next(0, safeCommands.Count);
			_prevCommand = safeCommands[index];
			return _prevCommand.Value;
		}

		private static Command GetOppositeCommand(Command command)
		{
			switch (command)
			{
				case Command.Left: return Command.Right;
				case Command.Right: return Command.Left;
				case Command.Up: return Command.Down;
				case Command.Down: return Command.Up;
				default:
					throw new ArgumentOutOfRangeException(nameof(command), command, null);
			}
		}

		private bool IsCommandSafe(Command command)
		{
			var nextPos = GetNextPosition(command);
			return !IsPointOutsideOfMap(nextPos) && !Me.Lines.Contains(nextPos) && GetMinPathLength(nextPos, Me.Territory, Me.Lines).HasValue;
		}

		private bool IsPointOutsideOfMap(Point point)
		{
			int delta = _gameParams.CellSize/2;
			return point.X < delta || point.X > _gameParams.MapSize.Width*_gameParams.CellSize + delta ||
				point.Y < delta || point.Y > _gameParams.MapSize.Height*_gameParams.CellSize + delta;
		}

		private Point GetNextPosition(Command command)
		{
			switch (command)
			{
				case Command.Left: return new Point(Me.Position.X - _gameParams.CellSize, Me.Position.Y);
				case Command.Right: return new Point(Me.Position.X + _gameParams.CellSize, Me.Position.Y);
				case Command.Up: return new Point(Me.Position.X, Me.Position.Y + _gameParams.CellSize);
				case Command.Down: return new Point(Me.Position.X, Me.Position.Y - _gameParams.CellSize);
				default:
					throw new ArgumentOutOfRangeException(nameof(command), command, null);
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
			var moves = new int[_gameParams.MapSize.Width, _gameParams.MapSize.Height];
			var isVisited = new bool[_gameParams.MapSize.Width, _gameParams.MapSize.Height];
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

					if (IsPointInSize(neighbor, _gameParams.MapSize) && !isVisited[neighbor.X, neighbor.Y] && !obstaclesHashSet.Contains(neighbor))
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
