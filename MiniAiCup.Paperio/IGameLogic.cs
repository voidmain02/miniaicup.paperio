namespace MiniAiCup.Paperio
{
	public interface IGameLogic
	{
		Command GetNextCommand(GameState state);
	}
}
