using System;
using MiniAiCup.Paperio.Core;

namespace MiniAiCup.Paperio.Client
{
	public class GameParamsParser
	{
		public static GameParams Parse(Message message)
		{
			if (message.Type != MessageType.StartGame)
			{
				throw new Exception("Ожидалось сообщение с типом start_game");
			}

			var jParams = message.Data;
			return new GameParams {
				MapLogicSize = new Size((int)jParams["x_cells_count"], (int)jParams["y_cells_count"]),
				Speed = (int)jParams["speed"],
				CellSize = (int)jParams["width"]
			};
		}
	}
}
