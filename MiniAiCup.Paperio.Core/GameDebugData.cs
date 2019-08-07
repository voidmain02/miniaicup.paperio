using System;

namespace MiniAiCup.Paperio.Core
{
	public class GameDebugData
	{
		public static GameDebugData Current { get; } = new GameDebugData();

		public GameParams GameParams { get; set; }

		public Point[] BestTrajectory { get; set; }

		public int[,] DangerousMap { get; set; }

		public int SimulationsCount { get; set; }

		public int ScoringsCount { get; set; }

		public TimeSpan UsedTime { get; set; }

		public void Reset()
		{
			BestTrajectory = null;
			DangerousMap = null;
			SimulationsCount = 0;
			ScoringsCount = 0;
			UsedTime = TimeSpan.Zero;
		}

		private GameDebugData()
		{
		}
	}
}
