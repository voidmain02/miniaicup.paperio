namespace MiniAiCup.Paperio.Core
{
	public interface IEnemyStrategy
	{
		Move GetMove(GameStateInternal state, PlayerInternal enemyPlayer);
	}
}
