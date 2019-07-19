using System;

namespace MiniAiCup.Paperio
{
	public class Program
	{
		public static void Main()
		{
			var commands = new string[4] { "left", "right", "up", "down" };
			var random = new Random();
			while (true)
			{
				string input = Console.ReadLine();
				int index = random.Next(0, commands.Length);
				Console.WriteLine("{{\"command\": \"{0}\"}}", commands[index]);
			}
		}
	}
}
