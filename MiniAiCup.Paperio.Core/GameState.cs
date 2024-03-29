using System;

namespace MiniAiCup.Paperio.Core
{
	/// <summary>
	/// Состояние игры
	/// </summary>
	[Serializable]
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
