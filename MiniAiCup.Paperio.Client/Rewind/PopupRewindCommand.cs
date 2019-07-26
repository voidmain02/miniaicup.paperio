using MiniAiCup.Paperio.Core;

namespace MiniAiCup.Paperio.Client.Rewind
{
	public class PopupRewindCommand : RewindCommand
	{
		public Point Location { get; set; }

		public int Radius { get; set; }

		public string Text { get; set; }

		public override string Serialize()
		{
			return $"{{\"type\":\"popup\",\"x\":{Location.X},\"y\":{Location.Y},\"r\":{Radius},\"text\":\"{Text}\"}}";
		}
	}
}
