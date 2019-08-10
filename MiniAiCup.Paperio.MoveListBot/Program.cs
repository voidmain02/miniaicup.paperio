using System;
using System.IO;
using System.Linq;
using MiniAiCup.Paperio.Client;
using MiniAiCup.Paperio.Core;

namespace MiniAiCup.Paperio.MoveListBot
{
	class Program
	{
		static void Main(string[] args)
		{
			var commands = File.ReadAllLines(args[0]).Select(s => (Direction)Enum.Parse(typeof(Direction), s)).ToList();

			ReadMessage();

			int index = 0;

			while (true)
			{
				var message = ReadMessage();
				if (message == null || message.Type == MessageType.EndGame || index == commands.Count)
				{
					break;
				}

				PushCommand(commands[index++]);
			}
		}

		private static Message ReadMessage()
		{
			string input = Console.ReadLine();
			return input == null ? null : Message.Load(input);
		}

		private static void PushCommand(Direction direction)
		{
			Console.WriteLine($"{{\"command\":\"{DirectionToString(direction)}\"}}");
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
