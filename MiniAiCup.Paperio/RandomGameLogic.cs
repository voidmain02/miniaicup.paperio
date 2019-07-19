using System;

namespace MiniAiCup.Paperio
{
	public class RandomGameLogic : IGameLogic
	{
		private readonly GameParams _gameParams;

		public RandomGameLogic(GameParams gameParams)
		{
			_gameParams = gameParams;
		}

		public Command GetNextCommand(GameState state)
		{
			var random = new Random();
			int index = random.Next(0, 4);
			return (Command)index;
		}
	}
}
