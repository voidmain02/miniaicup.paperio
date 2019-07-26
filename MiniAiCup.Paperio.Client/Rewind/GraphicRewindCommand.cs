using System.Drawing;

namespace MiniAiCup.Paperio.Client.Rewind
{
	public abstract class GraphicRewindCommand : RewindCommand
	{
		public Color Color { get; set; }

		public int Layer { get; set; } = 2;
	}
}
