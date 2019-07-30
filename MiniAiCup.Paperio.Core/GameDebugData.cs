namespace MiniAiCup.Paperio.Core
{
	public class GameDebugData
	{
		public static GameDebugData Current { get; } = new GameDebugData();

		public Point[] PathToHome { get; set; }

		private GameDebugData()
		{
		}
	}
}
