using System;

namespace MiniAiCup.Paperio
{
	public static class DirectionExtensions
	{
		public static Direction GetOpposite(this Direction direction)
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
	}
}
