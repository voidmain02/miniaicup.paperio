using System.Collections.Generic;
using MiniAiCup.Paperio.Core.Debug;

namespace MiniAiCup.Paperio.Core
{
	public class BfsTerritoryCapturer
	{
		public unsafe PointsSet Capture(PointsSet territory, IEnumerable<Point> tail)
		{
#if DEBUG
			GameDebugData.Current.CaptureCount++;
#endif

			var visited = stackalloc byte[GameParams.MapSize.Width*GameParams.MapSize.Height];
			const byte emptyCell = 0;
			const byte territoryCell = 1;
			const byte tailCell = 2;
			const byte outsideCell = 3;

			int minX = GameParams.MapSize.Width - 1;
			int minY = GameParams.MapSize.Height - 1;
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
				visited[point.X + point.Y*GameParams.MapSize.Width] = tailCell;
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
				visited[point.X + point.Y*GameParams.MapSize.Width] = territoryCell;
			}

			int totalBoxSize = (maxX - minX + 1)*(maxY - minY + 1);
			var queue = stackalloc int[totalBoxSize];
			int queueHead = 0;
			int queueTail = 0;

			int outsideCount = 0;
			for (int x = minX; x <= maxX; x++)
			{
				if (visited[x + minY*GameParams.MapSize.Width] == emptyCell)
				{
					visited[x + minY*GameParams.MapSize.Width] = outsideCell;
					outsideCount++;
					queue[queueHead++] = x + minY*GameParams.MapSize.Width;
				}
				if (visited[x + maxY*GameParams.MapSize.Width] == emptyCell)
				{
					visited[x + maxY*GameParams.MapSize.Width] = outsideCell;
					outsideCount++;
					queue[queueHead++] = x + maxY*GameParams.MapSize.Width;
				}
			}

			for (int y = minY + 1; y <= maxY - 1; y++)
			{
				if (visited[minX + y*GameParams.MapSize.Width] == emptyCell)
				{
					visited[minX + y*GameParams.MapSize.Width] = outsideCell;
					outsideCount++;
					queue[queueHead++] = minX + y*GameParams.MapSize.Width;
				}
				if (visited[maxX + y*GameParams.MapSize.Width] == emptyCell)
				{
					visited[maxX + y*GameParams.MapSize.Width] = outsideCell;
					outsideCount++;
					queue[queueHead++] = maxX + y*GameParams.MapSize.Width;
				}
			}

			while (queueTail != queueHead)
			{
				int pointInt = queue[queueTail++];
				var point = new Point(pointInt%GameParams.MapSize.Width, pointInt/GameParams.MapSize.Width);
				foreach (var neighbor in point.GetNeighbors())
				{
					if (neighbor.X < minX || neighbor.X > maxX || neighbor.Y < minY || neighbor.Y > maxY)
					{
						continue;
					}

					if (visited[neighbor.X + neighbor.Y*GameParams.MapSize.Width] != emptyCell)
					{
						continue;
					}

					visited[neighbor.X + neighbor.Y*GameParams.MapSize.Width] = outsideCell;
					outsideCount++;
					queue[queueHead++] = neighbor.X + neighbor.Y*GameParams.MapSize.Width;
				}
			}

			var result = new Point[totalBoxSize - outsideCount - territory.Count];
			int index = 0;
			for (int y = minY; y <= maxY; y++)
			{
				for (int x = minX; x <= maxX; x++)
				{
					if (visited[x + y*GameParams.MapSize.Width] == emptyCell ||
						visited[x + y*GameParams.MapSize.Width] == tailCell)
					{
						result[index++] = new Point(x, y);
					}
				}
			}

			return new PointsSet(result);
		}
	}
}
