using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using MiniAiCup.Paperio.Core;
using Newtonsoft.Json.Linq;

namespace MiniAiCup.Paperio.VisioPlayer
{
	class Program
	{
		static void Main(string[] args)
		{
			const int myPlayerIndex = 1;
			const int startTickNumber = 1;
			const string visioPath = "..\\..\\..\\..\\MiniAiCup.Paperio.Client\\bin\\Debug\\visio.gz";
			string json = Decompress(visioPath);
			var jData = (JArray)JObject.Parse(json)["visio_info"];

			var gameParams = ParseGameParams((JObject)jData.First);
			var gameStates = jData.Where(x => (string)x["type"] == "tick").Select(x => ParseGameState((JObject)x, myPlayerIndex));
			var decisionGameStates = gameStates.Where(IsDecisionState);

			Game.Initialize(gameParams);
			var game = new Game();
			foreach (var state in decisionGameStates.SkipWhile(x => x.TickNumber < startTickNumber))
			{
				game.GetNextDirection(state);
			}
		}

		private static bool IsDecisionState(GameState state)
		{
			var me = state.Players.FirstOrDefault(p => p.Id == "i");
			if (me == null)
			{
				return false;
			}

			const int cellSize = 30;

			return (me.Position.X + cellSize/2)%cellSize == 0 && (me.Position.Y + cellSize/2)%cellSize == 0;
		}

		private static GameParams ParseGameParams(JObject jParams)
		{
			return new GameParams {
				MapLogicSize = new Size((int)jParams["x_cells_count"], (int)jParams["y_cells_count"]),
				Speed = (int)jParams["speed"],
				CellSize = (int)jParams["width"]
			};
		}

		public static GameState ParseGameState(JObject jParams, int myPlayerIndex)
		{
			return new GameState {
				Players = jParams["players"].Cast<JProperty>().Select(x => ParsePlayer(x, myPlayerIndex)).ToArray(),
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

		private static PlayerInfo ParsePlayer(JProperty jIdentityPlayer, int myPlayerIndex)
		{
			string id = jIdentityPlayer.Name == myPlayerIndex.ToString() ? "i" : jIdentityPlayer.Name;
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

		private static string Decompress(string path)
		{
			using (var originalFileStream = File.OpenRead(path))
			{
				using (var decompressedFileStream = new MemoryStream())
				{
					using (var decompressionStream = new GZipStream(originalFileStream, CompressionMode.Decompress))
					{
						decompressionStream.CopyTo(decompressedFileStream);
					}

					decompressedFileStream.Seek(0, SeekOrigin.Begin);
					using (var reader = new StreamReader(decompressedFileStream, Encoding.UTF8))
					{
						return reader.ReadToEnd();
					}
				}
			}
		}
	}
}
