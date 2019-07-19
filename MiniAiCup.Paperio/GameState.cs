using System;
using System.Linq;
using Newtonsoft.Json.Linq;

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

		/// <summary>
		/// Загрузить из JSON
		/// </summary>
		/// <param name="json">JSON с информацией о текущем состоянии игры</param>
		/// <returns>Текущее состояние игры</returns>
		public static GameState Load(string json)
		{
			var jMessage = JObject.Parse(json);
			if ((string)jMessage["type"] != "tick")
			{
				throw new Exception("Некорректный тип сообщения");
			}

			var jParams = jMessage["params"];
			return new GameState {
				Players = jParams["players"].Cast<JProperty>().Select(ParsePlayer).ToArray(),
				Bonuses = jParams["bonuses"].Cast<JObject>().Select(ParseBonus).ToArray(),
				TickNumber = (int)jParams["tick_num"]
			};
		}

		private static BonusInfo ParseBonus(JObject jBonus)
		{
			return new BonusInfo {
				Type = ParseBonusType((string)jBonus["type"]),
				Position = ParsePoint((JArray)jBonus["position"])
			};
		}

		private static PlayerInfo ParsePlayer(JProperty jIdentityPlayer)
		{
			string id = jIdentityPlayer.Name;
			var jPlayer = jIdentityPlayer.Value;
			return new PlayerInfo {
				Id = id,
				Score = (int)jPlayer["score"],
				Territory = jPlayer["territory"].Cast<JArray>().Select(ParsePoint).ToArray(),
				Position = ParsePoint((JArray)jPlayer["position"]),
				Lines = jPlayer["lines"].Cast<JArray>().Select(ParsePoint).ToArray(),
				Bonuses = jPlayer["bonuses"].Cast<JObject>().Select(ParseActiveBonus).ToArray()
			};
		}

		private static ActiveBonusInfo ParseActiveBonus(JObject jActiveBonus)
		{
			return new ActiveBonusInfo {
				Type = ParseBonusType((string)jActiveBonus["type"]),
				RemainingTicks = (int)jActiveBonus["ticks"]
			};
		}

		private static BonusType ParseBonusType(string sType)
		{
			switch (sType)
			{
				case "n": return BonusType.Nitro;
				case "s": return BonusType.Slowdown;
				case "saw": return BonusType.Saw;
				default: throw new ArgumentOutOfRangeException(nameof(sType), sType, null);
			}
		}

		private static Point ParsePoint(JArray jPointArray)
		{
			return new Point((int)jPointArray[0], (int)jPointArray[1]);
		}
	}
}
