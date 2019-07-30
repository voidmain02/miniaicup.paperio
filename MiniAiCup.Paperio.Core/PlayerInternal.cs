using System.Collections.Generic;

namespace MiniAiCup.Paperio.Core
{
	public class PlayerInternal
	{
		public string Id { get; set; }

		public int Score { get; set; }

		public HashSet<Point> Territory { get; set; }

		public Point Position { get; set; }

		public HashSet<Point> Lines { get; set; }

		public ActiveBonusInfo[] Bonuses { get; set; }

		public Direction? Direction { get; set; }
	}
}
