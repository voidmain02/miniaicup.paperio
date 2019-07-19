using System;

namespace MiniAiCup.Paperio
{
	public class Program
	{
		public static void Main()
		{
			var gameParams = GameParams.Load(Console.ReadLine());

			var logic = new RandomGameLogic(gameParams);

			while (true)
			{
				string input = Console.ReadLine();
				if (input == null)
				{
					break;
				}
				var gameState = GameState.Load(input);
				PushCommand(logic.GetNextCommand(gameState));
			}
		}

		private static void PushCommand(Command command)
		{
			Console.WriteLine($"{{\"command\": \"{CommandToString(command)}\"}}");
		}

		private static string CommandToString(Command command)
		{
			switch (command)
			{
				case Command.Left: return "left";
				case Command.Right: return "right";
				case Command.Up: return "up";
				case Command.Down: return "down";
				default:
					throw new ArgumentOutOfRangeException(nameof(command), command, null);
			}
		}
	}
}
