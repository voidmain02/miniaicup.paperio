using System;
using Newtonsoft.Json.Linq;

namespace MiniAiCup.Paperio
{
	/// <summary>
	/// Параметры игры
	/// </summary>
	public class GameParams
	{
		/// <summary>
		/// Количество элементарных ячеек по оси x
		/// </summary>
		public int Width { get; set; }

		/// <summary>
		/// Количество элементарных ячеек по оси y
		/// </summary>
		public int Height { get; set; }

		/// <summary>
		/// Скорость игрока
		/// </summary>
		public int Speed { get; set; }

		/// <summary>
		/// Ширина и высота элементарной ячейки
		/// </summary>
		public int CellSize { get; set; }

		/// <summary>
		/// Загрузить параметры игры из JSON
		/// </summary>
		/// <param name="json">JSON с параметрами игры</param>
		/// <returns>Параметры игры</returns>
		public static GameParams Load(string json)
		{
			var jMessage = JObject.Parse(json);
			if ((string)jMessage["type"] != "start_game")
			{
				throw new Exception("Некорректный тип сообщения");
			}

			var jParams = jMessage["params"];
			return new GameParams {
				Width = (int)jParams["x_cells_count"],
				Height = (int)jParams["y_cells_count"],
				Speed = (int)jParams["speed"],
				CellSize = (int)jParams["width"]
			};
		}
	}
}
