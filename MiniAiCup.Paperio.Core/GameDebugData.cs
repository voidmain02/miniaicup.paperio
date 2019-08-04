using System.Collections.Generic;

namespace MiniAiCup.Paperio.Core
{
	public class GameDebugData
	{
		public static GameDebugData Current { get; } = new GameDebugData();

		public GameParams GameParams { get; set; }

		public Point[] PathToHome { get; set; }

		public Dictionary<Move, int> MoveScores { get; set; }

		public int[,] DangerousMap { get; set; }

		private GameDebugData()
		{
		}
	}
}
