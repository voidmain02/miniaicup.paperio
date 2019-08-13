using System.Collections.Generic;
using System.Linq;
using MiniAiCup.Paperio.Core.Debug;

namespace MiniAiCup.Paperio.Core
{
	public class BfsTerritoryCapturer
	{
		private readonly List<Point> _mapBoundaryPoints;

		private readonly bool[,] _visited;

		private readonly bool[,] _emptyVisited;

		private readonly Queue<Point> _queue;

		public BfsTerritoryCapturer()
		{
			_mapBoundaryPoints = GetBoundary(Game.Params.MapLogicSize).ToList();
			_visited = Game.GetNewMap<bool>();
			_emptyVisited = Game.GetNewMap<bool>();
			_queue = new Queue<Point>(Game.Params.MapLogicSize.Width * Game.Params.MapLogicSize.Height);
		}

		public PointsSet Capture(PointsSet territory, IEnumerable<Point> tail)
		{
#if DEBUG
			GameDebugData.Current.CaptureCount++;
#endif

			ResetVisited();

			var usedPoints = territory.UnionWith(tail);
			var startBoundaryPoints = _mapBoundaryPoints.Where(p => !usedPoints.Contains(p));
			var outsidePoints = new List<Point>(startBoundaryPoints);
			foreach (var outsidePoint in outsidePoints)
			{
				_visited[outsidePoint.X, outsidePoint.Y] = true;
				_queue.Enqueue(outsidePoint);
			}

			while (_queue.Count > 0)
			{
				var point = _queue.Dequeue();
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
					_queue.Enqueue(neighbor);
				}
			}

			return Game.AllMapPoints.ExceptWith(outsidePoints).ExceptWith(territory);
		}

		private void ResetVisited()
		{
			Utils.FastCopyArray(_emptyVisited, _visited, Game.Params.MapLogicSize.Width*Game.Params.MapLogicSize.Height);
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
