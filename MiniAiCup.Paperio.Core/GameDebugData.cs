using System.Collections.Generic;

namespace MiniAiCup.Paperio.Core
{
	public class GameDebugData
	{
		public static GameDebugData Current { get; } = new GameDebugData();

		public Point[] PathToHome { get; set; }

		public Dictionary<Move, float> MoveScores { get; set; }

		private GameDebugData()
		{
		}
	}
}
