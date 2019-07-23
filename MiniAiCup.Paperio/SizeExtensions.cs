namespace MiniAiCup.Paperio
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
	}
}
