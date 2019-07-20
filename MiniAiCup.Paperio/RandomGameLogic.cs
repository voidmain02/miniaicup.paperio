using System;
using System.Collections.Generic;
using System.Linq;

namespace MiniAiCup.Paperio
{
	public class RandomGameLogic : IGameLogic
	{
		private readonly GameParams _gameParams;

		private readonly Random _random = new Random();

		private GameState _currentState;

		private Command? _prevCommand;

		private PlayerInfo Me => _currentState.Players.First(p => p.Id == "i");

		private IEnumerable<PlayerInfo> Enemies => _currentState.Players.Where(p => p != Me);

		public RandomGameLogic(GameParams gameParams)
		{
			_gameParams = gameParams;
		}

		public Command GetNextCommand(GameState state)
		{
			_currentState = state;

			var oppositePrevCommand = _prevCommand.HasValue
				? GetOppositeCommand(_prevCommand.Value)
				: (Command?)null;

			var safeCommands = Enum.GetValues(typeof(Command)).Cast<Command>().Where(c => c != oppositePrevCommand && IsCommandSafe(c)).ToList();
			if (safeCommands.Count == 0)
			{
				return Command.Left;
			}

			int index = _random.Next(0, safeCommands.Count);
			_prevCommand = safeCommands[index];
			return _prevCommand.Value;
		}

		private static Command GetOppositeCommand(Command command)
		{
			switch (command)
			{
				case Command.Left: return Command.Right;
				case Command.Right: return Command.Left;
				case Command.Up: return Command.Down;
				case Command.Down: return Command.Up;
				default:
					throw new ArgumentOutOfRangeException(nameof(command), command, null);
			}
		}

		private bool IsCommandSafe(Command command)
		{
			var nextPos = GetNextPosition(command);
			return !IsPointOutsideOfMap(nextPos) && !Me.Lines.Contains(nextPos);
		}

		private bool IsPointOutsideOfMap(Point point)
		{
			int delta = _gameParams.CellSize/2;
			return point.X < delta || point.X > _gameParams.MapSize.Width*_gameParams.CellSize + delta ||
				point.Y < delta || point.Y > _gameParams.MapSize.Height*_gameParams.CellSize + delta;
		}

		private Point GetNextPosition(Command command)
		{
			switch (command)
			{
				case Command.Left: return new Point(Me.Position.X - _gameParams.CellSize, Me.Position.Y);
				case Command.Right: return new Point(Me.Position.X + _gameParams.CellSize, Me.Position.Y);
				case Command.Up: return new Point(Me.Position.X, Me.Position.Y + _gameParams.CellSize);
				case Command.Down: return new Point(Me.Position.X, Me.Position.Y - _gameParams.CellSize);
				default:
					throw new ArgumentOutOfRangeException(nameof(command), command, null);
			}
		}
	}
}
