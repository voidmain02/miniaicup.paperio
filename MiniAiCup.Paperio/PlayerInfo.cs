namespace MiniAiCup.Paperio
{
	/// <summary>
	/// Информация об игроке
	/// </summary>
	public class PlayerInfo
	{
		/// <summary>
		/// Идентификатор
		/// </summary>
		public string Id { get; set; }

		/// <summary>
		/// Количество очков
		/// </summary>
		public int Score { get; set;  }

		/// <summary>
		/// Захваченная территория
		/// </summary>
		public Point[] Territory { get; set; }

		/// <summary>
		/// Текущая позиция
		/// </summary>
		public Point Position { get; set; }

		/// <summary>
		/// Шлейф
		/// </summary>
		public Point[] Lines { get; set; }

		/// <summary>
		/// Активные бонусы
		/// </summary>
		public ActiveBonusInfo[] Bonuses { get; set; }
	}
}
