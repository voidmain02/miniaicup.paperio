using System;
using System.Drawing;
using System.Linq;
using MiniAiCup.Paperio.Client.Rewind;
using MiniAiCup.Paperio.Core;
using ColorConverter = MiniAiCup.Paperio.Client.Rewind.ColorConverter;
using Point = MiniAiCup.Paperio.Core.Point;

namespace MiniAiCup.Paperio.Client
{
	public class Program
	{
		public static void Main()
		{
			var message = ReadMessage();
			var gameParams = GameParamsParser.Parse(message);
			var game = new Game(gameParams);

			while (true)
			{
				message = ReadMessage();
				if (message == null || message.Type == MessageType.EndGame)
				{
					break;
				}

				var gameState = GameStateParser.Parse(message);

#if DEBUG
				var nextDirection = game.GetNextDirection(gameState);
				PushCommandWithRewind(nextDirection, GameDebugData.Current);
#else
				try
				{
					var nextDirection = game.GetNextDirection(gameState);
					PushCommand(nextDirection);
				}
				catch (Exception e)
				{
					PushErrorInfo(e);
					throw;
				}
#endif
			}
		}

		private static Message ReadMessage()
		{
			string input = Console.ReadLine();
			return input == null ? null : Message.Load(input);
		}

		private static void PushCommandWithRewind(Direction direction, GameDebugData debugData)
		{
			string commandText = $"{{\"command\":\"{DirectionToString(direction)}\"";
			if (debugData != null)
			{
				commandText += $", \"rewind\":{BuildRewindData(debugData)}";
			}

			commandText += "}";
			Console.WriteLine(commandText);
		}

		private static void PushCommand(Direction direction)
		{
			Console.WriteLine($"{{\"command\":\"{DirectionToString(direction)}\"}}");
		}

		private static void PushErrorInfo(Exception e)
		{
			Console.WriteLine($"{{\"debug\":\"{e}\"}}");
		}

		private static string BuildRewindData(GameDebugData debugData)
		{
			var builder = new RewindBuilder();

			if (debugData.BestTrajectory != null)
			{
				const int minOpacity = 20;
				for (int i = 0; i < debugData.BestTrajectory.Length; i++)
				{
					var p = debugData.BestTrajectory[i];
					builder.Add(new CircleRewindCommand {
						Center = p,
						Radius = 7,
						Color = Color.FromArgb(255 - i*((255 - minOpacity)/debugData.BestTrajectory.Length), 6, 141, 209)
					});
				}
			}

			if (debugData.DangerousMap != null)
			{
				const double maxHue = 100.0/360.0;

				int maxValue = 0;
				for (int y = 0; y < debugData.GameParams.MapLogicSize.Height; y++)
				{
					for (int x = 0; x < debugData.GameParams.MapLogicSize.Width; x++)
					{
						if (debugData.DangerousMap[x, y] != Int32.MaxValue && debugData.DangerousMap[x, y] > maxValue)
						{
							maxValue = debugData.DangerousMap[x, y];
						}
					}
				}
				for (int y = 0; y < debugData.GameParams.MapLogicSize.Height; y++)
				{
					for (int x = 0; x < debugData.GameParams.MapLogicSize.Width; x++)
					{
						if (debugData.DangerousMap[x, y] > debugData.GameParams.MapLogicSize.Width*debugData.GameParams.MapLogicSize.Height)
						{
							continue;
						}

						double hue = (double)debugData.DangerousMap[x, y]/(double)maxValue*maxHue;

						builder.Add(new CellRewindCommand(debugData.GameParams.CellSize) {
							LogicPoint = new Point(x, y),
							Color = ColorConverter.FromHsla(hue, 1.0, 0.5, 0.6),
							Layer = 3
						});
						builder.Add(new PopupRewindCommand {
							Location = new Point(x, y).ConvertToReal(debugData.GameParams.CellSize),
							Radius = debugData.GameParams.CellSize/2,
							Text = $"dang: {debugData.DangerousMap[x, y]}"
						});
					}
				}
			}

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
