namespace MiniAiCup.Paperio.Core
{
	public class GameDebugData
	{
		public static GameDebugData Current { get; } = new GameDebugData();

		public GameParams GameParams { get; set; }

		public Point[] BestTrajectory { get; set; }

		public int[,] DangerousMap { get; set; }

		private GameDebugData()
		{
		}
	}
}
