using System.Collections.Generic;
using System.Linq;

namespace MiniAiCup.Paperio.Core
{
	public class PathFinder
	{
		public static Point[] GetShortestPath(Point startPoint, HashSet<Point> destinationHashSet, HashSet<Point> obstaclesHashSet, Size mapSize)
		{
			if (destinationHashSet.Contains(startPoint))
			{
				return new Point[] {};
			}

			var moves = new int[mapSize.Width, mapSize.Height];
			var isVisited = new bool[mapSize.Width, mapSize.Height];
			var queue = new Queue<Point>();
			queue.Enqueue(startPoint);
			isVisited[startPoint.X, startPoint.Y] = true;
			moves[startPoint.X, startPoint.Y] = 0;
			while (queue.Count > 0)
			{
				var point = queue.Dequeue();
				int currentPathLength = moves[point.X, point.Y];
				foreach (var neighbor in point.GetNeighbors())
				{
					if (destinationHashSet.Contains(neighbor))
					{
						moves[neighbor.X, neighbor.Y] = currentPathLength + 1;
						return GetPath(moves, neighbor);
					}

					if (mapSize.ContainsPoint(neighbor) && !isVisited[neighbor.X, neighbor.Y] && !obstaclesHashSet.Contains(neighbor))
					{
						isVisited[neighbor.X, neighbor.Y] = true;
						moves[neighbor.X, neighbor.Y] = currentPathLength + 1;
						queue.Enqueue(neighbor);
					}
				}
			}

			Point[] GetPath(int[,] movesMap, Point currentPoint)
			{
				int currentLength = movesMap[currentPoint.X, currentPoint.Y];
				var resultPath = new List<Point>(currentLength);

				for (int i = currentLength - 1; i >= 0; i--)
				{
					resultPath.Add(currentPoint);
					var validNeighbors = currentPoint.GetNeighbors().Where(p => mapSize.ContainsPoint(p));
					foreach (var validNeighbor in validNeighbors)
					{
						if (moves[validNeighbor.X, validNeighbor.Y] == i)
						{
							currentPoint = validNeighbor;
							break;
						}
					}
				}

				resultPath.Reverse();
				return resultPath.ToArray();
			}

			return null;
		}
	}
}
