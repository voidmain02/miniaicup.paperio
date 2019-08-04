using MiniAiCup.Paperio.Core;

namespace MiniAiCup.Paperio.Client.Rewind
{
	public class CellRewindCommand : GraphicRewindCommand
	{
		private readonly int _cellSize;

		public Point LogicPoint { get; set; }

		public CellRewindCommand(int cellSize)
		{
			_cellSize = cellSize;
		}

		public override string Serialize()
		{
			var rectangleCommand = new RectangleRewindCommand {
				Location = new Point(LogicPoint.X*_cellSize, LogicPoint.Y*_cellSize),
				Size = new Size(_cellSize, _cellSize),
				Color = Color,
				Layer = Layer
			};

			return rectangleCommand.Serialize();
		}
	}
}
