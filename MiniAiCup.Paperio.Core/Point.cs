using System;

namespace MiniAiCup.Paperio.Core
{
	/// <summary>
	/// Координата
	/// </summary>
	[Serializable]
	public struct Point : IEquatable<Point>
	{
		/// <summary>
		/// X-координата
		/// </summary>
		public int X;

		/// <summary>
		/// Y-координата
		/// </summary>
		public int Y;

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
				return X*397 ^ Y;
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

		public override string ToString()
		{
			return $"[{X}, {Y}]";
		}
	}
}
