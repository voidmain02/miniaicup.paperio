namespace MiniAiCup.Paperio.Core
{
	public static class SizeExtensions
	{
		public static bool ContainsPoint(this Size size, Point point)
		{
			return point.X >= 0 && point.Y >= 0 && point.X < size.Width && point.Y < size.Height;
		}

		public static Size ConvertToLogic(this Size realSize, int cellSize)
		{
			return new Size(realSize.Width/cellSize, realSize.Height/cellSize);
		}

		public static Size ConvertToReal(this Size logicSize, int cellSize)
		{
			return new Size(logicSize.Width*cellSize, logicSize.Height*cellSize);
		}

		public static PointsSet GetAllLogicPoints(this Size size)
		{
			var points = new Point[size.Width*size.Height];
			for (int y = 0; y < size.Height; y++)
			{
				for (int x = 0; x < size.Width; x++)
				{
					points[size.Width*y + x] = new Point(x, y);
				}
			}

			return new PointsSet(points);
		}
	}
}
