using System;
using System.Collections.Generic;

namespace MiniAiCup.Paperio.Core
{
	public static class PointExtensions
	{
		public static IEnumerable<Point> GetNeighbors(this Point point)
		{
			yield return new Point(point.X - 1, point.Y);
			yield return new Point(point.X, point.Y + 1);
			yield return new Point(point.X + 1, point.Y);
			yield return new Point(point.X, point.Y - 1);
		}

		public static IEnumerable<Point> GetEightNeighbors(this Point point)
		{
			yield return new Point(point.X - 1, point.Y);
			yield return new Point(point.X - 1, point.Y + 1);
			yield return new Point(point.X, point.Y + 1);
			yield return new Point(point.X + 1, point.Y + 1);
			yield return new Point(point.X + 1, point.Y);
			yield return new Point(point.X + 1, point.Y - 1);
			yield return new Point(point.X, point.Y - 1);
			yield return new Point(point.X - 1, point.Y - 1);
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
				case Direction.Up: return new Point(point.X, point.Y + 1);
				case Direction.Right: return new Point(point.X + 1, point.Y);
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
				case Direction.Up: return new Point(point.X, point.Y + cellSize);
				case Direction.Right: return new Point(point.X + cellSize, point.Y);
				case Direction.Down: return new Point(point.X, point.Y - cellSize);
				default:
					throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
			}
		}

		public static bool IsNeighbor(this Point point1, Point point2)
		{
			return point1.X == point2.X && Math.Abs(point1.Y - point2.Y) == 1 ||
				point1.Y == point2.Y && Math.Abs(point1.X - point2.X) == 1;
		}

		public static int GetDistanceTo(this Point src, Point dst)
		{
			return Math.Abs(src.X - dst.X) + Math.Abs(src.Y - dst.Y);
		}

		public static int GetDistanceTo(this Point src, Point dst, Direction? direction)
		{
			int distance = src.GetDistanceTo(dst);

			if (dst.X == src.X && (dst.Y > src.Y && direction == Direction.Down || dst.Y < src.Y && direction == Direction.Up) ||
				dst.Y == src.Y && (dst.X > src.X && direction == Direction.Left || dst.X < src.X && direction == Direction.Right))
			{
				distance += 2;
			}

			return distance;
		}

		public static Direction GetDirectionTo(this Point src, Point dst)
		{
			if (dst.X == src.X)
			{
				return dst.Y > src.Y
					? Direction.Up
					: Direction.Down;
			}

			return dst.X > src.X
				? Direction.Right
				: Direction.Left;
		}
	}
}
