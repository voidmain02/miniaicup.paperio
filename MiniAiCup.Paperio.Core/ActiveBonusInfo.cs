using System;

namespace MiniAiCup.Paperio.Core
{
	/// <summary>
	/// Информация о действующем бонусе
	/// </summary>
	[Serializable]
	public class ActiveBonusInfo : ICloneable
	{
		/// <summary>
		/// Тип
		/// </summary>
		public BonusType Type { get; set; }

		/// <summary>
		/// Оставшееся время действия бонуса
		/// </summary>
		public int RemainingTicks { get; set; }

		public object Clone()
		{
			return new ActiveBonusInfo {
				Type = Type,
				RemainingTicks = RemainingTicks
			};
		}
	}
}
