namespace MiniAiCup.Paperio.Core
{
	public interface IGameLogic
	{
		Direction GetNextDirection(GameState state);
	}
}
