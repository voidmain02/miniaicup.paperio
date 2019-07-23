using System;
using Newtonsoft.Json.Linq;

namespace MiniAiCup.Paperio
{
	public class GameParamsParser
	{
		public static GameParams Parse(string json)
		{
			var jMessage = JObject.Parse(json);
			if ((string)jMessage["type"] != "start_game")
			{
				throw new Exception("Некорректный тип сообщения");
			}

			var jParams = jMessage["params"];
			return new GameParams {
				MapLogicSize = new Size((int)jParams["x_cells_count"], (int)jParams["y_cells_count"]),
				Speed = (int)jParams["speed"],
				CellSize = (int)jParams["width"]
			};
		}
	}
}
