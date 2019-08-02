using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace MiniAiCup.Paperio.Core
{
	public class PointsSet : IEnumerable<Point>
	{
		public static PointsSet Empty { get; } = new PointsSet(Enumerable.Empty<Point>());

		private readonly HashSet<Point> _hashSet;

		public int Count => _hashSet.Count;

		public bool Contains(Point point) => _hashSet.Contains(point);

		private PointsSet(HashSet<Point> hashSet)
		{
			_hashSet = hashSet;
		}

		public PointsSet(IEnumerable<Point> points)
		{
			_hashSet = new HashSet<Point>(points);
		}

		public IEnumerator<Point> GetEnumerator()
		{
			return _hashSet.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public PointsSet UnionWith(IEnumerable<Point> points)
		{
			var hashSet = new HashSet<Point>(_hashSet);
			hashSet.UnionWith(points);
			return new PointsSet(hashSet);
		}

		public PointsSet ExceptWith(IEnumerable<Point> points)
		{
			var hashSet = new HashSet<Point>(_hashSet);
			hashSet.ExceptWith(points);
			return new PointsSet(hashSet);
		}

		public PointsSet IntersectWith(IEnumerable<Point> points)
		{
			var hashSet = new HashSet<Point>(_hashSet);
			hashSet.IntersectWith(points);
			return new PointsSet(hashSet);
		}
	}
}
