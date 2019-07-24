using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using MiniAiCup.Paperio.Core;

namespace MiniAiCup.Paperio.DebuggerVisualizer
{
	public partial class GameStateForm : Form
	{
		private readonly GameState _gameState;

		private const int WidthInCells = 31;
		private const int HeightInCells = 31;
		private const int CellSize = 30;

		public GameStateForm(GameState gameState)
		{
			_gameState = gameState;
			InitializeComponent();
		}

		private void PictureBox_Paint(object sender, PaintEventArgs e)
		{
			var bgBrush = new SolidBrush(Color.FromArgb(212, 223, 235));
			e.Graphics.FillRectangle(bgBrush, 0, 0, 931, 931);

			var territoryColors = new[] {
				Color.FromArgb(251, 128, 114),
				Color.FromArgb(128, 177, 211),
				Color.FromArgb(141, 211, 199),
				Color.FromArgb(249, 234, 129),
				Color.FromArgb(190, 186, 218),
				Color.FromArgb(252, 205, 229)
			};

			var playerColors = new[] {
				Color.FromArgb(239, 71, 50),
				Color.FromArgb(42, 141, 212),
				Color.FromArgb(54, 191, 166),
				Color.FromArgb(247, 222, 63),
				Color.FromArgb(101, 95, 170),
				Color.FromArgb(252, 124, 190)
			};

			var playersIndexes = new Dictionary<string, int>(_gameState.Players.Length);
			for (int i = 0; i < _gameState.Players.Length; i++)
			{
				playersIndexes.Add(_gameState.Players[i].Id, i);
			}

			foreach (var player in _gameState.Players)
			{
				var territoryBrush = new SolidBrush(territoryColors[playersIndexes[player.Id]]);
				e.Graphics.FillRectangles(territoryBrush, player.Territory.Select(p => GetCellSizeRectangle(TranslateCoordinates(p))).ToArray());

				foreach (var linePoint in player.Lines)
				{
					var translatedLinePoint = TranslateCoordinates(linePoint);
					e.Graphics.FillEllipse(territoryBrush, translatedLinePoint.X - CellSize/4, translatedLinePoint.Y - CellSize/4, CellSize/2 - 1, CellSize/2 - 1);
				}

				var playerBrush = new SolidBrush(playerColors[playersIndexes[player.Id]]);
				e.Graphics.FillRectangle(playerBrush, GetCellSizeRectangle(TranslateCoordinates(player.Position)));
			}
		}

		private static Core.Point TranslateCoordinates(Core.Point point)
		{
			return new Core.Point(point.X, HeightInCells*CellSize - point.Y - 1);
		}

		private static Rectangle GetCellSizeRectangle(Core.Point center)
		{
			return new Rectangle(center.X - CellSize/2, center.Y - CellSize/2, CellSize - 1, CellSize - 1);
		}
	}
}
