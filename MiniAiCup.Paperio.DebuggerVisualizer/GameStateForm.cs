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
			var bgBrush = new SolidBrush(Color.FromArgb(220, 240, 244));
			e.Graphics.FillRectangle(bgBrush, 0, 0, 931, 931);

			var territoryColors = new[] {
				Color.FromArgb(90, 159, 153),
				Color.FromArgb(216, 27, 96),
				Color.FromArgb(96, 125, 139),
				Color.FromArgb(245, 124, 0),
				Color.FromArgb(92, 107, 192),
				Color.FromArgb(141, 110, 99)
			};

			var playerColors = new[] {
				Color.FromArgb(65, 134, 128),
				Color.FromArgb(191, 2, 71),
				Color.FromArgb(71, 100, 114),
				Color.FromArgb(220, 99, 0),
				Color.FromArgb(67, 82, 167),
				Color.FromArgb(116, 85, 74)
			};

			var lineColors = new[] {
				Color.FromArgb(138, 189, 187),
				Color.FromArgb(217, 106, 151),
				Color.FromArgb(142, 168, 178),
				Color.FromArgb(236, 167, 91),
				Color.FromArgb(140, 157, 211),
				Color.FromArgb(170, 158, 153)
			};

			var playersIndexes = new Dictionary<string, int>(_gameState.Players.Length);
			for (int i = 0; i < _gameState.Players.Length; i++)
			{
				playersIndexes.Add(_gameState.Players[i].Id, i);
			}

			foreach (var player in _gameState.Players)
			{
				int playerIndex = playersIndexes[player.Id];

				if (player.Territory.Any())
				{
					var territoryBrush = new SolidBrush(territoryColors[playerIndex]);
					e.Graphics.FillRectangles(territoryBrush, player.Territory.Select(p => GetCellSizeRectangle(TranslateCoordinates(p))).ToArray());
				}
				
				if (player.Lines.Any())
				{
					var lineBrush = new SolidBrush(lineColors[playerIndex]);
					e.Graphics.FillRectangles(lineBrush, player.Lines.Select(p => GetCellSizeRectangle(TranslateCoordinates(p))).ToArray());
				}

				var playerBrush = new SolidBrush(playerColors[playerIndex]);
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
