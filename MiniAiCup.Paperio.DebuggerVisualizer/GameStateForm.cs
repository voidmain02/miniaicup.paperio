using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using MiniAiCup.Paperio.Core;
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
				Color.FromArgb(141, 110, 99),
				Color.FromArgb(90, 218, 26)
			};

			var playerColors = new[] {
				Color.FromArgb(65, 134, 128),
				Color.FromArgb(191, 2, 71),
				Color.FromArgb(71, 100, 114),
				Color.FromArgb(220, 99, 0),
				Color.FromArgb(67, 82, 167),
				Color.FromArgb(116, 85, 74),
				Color.FromArgb(65, 192, 2)
			};

			var tailColors = new[] {
				Color.FromArgb(138, 189, 187),
				Color.FromArgb(217, 106, 151),
				Color.FromArgb(142, 168, 178),
				Color.FromArgb(236, 167, 91),
				Color.FromArgb(140, 157, 211),
				Color.FromArgb(170, 158, 153),
				Color.FromArgb(125, 218, 107)
			};

			var playersIndexes = new Dictionary<string, int> {
				{ "1", 0 },
				{ "2", 1 },
				{ "3", 2 },
				{ "4", 3 },
				{ "5", 4 },
				{ "6", 5 },
				{ "i", 6 }
			};

			foreach (var player in _gameState.Players)
			{
				if (player.Territory.Any())
				{
					var territoryBrush = new SolidBrush(territoryColors[playersIndexes[player.Id]]);
					e.Graphics.FillRectangles(territoryBrush, player.Territory.Select(p => GetCellSizeRectangle(TranslateCoordinates(p, _gameState.Size), _gameState.CellSize)).ToArray());
				}
			}
			foreach (var player in _gameState.Players)
			{
				if (player.Tail.Any())
				{
					var tailBrush = new SolidBrush(tailColors[playersIndexes[player.Id]]);
					e.Graphics.FillRectangles(tailBrush, player.Tail.Select(p => GetCellSizeRectangle(TranslateCoordinates(p, _gameState.Size), _gameState.CellSize)).ToArray());
				}
			}
			foreach (var player in _gameState.Players)
			{
				var playerBrush = new SolidBrush(playerColors[playersIndexes[player.Id]]);
				e.Graphics.FillRectangle(playerBrush, GetCellSizeRectangle(TranslateCoordinates(player.Position, _gameState.Size), player.Direction, player.PathToNextPositionLength, _gameState.CellSize));
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

		private static Rectangle GetCellSizeRectangle(Core.Point point, Direction? direction, int pathToNextPosLength, int cellSize)
		{
			int dx = 0;
			int dy = 0;
			switch (direction)
			{
				case Direction.Left:
					dx = -pathToNextPosLength;
					break;
				case Direction.Up:
					dy = -pathToNextPosLength;
					break;
				case Direction.Right:
					dx = pathToNextPosLength;
					break;
				case Direction.Down:
					dy = pathToNextPosLength;
					break;
				case null:
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
			}

			return new Rectangle(point.X*cellSize + dx, point.Y*cellSize + dy, cellSize - 1, cellSize - 1);
		}
	}
}
