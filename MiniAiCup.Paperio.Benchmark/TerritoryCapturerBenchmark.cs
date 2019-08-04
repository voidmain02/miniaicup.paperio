using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using MiniAiCup.Paperio.Core;

namespace MiniAiCup.Paperio.Benchmark
{
	[CoreJob]
	[RPlotExporter]
	[RankColumn]
	public class TerritoryCapturerBenchmark
	{
		private ITerritoryCapturer _reference;
		private ITerritoryCapturer _bfs;

		private PointsSet _territory;
		private Path _tail;
		private Size _mapSize;
		
		[GlobalSetup]
		public void Setup()
		{
			_mapSize = new Size(31, 31);

			var territoryPoints = new List<Point>();
			for (int y = 4; y < 25; y++)
			{
				for (int x = 3; x < 30; x++)
				{
					territoryPoints.Add(new Point(x, y));
				}
			}
			_territory = new PointsSet(territoryPoints);

			_tail = new Path(new[] {
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
				new Point(8, 25),
				new Point(8, 24)
			});

			_reference = new ReferenceTerritoryCapturer(_mapSize);
			_bfs = new BfsTerritoryCapturer(_mapSize);
		}

		[Benchmark(Baseline = true)]
		public PointsSet Reference() => _reference.Capture(_territory, _tail);

		[Benchmark]
		public PointsSet Bfs() => _bfs.Capture(_territory, _tail);
	}
}
