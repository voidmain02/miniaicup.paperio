using System.Collections.Generic;
using System.Linq;

namespace MiniAiCup.Paperio.Core
{
	public class TerritoryCapturer
	{
		private readonly Size _mapSize;
		private readonly PointsSet _territory;

		public TerritoryCapturer(Size mapSize, PointsSet territory)
		{
			_mapSize = mapSize;
			_territory = territory;
		}

		public PointsSet Capture(Path tail)
		{
			if (tail.Length <= 1)
			{
				return PointsSet.Empty;
			}

			if (!_territory.Contains(tail.Last()))
			{
				return PointsSet.Empty;
			}

			var capturedPoints = new List<Point>();
			capturedPoints.AddRange(tail.Take(tail.Length - 1));
			capturedPoints.AddRange(CaptureVoidsInTail(tail));
			var voidsBoundaries = GetVoidsBetweenTailAndTerritory(tail);
			foreach (var voidBoundary in voidsBoundaries)
			{
				capturedPoints.AddRange(CaptureBoundary(voidBoundary));
			}

			return new PointsSet(capturedPoints);
		}

		private IEnumerable<Point> CaptureBoundary(ICollection<Point> boundary)
		{
			var polygonXArray = boundary.Select(p => p.X).ToList();
			var polygonYArray = boundary.Select(p => p.Y).ToList();

			int xMax = polygonXArray.Max();
			int yMax = polygonYArray.Max();
			int xMin = polygonXArray.Min();
			int yMin = polygonYArray.Min();

			var captured = new List<Point>();
			for (int x = xMax; x > xMin; x--)
			{
				for (int y = yMax; y > yMin; y--)
				{
					var point = new Point(x, y);
					if (!_territory.Contains(point) && CheckPointInPolygon(point, polygonXArray, polygonYArray))
					{
						captured.Add(point);
					}
				}
			}

			return captured;
		}

		private static bool CheckPointInPolygon(Point point, IReadOnlyList<int> xp, IReadOnlyList<int> yp)
		{
			bool c = false;
			for (int i = 0; i < xp.Count; i++)
			{
				int j = (i + xp.Count - 1)%xp.Count;
				if ((yp[i] <= point.Y && point.Y < yp[j] || yp[j] <= point.Y && point.Y < yp[i]) && point.X > (xp[j] - xp[i])*(point.Y - yp[i])/(yp[j] - yp[i]) + xp[i])
				{
					c = !c;
				}
			}

			return c;
		}

		private IEnumerable<Point> CaptureVoidsInTail(Path tail)
		{
			var captured = new List<Point>();
			for (int index = 0; index < tail.Length; index++)
			{
				var currentTailPoint = tail[index];
				foreach (var neighbor in currentTailPoint.GetNeighbors())
				{
					if (!tail.AsPointsSet().Contains(neighbor))
					{
						continue;
					}

					int endIndex = tail.IndexOf(neighbor);
					var path = tail.Skip(index).Take(endIndex - index + 1).ToList();
					if (path.Count >= 8)
					{
						captured.AddRange(CaptureBoundary(path));
					}
				}
			}

			return captured;
		}

		private List<List<Point>> GetVoidsBetweenTailAndTerritory(Path tail)
		{
			var boundary = _territory.GetBoundary();
			var voids = new List<List<Point>>();

			for(int i_lp1 = 0; i_lp1 < tail.Length; i_lp1++)
			{
				var lp1 = tail[i_lp1];
				foreach (var point in lp1.GetEightNeighbors())
				{
					if (!boundary.Contains(point))
					{
						continue;
					}

					Point? prev = null;
					for (int i_lp2 = 0; i_lp2 < i_lp1 + 1; i_lp2++)
					{
						var lp2 = tail[i_lp2];
						var startPoint = GetNearestBoundaryPoint(lp2, boundary);
						if (startPoint != null)
						{
							if (prev.HasValue && (prev.Value.IsNeighbor(startPoint.Value) || prev == startPoint))
							{
								prev = startPoint;
								continue;
							}

							var path = GetPath(startPoint.Value, point, boundary);
							if (path == null || path.Length == 0)
							{
								continue;
							}

							var voidPoints = tail.Skip(i_lp2 - 1).Take(i_lp1 - i_lp2 + 1).ToList();
							voidPoints.AddRange(path);
							voids.Add(voidPoints);
						}

						prev = startPoint;
					}
				}
			}

			return voids;
		}

		private Path GetPath(Point startPoint, Point endPoint, PointsSet boundary)
		{
			var allPoints = new List<Point>(_mapSize.Width * _mapSize.Height);
			for (int x = 0; x < _mapSize.Width; x++)
			{
				for (int y = 0; y < _mapSize.Height; y++)
				{
					allPoints.Add(new Point(x, y));
				}
			}
			var obstacles = new PointsSet(allPoints).ExceptWith(boundary);
			return PathFinder.GetShortestPath(startPoint, new PointsSet(new[] { endPoint }), obstacles, _mapSize);
		}

		private static Point? GetNearestBoundaryPoint(Point point, PointsSet boundary)
		{
			foreach (var neighbor in point.GetEightNeighbors())
			{
				if (boundary.Contains(neighbor))
				{
					return neighbor;
				}
			}

			return null;
		}
	}
}
