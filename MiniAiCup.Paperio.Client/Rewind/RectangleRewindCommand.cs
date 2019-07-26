using MiniAiCup.Paperio.Core;

namespace MiniAiCup.Paperio.Client.Rewind
{
	public class RectangleRewindCommand : GraphicRewindCommand
	{
		public Point Location { get; set; }

		public Size Size { get; set; }

		public override string Serialize()
		{
			var oppositePoint = new Point(Location.X + Size.Width, Location.Y + Size.Height);
			return $"{{\"type\":\"rectangle\",\"x1\":{Location.X},\"y1\":{Location.Y},\"x2\":{oppositePoint.X},\"y2\":{oppositePoint.Y},\"color\":{Color.ToArgb()},\"layer\":{Layer}}}";
		}
	}
}
