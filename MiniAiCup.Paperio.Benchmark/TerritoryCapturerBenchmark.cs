using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;
using MiniAiCup.Paperio.Core;

namespace MiniAiCup.Paperio.Benchmark
{
	[ClrJob, MonoJob]
	[RankColumn]
	public class TerritoryCapturerBenchmark
	{
		private BfsTerritoryCapturer1 _bfs;
		private BfsTerritoryCapturer2 _bfs2;
		private BfsTerritoryCapturer3 _bfs3;
		private BfsTerritoryCapturer4 _bfs4;
		private BfsTerritoryCapturer5 _bfs5;
		private BfsTerritoryCapturer6 _bfs6;
		private BfsTerritoryCapturer7 _bfs7;
		private BfsTerritoryCapturer8 _bfs8;
		private BfsTerritoryCapturer9 _bfs9;
		private BfsTerritoryCapturer10 _bfs10;
		private BfsTerritoryCapturer11 _bfs11;
		private BfsTerritoryCapturer12 _bfs12;
		private BfsTerritoryCapturer13 _bfs13;

		private PointsSet _territory;
		private Path _tail;

		[GlobalSetup]
		public void Setup()
		{
			Game.Initialize(new GameParams {
				MapLogicSize = new Size(31, 31),
				CellSize = 30,
				Speed = 5
			});

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

			_bfs = new BfsTerritoryCapturer1();
			_bfs2 = new BfsTerritoryCapturer2();
			_bfs3 = new BfsTerritoryCapturer3();
			_bfs4 = new BfsTerritoryCapturer4();
			_bfs5 = new BfsTerritoryCapturer5();
			_bfs6 = new BfsTerritoryCapturer6();
			_bfs7 = new BfsTerritoryCapturer7();
			_bfs8 = new BfsTerritoryCapturer8();
			_bfs9 = new BfsTerritoryCapturer9();
			_bfs10 = new BfsTerritoryCapturer10();
			_bfs11 = new BfsTerritoryCapturer11();
			_bfs12 = new BfsTerritoryCapturer12();
			_bfs13 = new BfsTerritoryCapturer13();
		}

		[Benchmark(Baseline = true)]
		public PointsSet Bfs() => _bfs.Capture(_territory, _tail);

		[Benchmark]
		public PointsSet StackAlloc() => _bfs2.Capture(_territory, _tail);

		[Benchmark]
		public PointsSet StateInsteadBool() => _bfs3.Capture(_territory, _tail);

		[Benchmark]
		public PointsSet ForInsteadForeachAllPoints() => _bfs4.Capture(_territory, _tail);

		[Benchmark]
		public PointsSet TerritoryAndTailStates() => _bfs5.Capture(_territory, _tail);

		[Benchmark]
		public PointsSet FastQueue() => _bfs6.Capture(_territory, _tail);

		[Benchmark]
		public PointsSet Box() => _bfs7.Capture(_territory, _tail);

		[Benchmark]
		public PointsSet ArrayResult() => _bfs8.Capture(_territory, _tail);

		[Benchmark]
		public PointsSet SmallQueue() => _bfs9.Capture(_territory, _tail);

		[Benchmark]
		public PointsSet WithoutGetBoundary() => _bfs10.Capture(_territory, _tail);
		
		[Benchmark]
		public PointsSet IntGetBoundary() => _bfs11.Capture(_territory, _tail);

		[Benchmark]
		public PointsSet Best() => _bfs12.Capture(_territory, _tail);

		[Benchmark]
		public PointsSet BestEnh() => _bfs13.Capture(_territory, _tail);
	}

	public class BfsTerritoryCapturer1
	{
		private readonly List<Point> _mapBoundaryPoints;

		private readonly bool[,] _visited;

		private readonly bool[,] _emptyVisited;

		private readonly Queue<Point> _queue;

		public BfsTerritoryCapturer1()
		{
			_mapBoundaryPoints = GetBoundary(Game.Params.MapLogicSize).ToList();
			_visited = Game.GetNewMap<bool>();
			_emptyVisited = Game.GetNewMap<bool>();
			_queue = new Queue<Point>(Game.Params.MapLogicSize.Width * Game.Params.MapLogicSize.Height);
		}

