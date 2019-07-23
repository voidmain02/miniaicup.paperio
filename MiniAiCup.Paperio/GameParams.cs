namespace MiniAiCup.Paperio
{
	/// <summary>
	/// Параметры игры
	/// </summary>
	public class GameParams
	{
		/// <summary>
		/// Размер карты в ячейках
		/// </summary>
		public Size MapLogicSize { get; set; }

		/// <summary>
		/// Скорость игрока
		/// </summary>
		public int Speed { get; set; }

		/// <summary>
		/// Ширина и высота элементарной ячейки
		/// </summary>
		public int CellSize { get; set; }
	}
}
