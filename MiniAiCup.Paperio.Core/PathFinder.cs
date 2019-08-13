using System.Collections.Generic;
using System.Linq;

namespace MiniAiCup.Paperio.Core
{
	public class PathFinder
	{
		private static readonly bool[,] Visited;

		private static readonly bool[,] EmptyVisited;

		private static readonly int[,] Moves;

		private static readonly Queue<Point> Queue;

		static PathFinder()
		{
			Queue = new Queue<Point>(Game.Params.MapLogicSize.Width*Game.Params.MapLogicSize.Height);
			Visited = Game.GetNewMap<bool>();
			EmptyVisited = Game.GetNewMap<bool>();
			Moves = Game.GetNewMap<int>();
		}

		public static Path GetShortestPath(Point startPoint, PointsSet destinationPoints, PointsSet obstaclesPoints)
		{
			if (destinationPoints.Contains(startPoint))
			{
				return Path.Empty;
			}

			Reset();

			Queue.Enqueue(startPoint);
			Visited[startPoint.X, startPoint.Y] = true;
			Moves[startPoint.X, startPoint.Y] = 0;
			while (Queue.Count > 0)
			{
				var point = Queue.Dequeue();
				int currentPathLength = Moves[point.X, point.Y];
				foreach (var neighbor in point.GetNeighbors())
				{
					if (destinationPoints.Contains(neighbor))
					{
						Moves[neighbor.X, neighbor.Y] = currentPathLength + 1;
						return GetPath(neighbor);
					}

					if (Game.Params.MapLogicSize.ContainsPoint(neighbor) && !Visited[neighbor.X, neighbor.Y] && !obstaclesPoints.Contains(neighbor))
					{
						Visited[neighbor.X, neighbor.Y] = true;
						Moves[neighbor.X, neighbor.Y] = currentPathLength + 1;
						Queue.Enqueue(neighbor);
					}
				}
			}

			return null;
		}

		private static Path GetPath(Point currentPoint)
		{
			int currentLength = Moves[currentPoint.X, currentPoint.Y];
			var resultPath = new Point[currentLength];

			for (int i = currentLength - 1; i >= 0; i--)
			{
				resultPath[i] = currentPoint;
				var validNeighbors = currentPoint.GetNeighbors().Where(p => Game.Params.MapLogicSize.ContainsPoint(p));
				foreach (var validNeighbor in validNeighbors)
				{
					if (Moves[validNeighbor.X, validNeighbor.Y] == i)
					{
						currentPoint = validNeighbor;
						break;
					}
				}
			}

			return new Path(resultPath);
		}

		private static void Reset()
		{
			Queue.Clear();
			Utils.FastCopyArray(EmptyVisited, Visited, Game.Params.MapLogicSize.Width*Game.Params.MapLogicSize.Height);
			Utils.FastCopyArray(Game.NoEnemiesDangerousMap, Moves, Game.Params.MapLogicSize.Width*Game.Params.MapLogicSize.Height);
		}
	}
}
