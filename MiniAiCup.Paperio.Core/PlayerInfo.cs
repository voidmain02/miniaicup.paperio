using System;

namespace MiniAiCup.Paperio.Core
{
	/// <summary>
	/// Информация об игроке
	/// </summary>
	[Serializable]
	public class PlayerInfo
	{
		/// <summary>
		/// Идентификатор
		/// </summary>
		public string Id { get; set; }

		/// <summary>
		/// Количество очков
		/// </summary>
		public int Score { get; set; }

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

		/// <summary>
		/// Направление
		/// </summary>
		public Direction? Direction { get; set; }
	}
}
