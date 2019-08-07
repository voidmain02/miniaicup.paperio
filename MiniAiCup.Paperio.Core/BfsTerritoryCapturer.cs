using System.Collections.Generic;
using System.Linq;

namespace MiniAiCup.Paperio.Core
{
	public class BfsTerritoryCapturer : ITerritoryCapturer
	{
		private readonly List<Point> _mapBoundaryPoints;

		private readonly bool[,] _visited;

		public BfsTerritoryCapturer()
		{
			_mapBoundaryPoints = GetBoundary(Game.Params.MapLogicSize).ToList();
			_visited = new bool[Game.Params.MapLogicSize.Width, Game.Params.MapLogicSize.Height];
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

			ResetVisited();

			var usedPoints = territory.UnionWith(tail);
			var startBoundaryPoints = _mapBoundaryPoints.Where(p => !usedPoints.Contains(p));
			var outsidePoints = new List<Point>(startBoundaryPoints);
			foreach (var outsidePoint in outsidePoints)
			{
				_visited[outsidePoint.X, outsidePoint.Y] = true;
			}

			var queue = new Queue<Point>(outsidePoints);
			while (queue.Count > 0)
			{
				var point = queue.Dequeue();
				foreach (var neighbor in point.GetNeighbors())
				{
					if (!Game.Params.MapLogicSize.ContainsPoint(neighbor))
					{
						continue;
					}

					if (_visited[neighbor.X, neighbor.Y])
					{
						continue;
					}

					_visited[neighbor.X, neighbor.Y] = true;
					if (usedPoints.Contains(neighbor))
					{
						continue;
					}
					outsidePoints.Add(neighbor);
					queue.Enqueue(neighbor);
				}
			}

			return Game.AllMapPoints.ExceptWith(outsidePoints).ExceptWith(territory);
		}

		private void ResetVisited()
		{
			for (int y = 0; y < Game.Params.MapLogicSize.Height; y++)
			{
				for (int x = 0; x < Game.Params.MapLogicSize.Width; x++)
				{
					_visited[x, y] = false;
				}
			}
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
