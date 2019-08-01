namespace MiniAiCup.Paperio.Core
{
	public class PlayerInternal
	{
		public string Id { get; set; }

		public int Score { get; set; }

		public PointsSet Territory { get; set; }

		public Point Position { get; set; }

		public Path Tail { get; set; }

		public ActiveBonusInfo[] Bonuses { get; set; }

		public Direction? Direction { get; set; }
	}
}
