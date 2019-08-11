using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;
using MiniAiCup.Paperio.Core;

namespace MiniAiCup.Paperio.Benchmark
{
	[ClrJob, MonoJob]
	[RankColumn]
	public class AppendHashSetBenchmark
	{
		private const int Value = 42;

		private readonly HashSet<Point> _hashSet;

		private readonly Point[] _points;

		public AppendHashSetBenchmark()
		{
			var territoryPoints = new List<Point>();
			for (int y = 4; y < 25; y++)
			{
				for (int x = 3; x < 30; x++)
				{
					territoryPoints.Add(new Point(x, y));
				}
			}
			_hashSet = new HashSet<Point>(territoryPoints);

			_points = new[] {
				new Point(5, 25),
				new Point(5, 26),
				new Point(5, 27),
				new Point(5, 28),
				new Point(5, 29),
				new Point(5, 30),
				new Point(6, 30),
				new Point(7, 30),
				new Point(7, 29),
				new Point(7, 28),
				new Point(6, 28),
				new Point(6, 27),
				new Point(6, 26),
				new Point(7, 26),
				new Point(8, 26),
				new Point(8, 25)
			};
		}

		[Benchmark(Baseline = true)]
		public HashSet<Point> HashSetUnion()
		{
			var newHashSet = new HashSet<Point>(_hashSet);
			newHashSet.UnionWith(_points);
			return newHashSet;
		}

		[Benchmark]
		public HashSet<Point> EnumerableConcat()
		{
			return new HashSet<Point>(_hashSet.Concat(_points));
		}

		[Benchmark]
		public HashSet<Point> HashsetAdd()
		{
			var newHashSet = new HashSet<Point>(_hashSet);
			foreach (var point in _points)
			{
				newHashSet.Add(point);
			}
			return newHashSet;
		}
	}
}
