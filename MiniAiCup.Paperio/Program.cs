using System;

namespace MiniAiCup.Paperio
{
	public class Program
	{
		public static void Main()
		{
			var gameParams = GameParamsParser.Parse(Console.ReadLine());

			var logic = new RandomGameLogic(gameParams);

			while (true)
			{
				string input = Console.ReadLine();
				if (input == null)
				{
					break;
				}
				var gameState = GameStateParser.Parse(input);
				PushCommand(logic.GetNextDirection(gameState));
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
