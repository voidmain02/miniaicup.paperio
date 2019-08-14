using System.Linq;

namespace MiniAiCup.Paperio.Core
{
	public class PathFinder
	{
		public static unsafe Path GetShortestPath(Point startPoint, PointsSet destinationPoints, PointsSet obstaclesPoints)
		{
			var queue = stackalloc int[GameParams.MapSize.Width*GameParams.MapSize.Height];
			int queueHead = 0;
			int queueTail = 0;

			var visited = stackalloc bool[GameParams.MapSize.Width*GameParams.MapSize.Height];
			var moves = stackalloc int[GameParams.MapSize.Width*GameParams.MapSize.Height];

			visited[startPoint.X + startPoint.Y*GameParams.MapSize.Width] = true;
			moves[startPoint.X + startPoint.Y*GameParams.MapSize.Width] = 0;
			queue[queueHead++] = startPoint.X + startPoint.Y*GameParams.MapSize.Width;

			while (queueTail != queueHead)
			{
				int coord = queue[queueTail++];
				var point = new Point(coord%GameParams.MapSize.Width, coord/GameParams.MapSize.Width);
				int currentPathLength = moves[coord];
				foreach (var neighbor in point.GetNeighbors())
				{
					int neighborCoord = neighbor.X + neighbor.Y*GameParams.MapSize.Width;
					if (destinationPoints.Contains(neighbor))
					{
						moves[neighborCoord] = currentPathLength + 1;
						return GetPath(neighbor);
					}

					if (GameParams.MapSize.ContainsPoint(neighbor) && !visited[neighborCoord] && !obstaclesPoints.Contains(neighbor))
					{
						visited[neighborCoord] = true;
						moves[neighborCoord] = currentPathLength + 1;
						queue[queueHead++] = neighborCoord;
					}
				}
			}

			return null;

			Path GetPath(Point currentPoint)
			{
				int currentLength = moves[currentPoint.X + currentPoint.Y*GameParams.MapSize.Width];
				var resultPath = new Point[currentLength];

				for (int i = currentLength - 1; i >= 0; i--)
				{
					resultPath[i] = currentPoint;
					var validNeighbors = currentPoint.GetNeighbors().Where(p => GameParams.MapSize.ContainsPoint(p));
					foreach (var validNeighbor in validNeighbors)
					{
						if (moves[validNeighbor.X + validNeighbor.Y*GameParams.MapSize.Width] == i)
						{
							currentPoint = validNeighbor;
							break;
						}
					}
				}

				return new Path(resultPath);
			}
		}

		public static unsafe int GetShortestPathToOutsideLength(Point startPoint, Direction direction, PointsSet territory)
		{
			var queue = stackalloc int[GameParams.MapSize.Width*GameParams.MapSize.Height];
			int queueHead = 0;
			int queueTail = 0;

			var visited = stackalloc bool[GameParams.MapSize.Width*GameParams.MapSize.Height];
			var moves = stackalloc int[GameParams.MapSize.Width*GameParams.MapSize.Height];

			visited[startPoint.X + startPoint.Y*GameParams.MapSize.Width] = true;
			moves[startPoint.X + startPoint.Y*GameParams.MapSize.Width] = 0;
			queue[queueHead++] = startPoint.X + startPoint.Y*GameParams.MapSize.Width;

			while (queueTail != queueHead)
			{
				int coord = queue[queueTail++];
				var point = new Point(coord%GameParams.MapSize.Width, coord/GameParams.MapSize.Width);
				int currentPathLength = moves[coord];
				foreach (var neighbor in point.GetNeighbors())
				{
					int neighborCoord = neighbor.X + neighbor.Y*GameParams.MapSize.Width;
					if (!GameParams.MapSize.ContainsPoint(neighbor) || visited[neighborCoord] ||
						currentPathLength == 0 && neighbor == startPoint.MoveLogic(direction.GetOpposite()))
					{
						continue;
					}

					if (!territory.Contains(neighbor))
					{
						return currentPathLength + 1;
					}

					visited[neighborCoord] = true;
					moves[neighborCoord] = currentPathLength + 1;
					queue[queueHead++] = neighborCoord;
				}
			}

			return -1;
		}
	}
}
