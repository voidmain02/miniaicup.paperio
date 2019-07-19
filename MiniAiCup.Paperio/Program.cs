using System;

namespace MiniAiCup.Paperio
{
	public class Program
	{
		public static void Main()
		{
			var gameParams = GameParams.Load(Console.ReadLine());

			while (true)
			{
				var gameState = GameState.Load(Console.ReadLine());
				PushCommand(GetRandomCommand());
			}
		}

		private static Command GetRandomCommand()
		{
			var random = new Random();
			int index = random.Next(0, 4);
			return (Command)index;
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
