using System.Linq;

namespace MiniAiCup.Paperio.Core
{
	public static class PointsSetExtensions
	{
		public static PointsSet GetBoundary(this PointsSet pointsSet)
		{
			return new PointsSet(pointsSet.Where(p => p.GetEightNeighbors().Any(n => !pointsSet.Contains(n))));
		}
	}
}
