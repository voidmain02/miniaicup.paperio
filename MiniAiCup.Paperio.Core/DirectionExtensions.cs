namespace MiniAiCup.Paperio.Core
{
	public static class DirectionExtensions
	{
		public static Direction GetOpposite(this Direction direction)
		{
			return (Direction)(((int)direction + 2)%4);
		}

		public static Direction GetMoved(this Direction direction, Move move)
		{
			return (Direction)(((int)direction + (int)move + 4)%4);
		}

		public static Move GetMoveTo(this Direction srcDirection, Direction dstDirection)
		{
			if (srcDirection == Direction.Down && dstDirection == Direction.Left)
			{
				return Move.Right;
			}
			if (srcDirection == Direction.Left && dstDirection == Direction.Down)
			{
				return Move.Left;
			}
			return (Move)((int)dstDirection - (int)srcDirection);
		}
	}
}
