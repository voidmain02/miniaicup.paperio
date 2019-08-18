using System;

namespace MiniAiCup.Paperio.Core
{
	/// <summary>
	/// Информация о бонусе на карте
	/// </summary>
	[Serializable]
	public class BonusInfo
	{
		/// <summary>
		/// Тип
		/// </summary>
		public BonusType Type { get; set; }

		/// <summary>
		/// Позиция
		/// </summary>
		public Point Position { get; set; }

		/// <summary>
		/// Продолжительность бонуса в шагах
		/// </summary>
		public int Steps { get; set; }
	}
}
