namespace MiniAiCup.Paperio
{
	/// <summary>
	/// Состояние игры
	/// </summary>
	public class GameState
	{
		/// <summary>
		/// Игроки
		/// </summary>
		public PlayerInfo[] Players { get; set; }

		/// <summary>
		/// Бонусы на карте
		/// </summary>
		public BonusInfo[] Bonuses { get; set; }

		/// <summary>
		/// Номер текущего тика
		/// </summary>
		public int TickNumber { get; set; }
	}
}
