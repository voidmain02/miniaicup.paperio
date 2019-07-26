using System;
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

			while (true)
			{
				input = Console.ReadLine();
				if (input == null)
				{
					break;
				}
				var gameState = GameStateParser.Parse(input);
				PushCommand(game.GetNextDirection(gameState));
			}
		}

		private static void PushCommand(Direction direction)
		{
			Console.WriteLine($"{{\"command\": \"{DirectionToString(direction)}\"}}");
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
