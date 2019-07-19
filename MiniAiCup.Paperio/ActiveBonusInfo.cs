namespace MiniAiCup.Paperio
{
	/// <summary>
	/// Информация о действующем бонусе
	/// </summary>
	public class ActiveBonusInfo
	{
		/// <summary>
		/// Тип
		/// </summary>
		public BonusType Type { get; set; }

		/// <summary>
		/// Оставшееся время действия бонуса
		/// </summary>
		public int RemainingTicks { get; set; }
	}
}
