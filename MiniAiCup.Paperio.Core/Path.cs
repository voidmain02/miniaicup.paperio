using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace MiniAiCup.Paperio.Core
{
	public class Path : IEnumerable<Point>
	{
		public static Path Empty { get; } = new Path(Enumerable.Empty<Point>());

		private readonly List<Point> _list;

		public HashSet<Point> HashSet { get; }

		public int Length => _list.Count;

		public Point this[int index] => _list[index];

		public Path(IEnumerable<Point> points)
		{
			_list = new List<Point>(points);
			HashSet = new HashSet<Point>(_list);

			if (_list.Count != HashSet.Count)
			{
				throw new ArgumentException();
			}
		}

		public IEnumerator<Point> GetEnumerator()
		{
			return _list.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}
