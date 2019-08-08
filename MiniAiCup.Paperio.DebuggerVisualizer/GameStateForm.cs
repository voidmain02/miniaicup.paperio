using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using MiniAiCup.Paperio.Core.Debug;

namespace MiniAiCup.Paperio.DebuggerVisualizer
{
	public partial class GameStateForm : Form
	{
		private readonly DebugStateView _gameState;

		public GameStateForm(DebugStateView gameState)
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

			var tailColors = new[] {
				Color.FromArgb(138, 189, 187),
				Color.FromArgb(217, 106, 151),
				Color.FromArgb(142, 168, 178),
				Color.FromArgb(236, 167, 91),
				Color.FromArgb(140, 157, 211),
				Color.FromArgb(170, 158, 153)
			};

			var playersIndexes = new Dictionary<string, int> {
				{ "i", 0 },
				{ "1", 0 },
				{ "2", 1 },
				{ "3", 2 },
				{ "4", 3 },
				{ "5", 4 },
				{ "6", 5 }
			};

			foreach (var player in _gameState.Players)
			{
				int playerIndex = playersIndexes[player.Id];

				if (player.Territory.Any())
				{
					var territoryBrush = new SolidBrush(territoryColors[playerIndex]);
					e.Graphics.FillRectangles(territoryBrush, player.Territory.Select(p => GetCellSizeRectangle(TranslateCoordinates(p, _gameState.Size), _gameState.CellSize)).ToArray());
				}
				
				if (player.Tail.Any())
				{
					var tailBrush = new SolidBrush(tailColors[playerIndex]);
					e.Graphics.FillRectangles(tailBrush, player.Tail.Select(p => GetCellSizeRectangle(TranslateCoordinates(p, _gameState.Size), _gameState.CellSize)).ToArray());
				}

				var playerBrush = new SolidBrush(playerColors[playerIndex]);
				e.Graphics.FillRectangle(playerBrush, GetCellSizeRectangle(TranslateCoordinates(player.Position, _gameState.Size), _gameState.CellSize));
			}
		}

		private static Core.Point TranslateCoordinates(Core.Point point, Core.Size mapSize)
		{
			return new Core.Point(point.X, mapSize.Height - point.Y - 1);
		}

		private static Rectangle GetCellSizeRectangle(Core.Point point, int cellSize)
		{
			return new Rectangle(point.X*cellSize, point.Y*cellSize, cellSize - 1, cellSize - 1);
		}
	}
}
