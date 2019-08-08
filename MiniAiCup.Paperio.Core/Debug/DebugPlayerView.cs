using System;

namespace MiniAiCup.Paperio.Core.Debug
{
	[Serializable]
	public class DebugPlayerView
	{
		public string Id { get; set; }

		public Point[] Territory { get; set; }

		public Point[] Tail { get; set; }

		public Point Position { get; set; }
	}
}
