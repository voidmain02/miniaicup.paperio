using System;
using System.Collections.Generic;

namespace MiniAiCup.Paperio.Core
{
	public static class PointExtensions
	{
		public static IEnumerable<Point> GetNeighbors(this Point point)
		{
			yield return new Point(point.X - 1, point.Y);
			yield return new Point(point.X + 1, point.Y);
			yield return new Point(point.X, point.Y - 1);
			yield return new Point(point.X, point.Y + 1);
		}

		public static Point ConvertToLogic(this Point realPoint, int cellSize)
		{
			return new Point(realPoint.X/cellSize, realPoint.Y/cellSize);
		}

		public static Point ConvertToReal(this Point logicPoint, int cellSize)
		{
			int delta = cellSize/2;
			return new Point(logicPoint.X*cellSize + delta, logicPoint.Y*cellSize + delta);
		}

		public static Point MoveLogic(this Point point, Direction direction)
		{
			switch (direction)
			{
				case Direction.Left: return new Point(point.X - 1, point.Y);
				case Direction.Right: return new Point(point.X + 1, point.Y);
				case Direction.Up: return new Point(point.X, point.Y + 1);
				case Direction.Down: return new Point(point.X, point.Y - 1);
				default:
					throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
			}
		}

		public static Point MoveReal(this Point point, Direction direction, int cellSize)
		{
			switch (direction)
			{
				case Direction.Left: return new Point(point.X - cellSize, point.Y);
				case Direction.Right: return new Point(point.X + cellSize, point.Y);
				case Direction.Up: return new Point(point.X, point.Y + cellSize);
				case Direction.Down: return new Point(point.X, point.Y - cellSize);
				default:
					throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
			}
		}
	}
}