		public PointsSet Capture(PointsSet territory, IEnumerable<Point> tail)
		{
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

	public class BfsTerritoryCapturer2
	{
		private readonly List<Point> _mapBoundaryPoints;

		private readonly Queue<Point> _queue;

		public BfsTerritoryCapturer2()
		{
			_mapBoundaryPoints = GetBoundary(Game.Params.MapLogicSize).ToList();
			_queue = new Queue<Point>(Game.Params.MapLogicSize.Width * Game.Params.MapLogicSize.Height);
		}

		public unsafe PointsSet Capture(PointsSet territory, IEnumerable<Point> tail)
		{
			var visited = stackalloc bool[Game.Params.MapLogicSize.Width*Game.Params.MapLogicSize.Height];

			var usedPoints = territory.UnionWith(tail);
			var startBoundaryPoints = _mapBoundaryPoints.Where(p => !usedPoints.Contains(p));
			var outsidePoints = new List<Point>(startBoundaryPoints);
			foreach (var outsidePoint in outsidePoints)
			{
				visited[outsidePoint.X + outsidePoint.Y*Game.Params.MapLogicSize.Width] = true;
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

					if (visited[neighbor.X + neighbor.Y*Game.Params.MapLogicSize.Width])
					{
						continue;
					}

					visited[neighbor.X + neighbor.Y*Game.Params.MapLogicSize.Width] = true;
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

	public class BfsTerritoryCapturer3
	{
		private readonly List<Point> _mapBoundaryPoints;

		private readonly Queue<Point> _queue;

		public BfsTerritoryCapturer3()
		{
			_mapBoundaryPoints = GetBoundary(Game.Params.MapLogicSize).ToList();
			_queue = new Queue<Point>(Game.Params.MapLogicSize.Width * Game.Params.MapLogicSize.Height);
		}

		public unsafe PointsSet Capture(PointsSet territory, IEnumerable<Point> tail)
		{
			var visited = stackalloc byte[Game.Params.MapLogicSize.Width*Game.Params.MapLogicSize.Height];

			const byte empty = 0;
			const byte inside = 1;
			const byte outside = 2;

			var usedPoints = territory.UnionWith(tail);
			int outsideCount = 0;
			foreach (var point in _mapBoundaryPoints)
			{
				if (usedPoints.Contains(point))
				{
					visited[point.X + point.Y*Game.Params.MapLogicSize.Width] = inside;
				}
				else
				{
					visited[point.X + point.Y*Game.Params.MapLogicSize.Width] = outside;
					outsideCount++;
					_queue.Enqueue(point);
				}
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

					if (visited[neighbor.X + neighbor.Y*Game.Params.MapLogicSize.Width] != empty)
					{
						continue;
					}

					if (usedPoints.Contains(neighbor))
					{
						visited[neighbor.X + neighbor.Y*Game.Params.MapLogicSize.Width] = inside;
						continue;
					}
					visited[neighbor.X + neighbor.Y*Game.Params.MapLogicSize.Width] = outside;
					outsideCount++;

					_queue.Enqueue(neighbor);
				}
			}

			var result = new List<Point>(Game.Params.MapLogicSize.Width*Game.Params.MapLogicSize.Height - outsideCount  - territory.Count);

			foreach (var point in Game.AllMapPoints)
			{
				if (visited[point.X + point.Y*Game.Params.MapLogicSize.Width] != outside && !territory.Contains(point))
				{
					result.Add(point);
				}
			}

			return new PointsSet(result);
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

	public class BfsTerritoryCapturer4
	{
		private readonly List<Point> _mapBoundaryPoints;

		private readonly Queue<Point> _queue;

		public BfsTerritoryCapturer4()
		{
			_mapBoundaryPoints = GetBoundary(Game.Params.MapLogicSize).ToList();
			_queue = new Queue<Point>(Game.Params.MapLogicSize.Width * Game.Params.MapLogicSize.Height);
		}

		public unsafe PointsSet Capture(PointsSet territory, IEnumerable<Point> tail)
		{
			var visited = stackalloc byte[Game.Params.MapLogicSize.Width*Game.Params.MapLogicSize.Height];

			const byte empty = 0;
			const byte inside = 1;
			const byte outside = 2;

			var usedPoints = territory.UnionWith(tail);
			int outsideCount = 0;
			foreach (var point in _mapBoundaryPoints)
			{
				if (usedPoints.Contains(point))
				{
					visited[point.X + point.Y*Game.Params.MapLogicSize.Width] = inside;
				}
				else
				{
					visited[point.X + point.Y*Game.Params.MapLogicSize.Width] = outside;
					outsideCount++;
					_queue.Enqueue(point);
				}
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

					if (visited[neighbor.X + neighbor.Y*Game.Params.MapLogicSize.Width] != empty)
					{
						continue;
					}

					if (usedPoints.Contains(neighbor))
					{
						visited[neighbor.X + neighbor.Y*Game.Params.MapLogicSize.Width] = inside;
						continue;
					}
					visited[neighbor.X + neighbor.Y*Game.Params.MapLogicSize.Width] = outside;
					outsideCount++;

					_queue.Enqueue(neighbor);
				}
			}

			var result = new List<Point>(Game.Params.MapLogicSize.Width*Game.Params.MapLogicSize.Height - outsideCount  - territory.Count);

			for (int y = 0; y < Game.Params.MapLogicSize.Height; y++)
			{
				for (int x = 0; x < Game.Params.MapLogicSize.Width; x++)
				{
					if (visited[x + y*Game.Params.MapLogicSize.Width] == outside)
					{
						continue;
					}
					var point = new Point(x, y);
					if (!territory.Contains(point))
					{
						result.Add(point);
					}
				}
			}

			return new PointsSet(result);
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

	public class BfsTerritoryCapturer5
	{
		private readonly List<Point> _mapBoundaryPoints;

		private readonly Queue<Point> _queue;

		public BfsTerritoryCapturer5()
		{
			_mapBoundaryPoints = GetBoundary(Game.Params.MapLogicSize).ToList();
			_queue = new Queue<Point>(Game.Params.MapLogicSize.Width * Game.Params.MapLogicSize.Height);
		}

		public unsafe PointsSet Capture(PointsSet territory, IEnumerable<Point> tail)
		{
			var visited = stackalloc byte[Game.Params.MapLogicSize.Width*Game.Params.MapLogicSize.Height];

			const byte emptyCell = 0;
			const byte territoryCell = 1;
			const byte tailCell = 2;
			const byte outsideCell = 3;

			foreach (var point in tail)
			{
				visited[point.X + point.Y*Game.Params.MapLogicSize.Width] = tailCell;
			}
			foreach (var point in territory)
			{
				visited[point.X + point.Y*Game.Params.MapLogicSize.Width] = territoryCell;
			}

			int outsideCount = 0;
			foreach (var point in _mapBoundaryPoints)
			{
				if (visited[point.X + point.Y*Game.Params.MapLogicSize.Width] == emptyCell)
				{
					visited[point.X + point.Y*Game.Params.MapLogicSize.Width] = outsideCell;
					outsideCount++;
					_queue.Enqueue(point);
				}
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

					if (visited[neighbor.X + neighbor.Y*Game.Params.MapLogicSize.Width] != emptyCell)
					{
						continue;
					}

					visited[neighbor.X + neighbor.Y*Game.Params.MapLogicSize.Width] = outsideCell;
					outsideCount++;
					_queue.Enqueue(neighbor);
				}
			}

			var result = new List<Point>(Game.Params.MapLogicSize.Width*Game.Params.MapLogicSize.Height - outsideCount - territory.Count);

			for (int y = 0; y < Game.Params.MapLogicSize.Height; y++)
			{
				for (int x = 0; x < Game.Params.MapLogicSize.Width; x++)
				{
					if (visited[x + y*Game.Params.MapLogicSize.Width] == emptyCell ||
						visited[x + y*Game.Params.MapLogicSize.Width] == tailCell)
					{
						result.Add(new Point(x, y));
					}
				}
			}

			return new PointsSet(result);
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

	public class BfsTerritoryCapturer6
	{
		private readonly List<Point> _mapBoundaryPoints;

		public BfsTerritoryCapturer6()
		{
			_mapBoundaryPoints = GetBoundary(Game.Params.MapLogicSize).ToList();
		}

		public unsafe PointsSet Capture(PointsSet territory, IEnumerable<Point> tail)
		{
			var visited = stackalloc byte[Game.Params.MapLogicSize.Width*Game.Params.MapLogicSize.Height];
			var queue = stackalloc int[Game.Params.MapLogicSize.Width*Game.Params.MapLogicSize.Height];
			int queueHead = 0;
			int queueTail = 0;

			const byte emptyCell = 0;
			const byte territoryCell = 1;
			const byte tailCell = 2;
			const byte outsideCell = 3;

			foreach (var point in tail)
			{
				visited[point.X + point.Y*Game.Params.MapLogicSize.Width] = tailCell;
			}
			foreach (var point in territory)
			{
				visited[point.X + point.Y*Game.Params.MapLogicSize.Width] = territoryCell;
			}

			int outsideCount = 0;
			foreach (var point in _mapBoundaryPoints)
			{
				if (visited[point.X + point.Y*Game.Params.MapLogicSize.Width] == emptyCell)
				{
					visited[point.X + point.Y*Game.Params.MapLogicSize.Width] = outsideCell;
					outsideCount++;
					queue[queueHead++] = point.X + point.Y*Game.Params.MapLogicSize.Width;
				}
			}

			while (queueTail != queueHead)
			{
				int pointInt = queue[queueTail++];
				var point = new Point(pointInt%Game.Params.MapLogicSize.Width, pointInt/Game.Params.MapLogicSize.Width);
				foreach (var neighbor in point.GetNeighbors())
				{
					if (!Game.Params.MapLogicSize.ContainsPoint(neighbor))
					{
						continue;
					}

					if (visited[neighbor.X + neighbor.Y*Game.Params.MapLogicSize.Width] != emptyCell)
					{
						continue;
					}

					visited[neighbor.X + neighbor.Y*Game.Params.MapLogicSize.Width] = outsideCell;
					outsideCount++;
					queue[queueHead++] = neighbor.X + neighbor.Y*Game.Params.MapLogicSize.Width;
				}
			}

			var result = new List<Point>(Game.Params.MapLogicSize.Width*Game.Params.MapLogicSize.Height - outsideCount - territory.Count);

			for (int y = 0; y < Game.Params.MapLogicSize.Height; y++)
			{
				for (int x = 0; x < Game.Params.MapLogicSize.Width; x++)
				{
					if (visited[x + y*Game.Params.MapLogicSize.Width] == emptyCell ||
						visited[x + y*Game.Params.MapLogicSize.Width] == tailCell)
					{
						result.Add(new Point(x, y));
					}
				}
			}

			return new PointsSet(result);
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

	public class BfsTerritoryCapturer7
	{
		public unsafe PointsSet Capture(PointsSet territory, IEnumerable<Point> tail)
		{
			var visited = stackalloc byte[Game.Params.MapLogicSize.Width*Game.Params.MapLogicSize.Height];
			var queue = stackalloc int[Game.Params.MapLogicSize.Width*Game.Params.MapLogicSize.Height];
			int queueHead = 0;
			int queueTail = 0;

			const byte emptyCell = 0;
			const byte territoryCell = 1;
			const byte tailCell = 2;
			const byte outsideCell = 3;

			int minX = Game.Params.MapLogicSize.Width - 1;
			int minY = Game.Params.MapLogicSize.Height - 1;
			int maxX = 0;
			int maxY = 0;

			foreach (var point in tail)
			{
				if (point.X < minX)
				{
					minX = point.X;
				}
				else if (point.X > maxX)
				{
					maxX = point.X;
				}
				if (point.Y < minY)
				{
					minY = point.Y;
				}
				else if (point.Y > maxY)
				{
					maxY = point.Y;
				}
				visited[point.X + point.Y*Game.Params.MapLogicSize.Width] = tailCell;
			}
			foreach (var point in territory)
			{
				if (point.X < minX)
				{
					minX = point.X;
				}
				else if (point.X > maxX)
				{
					maxX = point.X;
				}
				if (point.Y < minY)
				{
					minY = point.Y;
				}
				else if (point.Y > maxY)
				{
					maxY = point.Y;
				}
				visited[point.X + point.Y*Game.Params.MapLogicSize.Width] = territoryCell;
			}

			int outsideCount = 0;
			foreach (var point in GetBoundary(minX, minY, maxX, maxY))
			{
				if (visited[point.X + point.Y*Game.Params.MapLogicSize.Width] == emptyCell)
				{
					visited[point.X + point.Y*Game.Params.MapLogicSize.Width] = outsideCell;
					outsideCount++;
					queue[queueHead++] = point.X + point.Y*Game.Params.MapLogicSize.Width;
				}
			}

			while (queueTail != queueHead)
			{
				int pointInt = queue[queueTail++];
				var point = new Point(pointInt%Game.Params.MapLogicSize.Width, pointInt/Game.Params.MapLogicSize.Width);
				foreach (var neighbor in point.GetNeighbors())
				{
					if (neighbor.X < minX || neighbor.X > maxX || neighbor.Y < minY || neighbor.Y > maxY)
					{
						continue;
					}

					if (visited[neighbor.X + neighbor.Y*Game.Params.MapLogicSize.Width] != emptyCell)
					{
						continue;
					}

					visited[neighbor.X + neighbor.Y*Game.Params.MapLogicSize.Width] = outsideCell;
					outsideCount++;
					queue[queueHead++] = neighbor.X + neighbor.Y*Game.Params.MapLogicSize.Width;
				}
			}

			var result = new List<Point>((maxX - minX + 1)*(maxY - minY + 1) - outsideCount - territory.Count);

			for (int y = minY; y <= maxY; y++)
			{
				for (int x = minX; x <= maxX; x++)
				{
					if (visited[x + y*Game.Params.MapLogicSize.Width] == emptyCell ||
						visited[x + y*Game.Params.MapLogicSize.Width] == tailCell)
					{
						result.Add(new Point(x, y));
					}
				}
			}

			return new PointsSet(result);
		}

		private static IEnumerable<Point> GetBoundary(int minX, int minY, int maxX, int maxY)
		{
			for (int x = minX; x <= maxX; x++)
			{
				yield return new Point(x, minY);
				yield return new Point(x, maxY);
			}

			for (int y = minY + 1; y <= maxY - 1; y++)
			{
				yield return new Point(minX, y);
				yield return new Point(maxX, y);
			}
		}
	}

	public class BfsTerritoryCapturer8
	{
		public unsafe PointsSet Capture(PointsSet territory, IEnumerable<Point> tail)
		{
			var visited = stackalloc byte[Game.Params.MapLogicSize.Width*Game.Params.MapLogicSize.Height];
			var queue = stackalloc int[Game.Params.MapLogicSize.Width*Game.Params.MapLogicSize.Height];
			int queueHead = 0;
			int queueTail = 0;

			const byte emptyCell = 0;
			const byte territoryCell = 1;
			const byte tailCell = 2;
			const byte outsideCell = 3;

			int minX = Game.Params.MapLogicSize.Width - 1;
			int minY = Game.Params.MapLogicSize.Height - 1;
			int maxX = 0;
			int maxY = 0;

			foreach (var point in tail)
			{
				if (point.X < minX)
				{
					minX = point.X;
				}
				else if (point.X > maxX)
				{
					maxX = point.X;
				}
				if (point.Y < minY)
				{
					minY = point.Y;
				}
				else if (point.Y > maxY)
				{
					maxY = point.Y;
				}
				visited[point.X + point.Y*Game.Params.MapLogicSize.Width] = tailCell;
			}
			foreach (var point in territory)
			{
				if (point.X < minX)
				{
					minX = point.X;
				}
				else if (point.X > maxX)
				{
					maxX = point.X;
				}
				if (point.Y < minY)
				{
					minY = point.Y;
				}
				else if (point.Y > maxY)
				{
					maxY = point.Y;
				}
				visited[point.X + point.Y*Game.Params.MapLogicSize.Width] = territoryCell;
			}

			int outsideCount = 0;
			foreach (var point in GetBoundary(minX, minY, maxX, maxY))
			{
				if (visited[point.X + point.Y*Game.Params.MapLogicSize.Width] == emptyCell)
				{
					visited[point.X + point.Y*Game.Params.MapLogicSize.Width] = outsideCell;
					outsideCount++;
					queue[queueHead++] = point.X + point.Y*Game.Params.MapLogicSize.Width;
				}
			}

			while (queueTail != queueHead)
			{
				int pointInt = queue[queueTail++];
				var point = new Point(pointInt%Game.Params.MapLogicSize.Width, pointInt/Game.Params.MapLogicSize.Width);
				foreach (var neighbor in point.GetNeighbors())
				{
					if (neighbor.X < minX || neighbor.X > maxX || neighbor.Y < minY || neighbor.Y > maxY)
					{
						continue;
					}

					if (visited[neighbor.X + neighbor.Y*Game.Params.MapLogicSize.Width] != emptyCell)
					{
						continue;
					}

					visited[neighbor.X + neighbor.Y*Game.Params.MapLogicSize.Width] = outsideCell;
					outsideCount++;
					queue[queueHead++] = neighbor.X + neighbor.Y*Game.Params.MapLogicSize.Width;
				}
			}

			var result = new Point[(maxX - minX + 1)*(maxY - minY + 1) - outsideCount - territory.Count];
			int index = 0;
			for (int y = minY; y <= maxY; y++)
			{
				for (int x = minX; x <= maxX; x++)
				{
					if (visited[x + y*Game.Params.MapLogicSize.Width] == emptyCell ||
						visited[x + y*Game.Params.MapLogicSize.Width] == tailCell)
					{
						result[index++] = new Point(x, y);
					}
				}
			}

			return new PointsSet(result);
		}

		private static IEnumerable<Point> GetBoundary(int minX, int minY, int maxX, int maxY)
		{
			for (int x = minX; x <= maxX; x++)
			{
				yield return new Point(x, minY);
				yield return new Point(x, maxY);
			}

			for (int y = minY + 1; y <= maxY - 1; y++)
			{
				yield return new Point(minX, y);
				yield return new Point(maxX, y);
			}
		}
	}

	public class BfsTerritoryCapturer9
	{
		public unsafe PointsSet Capture(PointsSet territory, IEnumerable<Point> tail)
		{
			var visited = stackalloc byte[Game.Params.MapLogicSize.Width*Game.Params.MapLogicSize.Height];
			const byte emptyCell = 0;
			const byte territoryCell = 1;
			const byte tailCell = 2;
			const byte outsideCell = 3;

			int minX = Game.Params.MapLogicSize.Width - 1;
			int minY = Game.Params.MapLogicSize.Height - 1;
			int maxX = 0;
			int maxY = 0;

			foreach (var point in tail)
			{
				if (point.X < minX)
				{
					minX = point.X;
				}
				else if (point.X > maxX)
				{
					maxX = point.X;
				}
				if (point.Y < minY)
				{
					minY = point.Y;
				}
				else if (point.Y > maxY)
				{
					maxY = point.Y;
				}
				visited[point.X + point.Y*Game.Params.MapLogicSize.Width] = tailCell;
			}
			foreach (var point in territory)
			{
				if (point.X < minX)
				{
					minX = point.X;
				}
				else if (point.X > maxX)
				{
					maxX = point.X;
				}
				if (point.Y < minY)
				{
					minY = point.Y;
				}
				else if (point.Y > maxY)
				{
					maxY = point.Y;
				}
				visited[point.X + point.Y*Game.Params.MapLogicSize.Width] = territoryCell;
			}

			var queue = stackalloc int[(maxX - minX + 1)*(maxY - minY + 1)];
			int queueHead = 0;
			int queueTail = 0;

			int outsideCount = 0;
			foreach (var point in GetBoundary(minX, minY, maxX, maxY))
			{
				if (visited[point.X + point.Y*Game.Params.MapLogicSize.Width] == emptyCell)
				{
					visited[point.X + point.Y*Game.Params.MapLogicSize.Width] = outsideCell;
					outsideCount++;
					queue[queueHead++] = point.X + point.Y*Game.Params.MapLogicSize.Width;
				}
			}

			while (queueTail != queueHead)
			{
				int pointInt = queue[queueTail++];
				var point = new Point(pointInt%Game.Params.MapLogicSize.Width, pointInt/Game.Params.MapLogicSize.Width);
				foreach (var neighbor in point.GetNeighbors())
				{
					if (neighbor.X < minX || neighbor.X > maxX || neighbor.Y < minY || neighbor.Y > maxY)
					{
						continue;
					}

					if (visited[neighbor.X + neighbor.Y*Game.Params.MapLogicSize.Width] != emptyCell)
					{
						continue;
					}

					visited[neighbor.X + neighbor.Y*Game.Params.MapLogicSize.Width] = outsideCell;
					outsideCount++;
					queue[queueHead++] = neighbor.X + neighbor.Y*Game.Params.MapLogicSize.Width;
				}
			}

			var result = new Point[(maxX - minX + 1)*(maxY - minY + 1) - outsideCount - territory.Count];
			int index = 0;
			for (int y = minY; y <= maxY; y++)
			{
				for (int x = minX; x <= maxX; x++)
				{
					if (visited[x + y*Game.Params.MapLogicSize.Width] == emptyCell ||
						visited[x + y*Game.Params.MapLogicSize.Width] == tailCell)
					{
						result[index++] = new Point(x, y);
					}
				}
			}

			return new PointsSet(result);
		}

		private static IEnumerable<Point> GetBoundary(int minX, int minY, int maxX, int maxY)
		{
			for (int x = minX; x <= maxX; x++)
			{
				yield return new Point(x, minY);
				yield return new Point(x, maxY);
			}

			for (int y = minY + 1; y <= maxY - 1; y++)
			{
				yield return new Point(minX, y);
				yield return new Point(maxX, y);
			}
		}
	}

	public class BfsTerritoryCapturer10
	{
		public unsafe PointsSet Capture(PointsSet territory, IEnumerable<Point> tail)
		{
			var visited = stackalloc byte[Game.Params.MapLogicSize.Width*Game.Params.MapLogicSize.Height];
			const byte emptyCell = 0;
			const byte territoryCell = 1;
			const byte tailCell = 2;
			const byte outsideCell = 3;

			int minX = Game.Params.MapLogicSize.Width - 1;
			int minY = Game.Params.MapLogicSize.Height - 1;
			int maxX = 0;
			int maxY = 0;

			foreach (var point in tail)
			{
				if (point.X < minX)
				{
					minX = point.X;
				}
				else if (point.X > maxX)
				{
					maxX = point.X;
				}
				if (point.Y < minY)
				{
					minY = point.Y;
				}
				else if (point.Y > maxY)
				{
					maxY = point.Y;
				}
				visited[point.X + point.Y*Game.Params.MapLogicSize.Width] = tailCell;
			}
			foreach (var point in territory)
			{
				if (point.X < minX)
				{
					minX = point.X;
				}
				else if (point.X > maxX)
				{
					maxX = point.X;
				}
				if (point.Y < minY)
				{
					minY = point.Y;
				}
				else if (point.Y > maxY)
				{
					maxY = point.Y;
				}
				visited[point.X + point.Y*Game.Params.MapLogicSize.Width] = territoryCell;
			}

			var queue = stackalloc int[(maxX - minX + 1)*(maxY - minY + 1)];
			int queueHead = 0;
			int queueTail = 0;

			int outsideCount = 0;
			for (int x = minX; x <= maxX; x++)
			{
				if (visited[x + minY*Game.Params.MapLogicSize.Width] == emptyCell)
				{
					visited[x + minY*Game.Params.MapLogicSize.Width] = outsideCell;
					outsideCount++;
					queue[queueHead++] = x + minY*Game.Params.MapLogicSize.Width;
				}
				if (visited[x + maxY*Game.Params.MapLogicSize.Width] == emptyCell)
				{
					visited[x + maxY*Game.Params.MapLogicSize.Width] = outsideCell;
					outsideCount++;
					queue[queueHead++] = x + maxY*Game.Params.MapLogicSize.Width;
				}
			}

			for (int y = minY + 1; y <= maxY - 1; y++)
			{
				if (visited[minX + y*Game.Params.MapLogicSize.Width] == emptyCell)
				{
					visited[minX + y*Game.Params.MapLogicSize.Width] = outsideCell;
					outsideCount++;
					queue[queueHead++] = minX + y*Game.Params.MapLogicSize.Width;
				}
				if (visited[maxX + y*Game.Params.MapLogicSize.Width] == emptyCell)
				{
					visited[maxX + y*Game.Params.MapLogicSize.Width] = outsideCell;
					outsideCount++;
					queue[queueHead++] = maxX + y*Game.Params.MapLogicSize.Width;
				}
			}

			while (queueTail != queueHead)
			{
				int pointInt = queue[queueTail++];
				var point = new Point(pointInt%Game.Params.MapLogicSize.Width, pointInt/Game.Params.MapLogicSize.Width);
				foreach (var neighbor in point.GetNeighbors())
				{
					if (neighbor.X < minX || neighbor.X > maxX || neighbor.Y < minY || neighbor.Y > maxY)
					{
						continue;
					}

					if (visited[neighbor.X + neighbor.Y*Game.Params.MapLogicSize.Width] != emptyCell)
					{
						continue;
					}

					visited[neighbor.X + neighbor.Y*Game.Params.MapLogicSize.Width] = outsideCell;
					outsideCount++;
					queue[queueHead++] = neighbor.X + neighbor.Y*Game.Params.MapLogicSize.Width;
				}
			}

			var result = new Point[(maxX - minX + 1)*(maxY - minY + 1) - outsideCount - territory.Count];
			int index = 0;
			for (int y = minY; y <= maxY; y++)
			{
				for (int x = minX; x <= maxX; x++)
				{
					if (visited[x + y*Game.Params.MapLogicSize.Width] == emptyCell ||
						visited[x + y*Game.Params.MapLogicSize.Width] == tailCell)
					{
						result[index++] = new Point(x, y);
					}
				}
			}

			return new PointsSet(result);
		}
	}

	public class BfsTerritoryCapturer11
	{
		public unsafe PointsSet Capture(PointsSet territory, IEnumerable<Point> tail)
		{
			var visited = stackalloc byte[Game.Params.MapLogicSize.Width*Game.Params.MapLogicSize.Height];
			const byte emptyCell = 0;
			const byte territoryCell = 1;
			const byte tailCell = 2;
			const byte outsideCell = 3;

			int minX = Game.Params.MapLogicSize.Width - 1;
			int minY = Game.Params.MapLogicSize.Height - 1;
			int maxX = 0;
			int maxY = 0;

			foreach (var point in tail)
			{
				if (point.X < minX)
				{
					minX = point.X;
				}
				else if (point.X > maxX)
				{
					maxX = point.X;
				}
				if (point.Y < minY)
				{
					minY = point.Y;
				}
				else if (point.Y > maxY)
				{
					maxY = point.Y;
				}
				visited[point.X + point.Y*Game.Params.MapLogicSize.Width] = tailCell;
			}
			foreach (var point in territory)
			{
				if (point.X < minX)
				{
					minX = point.X;
				}
				else if (point.X > maxX)
				{
					maxX = point.X;
				}
				if (point.Y < minY)
				{
					minY = point.Y;
				}
				else if (point.Y > maxY)
				{
					maxY = point.Y;
				}
				visited[point.X + point.Y*Game.Params.MapLogicSize.Width] = territoryCell;
			}

			var queue = stackalloc int[(maxX - minX + 1)*(maxY - minY + 1)];
			int queueHead = 0;
			int queueTail = 0;

			int outsideCount = 0;
			foreach (int coord in GetBoundary(minX, minY, maxX, maxY))
			{
				if (visited[coord] == emptyCell)
				{
					visited[coord] = outsideCell;
					outsideCount++;
					queue[queueHead++] = coord;
				}
			}

			while (queueTail != queueHead)
			{
				int pointInt = queue[queueTail++];
				var point = new Point(pointInt%Game.Params.MapLogicSize.Width, pointInt/Game.Params.MapLogicSize.Width);
				foreach (var neighbor in point.GetNeighbors())
				{
					if (neighbor.X < minX || neighbor.X > maxX || neighbor.Y < minY || neighbor.Y > maxY)
					{
						continue;
					}

					if (visited[neighbor.X + neighbor.Y*Game.Params.MapLogicSize.Width] != emptyCell)
					{
						continue;
					}

					visited[neighbor.X + neighbor.Y*Game.Params.MapLogicSize.Width] = outsideCell;
					outsideCount++;
					queue[queueHead++] = neighbor.X + neighbor.Y*Game.Params.MapLogicSize.Width;
				}
			}

			var result = new Point[(maxX - minX + 1)*(maxY - minY + 1) - outsideCount - territory.Count];
			int index = 0;
			for (int y = minY; y <= maxY; y++)
			{
				for (int x = minX; x <= maxX; x++)
				{
					if (visited[x + y*Game.Params.MapLogicSize.Width] == emptyCell ||
						visited[x + y*Game.Params.MapLogicSize.Width] == tailCell)
					{
						result[index++] = new Point(x, y);
					}
				}
			}

			return new PointsSet(result);
		}

		private static IEnumerable<int> GetBoundary(int minX, int minY, int maxX, int maxY)
		{
			for (int x = minX; x <= maxX; x++)
			{
				yield return x + minY*Game.Params.MapLogicSize.Width;
				yield return x + maxY*Game.Params.MapLogicSize.Width;
			}

			for (int y = minY + 1; y <= maxY - 1; y++)
			{
				yield return minX + y*Game.Params.MapLogicSize.Width;
				yield return maxX + y*Game.Params.MapLogicSize.Width;
			}
		}
	}

	public class BfsTerritoryCapturer12
	{
		public unsafe PointsSet Capture(PointsSet territory, IEnumerable<Point> tail)
		{
			var visited = stackalloc byte[Game.Params.MapLogicSize.Width*Game.Params.MapLogicSize.Height];
			const byte emptyCell = 0;
			const byte territoryCell = 1;
			const byte tailCell = 2;
			const byte outsideCell = 3;

			var queue = stackalloc int[Game.Params.MapLogicSize.Width*Game.Params.MapLogicSize.Height];
			int queueHead = 0;
			int queueTail = 0;

			int minX = Game.Params.MapLogicSize.Width - 1;
			int minY = Game.Params.MapLogicSize.Height - 1;
			int maxX = 0;
			int maxY = 0;

			foreach (var point in tail)
			{
				if (point.X < minX)
				{
					minX = point.X;
				}
				else if (point.X > maxX)
				{
					maxX = point.X;
				}
				if (point.Y < minY)
				{
					minY = point.Y;
				}
				else if (point.Y > maxY)
				{
					maxY = point.Y;
				}
				visited[point.X + point.Y*Game.Params.MapLogicSize.Width] = tailCell;
			}
			foreach (var point in territory)
			{
				if (point.X < minX)
				{
					minX = point.X;
				}
				else if (point.X > maxX)
				{
					maxX = point.X;
				}
				if (point.Y < minY)
				{
					minY = point.Y;
				}
				else if (point.Y > maxY)
				{
					maxY = point.Y;
				}
				visited[point.X + point.Y*Game.Params.MapLogicSize.Width] = territoryCell;
			}

			int outsideCount = 0;
			for (int x = minX; x <= maxX; x++)
			{
				if (visited[x + minY*Game.Params.MapLogicSize.Width] == emptyCell)
				{
					visited[x + minY*Game.Params.MapLogicSize.Width] = outsideCell;
					outsideCount++;
					queue[queueHead++] = x + minY*Game.Params.MapLogicSize.Width;
				}
				if (visited[x + maxY*Game.Params.MapLogicSize.Width] == emptyCell)
				{
					visited[x + maxY*Game.Params.MapLogicSize.Width] = outsideCell;
					outsideCount++;
					queue[queueHead++] = x + maxY*Game.Params.MapLogicSize.Width;
				}
			}

			for (int y = minY + 1; y <= maxY - 1; y++)
			{
				if (visited[minX + y*Game.Params.MapLogicSize.Width] == emptyCell)
				{
					visited[minX + y*Game.Params.MapLogicSize.Width] = outsideCell;
					outsideCount++;
					queue[queueHead++] = minX + y*Game.Params.MapLogicSize.Width;
				}
				if (visited[maxX + y*Game.Params.MapLogicSize.Width] == emptyCell)
				{
					visited[maxX + y*Game.Params.MapLogicSize.Width] = outsideCell;
					outsideCount++;
					queue[queueHead++] = maxX + y*Game.Params.MapLogicSize.Width;
				}
			}

			while (queueTail != queueHead)
			{
				int pointInt = queue[queueTail++];
				var point = new Point(pointInt%Game.Params.MapLogicSize.Width, pointInt/Game.Params.MapLogicSize.Width);
				foreach (var neighbor in point.GetNeighbors())
				{
					if (neighbor.X < minX || neighbor.X > maxX || neighbor.Y < minY || neighbor.Y > maxY)
					{
						continue;
					}

					if (visited[neighbor.X + neighbor.Y*Game.Params.MapLogicSize.Width] != emptyCell)
					{
						continue;
					}

					visited[neighbor.X + neighbor.Y*Game.Params.MapLogicSize.Width] = outsideCell;
					outsideCount++;
					queue[queueHead++] = neighbor.X + neighbor.Y*Game.Params.MapLogicSize.Width;
				}
			}

			var result = new Point[(maxX - minX + 1)*(maxY - minY + 1) - outsideCount - territory.Count];
			int index = 0;
			for (int y = minY; y <= maxY; y++)
			{
				for (int x = minX; x <= maxX; x++)
				{
					if (visited[x + y*Game.Params.MapLogicSize.Width] == emptyCell ||
						visited[x + y*Game.Params.MapLogicSize.Width] == tailCell)
					{
						result[index++] = new Point(x, y);
					}
				}
			}

			return new PointsSet(result);
		}
	}

	public class BfsTerritoryCapturer13
	{
		public unsafe PointsSet Capture(PointsSet territory, IEnumerable<Point> tail)
		{
			var visited = stackalloc byte[Game.Params.MapLogicSize.Width*Game.Params.MapLogicSize.Height];
			const byte emptyCell = 0;
			const byte territoryCell = 1;
			const byte tailCell = 2;
			const byte outsideCell = 3;

			int minX = Game.Params.MapLogicSize.Width - 1;
			int minY = Game.Params.MapLogicSize.Height - 1;
			int maxX = 0;
			int maxY = 0;

			foreach (var point in tail)
			{
				if (point.X < minX)
				{
					minX = point.X;
				}
				else if (point.X > maxX)
				{
					maxX = point.X;
				}
				if (point.Y < minY)
				{
					minY = point.Y;
				}
				else if (point.Y > maxY)
				{
					maxY = point.Y;
				}
				visited[point.X + point.Y*Game.Params.MapLogicSize.Width] = tailCell;
			}
			foreach (var point in territory)
			{
				if (point.X < minX)
				{
					minX = point.X;
				}
				else if (point.X > maxX)
				{
					maxX = point.X;
				}
				if (point.Y < minY)
				{
					minY = point.Y;
				}
				else if (point.Y > maxY)
				{
					maxY = point.Y;
				}
				visited[point.X + point.Y*Game.Params.MapLogicSize.Width] = territoryCell;
			}

			int totalBoxSize = (maxX - minX + 1)*(maxY - minY + 1);
			var queue = stackalloc int[totalBoxSize];
			int queueHead = 0;
			int queueTail = 0;

			int outsideCount = 0;
			for (int x = minX; x <= maxX; x++)
			{
				if (visited[x + minY*Game.Params.MapLogicSize.Width] == emptyCell)
				{
					visited[x + minY*Game.Params.MapLogicSize.Width] = outsideCell;
					outsideCount++;
					queue[queueHead++] = x + minY*Game.Params.MapLogicSize.Width;
				}
				if (visited[x + maxY*Game.Params.MapLogicSize.Width] == emptyCell)
				{
					visited[x + maxY*Game.Params.MapLogicSize.Width] = outsideCell;
					outsideCount++;
					queue[queueHead++] = x + maxY*Game.Params.MapLogicSize.Width;
				}
			}

			for (int y = minY + 1; y <= maxY - 1; y++)
			{
				if (visited[minX + y*Game.Params.MapLogicSize.Width] == emptyCell)
				{
					visited[minX + y*Game.Params.MapLogicSize.Width] = outsideCell;
					outsideCount++;
					queue[queueHead++] = minX + y*Game.Params.MapLogicSize.Width;
				}
				if (visited[maxX + y*Game.Params.MapLogicSize.Width] == emptyCell)
				{
					visited[maxX + y*Game.Params.MapLogicSize.Width] = outsideCell;
					outsideCount++;
					queue[queueHead++] = maxX + y*Game.Params.MapLogicSize.Width;
				}
			}

			while (queueTail != queueHead)
			{
				int pointInt = queue[queueTail++];
				var point = new Point(pointInt%Game.Params.MapLogicSize.Width, pointInt/Game.Params.MapLogicSize.Width);
				foreach (var neighbor in point.GetNeighbors())
				{
					if (neighbor.X < minX || neighbor.X > maxX || neighbor.Y < minY || neighbor.Y > maxY)
					{
						continue;
					}

					if (visited[neighbor.X + neighbor.Y*Game.Params.MapLogicSize.Width] != emptyCell)
					{
						continue;
					}

					visited[neighbor.X + neighbor.Y*Game.Params.MapLogicSize.Width] = outsideCell;
					outsideCount++;
					queue[queueHead++] = neighbor.X + neighbor.Y*Game.Params.MapLogicSize.Width;
				}
			}

			var result = new Point[totalBoxSize - outsideCount - territory.Count];
			int index = 0;
			for (int y = minY; y <= maxY; y++)
			{
				for (int x = minX; x <= maxX; x++)
				{
					if (visited[x + y*Game.Params.MapLogicSize.Width] == emptyCell ||
						visited[x + y*Game.Params.MapLogicSize.Width] == tailCell)
					{
						result[index++] = new Point(x, y);
					}
				}
			}

			return new PointsSet(result);
		}
	}
}
