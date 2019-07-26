using System;
using System.Drawing;
using System.Linq;
using MiniAiCup.Paperio.Client.Rewind;
using MiniAiCup.Paperio.Core;

namespace MiniAiCup.Paperio.Client
{
	public class Program
	{
		public static void Main()
		{
			string input = Console.ReadLine();
			var gameParams = GameParamsParser.Parse(input);

			var game = new Game(gameParams);

			input = Console.ReadLine();
			PushCommand(Direction.Left);

			while (true)
			{
				input = Console.ReadLine();
				if (input == null)
				{
					break;
				}
				var gameState = GameStateParser.Parse(input);
				var nextDirection = game.GetNextDirection(gameState, out var debugData);
				PushCommand(nextDirection, debugData);
			}
		}

		private static void PushCommand(Direction direction, GameDebugData debugData = null)
		{
			string commandText = $"{{\"command\":\"{DirectionToString(direction)}\"";
			if (debugData != null)
			{
				commandText += $", \"rewind\":{BuildRewindData(debugData)}";
			}

			commandText += "}";
			Console.WriteLine(commandText);
		}

		private static string BuildRewindData(GameDebugData debugData)
		{
			var builder = new RewindBuilder();

			builder.AddRange(debugData.PathToHome.Select(p => new CircleRewindCommand {
				Center = p,
				Radius = 7,
				Color = Color.FromArgb(6, 141, 209)
			}));

			return builder.ToString();
		}

		private static string DirectionToString(Direction direction)
		{
			switch (direction)
			{
				case Direction.Left: return "left";
				case Direction.Right: return "right";
				case Direction.Up: return "up";
				case Direction.Down: return "down";
				default:
					throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
			}
		}
	}
}
