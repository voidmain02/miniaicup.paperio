using MiniAiCup.Paperio.Core;

namespace MiniAiCup.Paperio.Client.Rewind
{
	public class CircleRewindCommand : GraphicRewindCommand
	{
		public Point Center { get; set; }

		public int Radius { get; set; }

		public override string Serialize()
		{
			return $"{{\"type\":\"circle\",\"x\":{Center.X},\"y\":{Center.Y},\"r\":{Radius},\"color\":{Color.ToArgb()},\"layer\":{Layer}}}";
		}
	}
}
