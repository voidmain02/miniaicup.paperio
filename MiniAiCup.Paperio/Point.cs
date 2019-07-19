using System;

namespace MiniAiCup.Paperio
{
	/// <summary>
	/// Координата
	/// </summary>
	public struct Point : IEquatable<Point>
	{
		/// <summary>
		/// X-координата
		/// </summary>
		public int X { get; }

		/// <summary>
		/// Y-координата
		/// </summary>
		public int Y { get; }

		public Point(int x, int y)
		{
			X = x;
			Y = y;
		}

		public bool Equals(Point other)
		{
			return X == other.X && Y == other.Y;
		}

		public override bool Equals(object obj)
		{
			return obj is Point other && Equals(other);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return (X*397) ^ Y;
			}
		}

		public static bool operator ==(Point left, Point right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(Point left, Point right)
		{
			return !left.Equals(right);
		}
	}
}
