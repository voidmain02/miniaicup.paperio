using MiniAiCup.Paperio.Core;

namespace MiniAiCup.Paperio.Client.Rewind
{
	public class LineRewindCommand : GraphicRewindCommand
	{
		public Point StartPoint { get; set; }

		public Point EndPoint { get; set; }

		public override string Serialize()
		{
			return $"{{\"type\":\"line\",\"x1\":{StartPoint.X},\"y1\":{StartPoint.Y},\"x2\":{EndPoint.X},\"y2\":{EndPoint.Y},\"color\":{Color.ToArgb()},\"layer\":{Layer}}}";
		}
	}
}
