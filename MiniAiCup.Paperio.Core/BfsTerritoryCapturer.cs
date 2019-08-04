using System.Collections.Generic;
using System.Linq;

namespace MiniAiCup.Paperio.Core
{
	public class BfsTerritoryCapturer : ITerritoryCapturer
	{
		private readonly Size _mapSize;

		public BfsTerritoryCapturer(Size mapSize)
		{
			_mapSize = mapSize;
		}

		public PointsSet Capture(PointsSet territory, Path tail)
		{
			if (tail.Length <= 1)
			{
				return PointsSet.Empty;
			}

			if (!territory.Contains(tail.Last()))
			{
				return PointsSet.Empty;
			}

			var usedPoints = territory.UnionWith(tail);
			var visited = new bool[_mapSize.Width, _mapSize.Height];
			var startBoundaryPoints = GetBoundary(_mapSize).Where(p => !usedPoints.Contains(p));
			var outsidePoints = new List<Point>(startBoundaryPoints);
			foreach (var outsidePoint in outsidePoints)
			{
				visited[outsidePoint.X, outsidePoint.Y] = true;
			}

			var queue = new Queue<Point>(outsidePoints);
			while (queue.Count > 0)
			{
				var point = queue.Dequeue();
				foreach (var neighbor in point.GetNeighbors())
				{
					if (!_mapSize.ContainsPoint(neighbor))
					{
						continue;
					}

					if (visited[neighbor.X, neighbor.Y])
					{
						continue;
					}

					visited[neighbor.X, neighbor.Y] = true;
					if (usedPoints.Contains(neighbor))
					{
						continue;
					}
					outsidePoints.Add(neighbor);
					queue.Enqueue(neighbor);
				}
			}

			var allPoints = _mapSize.GetAllLogicPoints();
			var newTerritoryPoints = allPoints.ExceptWith(outsidePoints);
			return newTerritoryPoints.ExceptWith(territory);
		}

		private static IEnumerable<Point> GetBoundary(Size size)
		{
			for (int i = 0; i < size.Width; i++)
			{
				yield return new Point(i, 0);
				yield return new Point(i, size.Height - 1);
			}

			for (int i = 1; i < size.Height - 1; i++)
			{
				yield return new Point(0, i);
				yield return new Point(size.Width - 1, i);
			}
		}
	}
}
