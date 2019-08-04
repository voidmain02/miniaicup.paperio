namespace MiniAiCup.Paperio.Core
{
	public interface ITerritoryCapturer
	{
		PointsSet Capture(PointsSet territory, Path tail);
	}
}
