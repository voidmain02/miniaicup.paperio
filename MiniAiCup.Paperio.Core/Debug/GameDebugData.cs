using System;

namespace MiniAiCup.Paperio.Core.Debug
{
	public class GameDebugData
	{
		public static GameDebugData Current { get; } = new GameDebugData();

		public Point[] BestTrajectory { get; set; }

		public int[,] DangerousMap { get; set; }

		public int SimulationsCount { get; set; }

		public int ScoringsCount { get; set; }

		public int CaptureCount { get; set; }

		public TimeSpan UsedTime { get; set; }

		public void Reset()
		{
			BestTrajectory = null;
			DangerousMap = null;
			SimulationsCount = 0;
			ScoringsCount = 0;
			CaptureCount = 0;
			UsedTime = TimeSpan.Zero;
		}

		private GameDebugData()
		{
		}
	}
}
