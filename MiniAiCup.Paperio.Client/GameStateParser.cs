using System;
using System.Linq;
using MiniAiCup.Paperio.Core;
using Newtonsoft.Json.Linq;

namespace MiniAiCup.Paperio.Client
{
	public class GameStateParser
	{
		public static GameState Parse(Message message)
		{
			if (message.Type != MessageType.Tick)
			{
				throw new Exception("Ожидалось сообщение с типом tick");
			}

			var jParams = message.Data;
			return new GameState {
				Players = jParams["players"].Cast<JProperty>().Select(ParsePlayer).ToArray(),
				Bonuses = jParams["bonuses"].Cast<JObject>().Select(ParseBonus).ToArray(),
				TickNumber = (int)jParams["tick_num"]
			};
		}

		private static BonusInfo ParseBonus(JObject jBonus)
		{
			var bonusInfo = new BonusInfo {
				Type = ParseBonusType((string)jBonus["type"]),
				Position = ParsePoint((JArray)jBonus["position"])
			};

			var jActiveTicks = jBonus["active_ticks"];
			if (jActiveTicks != null)
			{
				int activeTicks = (int)jActiveTicks;
				if (activeTicks > 0)
				{
					bonusInfo.Steps = activeTicks;
				}
				else
				{
					SetDefaultBonusSteps(bonusInfo);
				}
			}
			else
			{
				SetDefaultBonusSteps(bonusInfo);
			}

			return bonusInfo;
		}

		private static void SetDefaultBonusSteps(BonusInfo bonusInfo)
		{
			switch (bonusInfo.Type)
			{
				case BonusType.Nitro:
					bonusInfo.Steps = GameParams.MinBonusDuration;
					break;
				case BonusType.Slowdown:
					bonusInfo.Steps = GameParams.MaxBonusDuration;
					break;
				case BonusType.Saw:
					bonusInfo.Steps = 0;
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
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
				Bonuses = jPlayer["bonuses"].Cast<JObject>().Select(ParseActiveBonus).ToArray(),
				Direction = ParseDirection((string)jPlayer["direction"])
			};
		}

		private static Direction? ParseDirection(string sDirection)
		{
			switch (sDirection)
			{
				case null: return null;
				case "left": return Direction.Left;
				case "right": return Direction.Right;
				case "up": return Direction.Up;
				case "down": return Direction.Down;
				default: throw new ArgumentOutOfRangeException(nameof(sDirection), sDirection, null);
			}
		}

		private static ActiveBonusInfo ParseActiveBonus(JObject jActiveBonus)
		{
			return new ActiveBonusInfo {
				Type = ParseBonusType((string)jActiveBonus["type"]),
				RemainingSteps = (int)jActiveBonus["ticks"]
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
