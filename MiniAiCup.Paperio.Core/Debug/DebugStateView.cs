using System;

namespace MiniAiCup.Paperio.Core.Debug
{
	[Serializable]
	public class DebugStateView
	{
		public Size Size { get; set; }

		public int CellSize { get; set; }

		public DebugPlayerView[] Players { get; set; }
	}
}
