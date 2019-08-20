using System.Linq;

namespace MiniAiCup.Paperio.Core
{
	public class PathFinder
	{
		public static unsafe Path GetShortestPathToHome(PlayerInternal player)
		{
			var queue = stackalloc int[GameParams.MapSize.Width*GameParams.MapSize.Height];
			int queueHead = 0;
			int queueTail = 0;

			var visited = stackalloc bool[GameParams.MapSize.Width*GameParams.MapSize.Height];
			var moves = stackalloc int[GameParams.MapSize.Width*GameParams.MapSize.Height];

			var startPoint = player.PathToNextPositionLength == 0 ? player.Position : player.Position.MoveLogic(player.Direction.Value);
			var prevPoint = player.PathToNextPositionLength > 0 ? player.Position : player.Position.MoveLogic(player.Direction.Value.GetOpposite());

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
					if (point == startPoint && neighbor == prevPoint)
					{
						continue;
					}

					int neighborCoord = neighbor.X + neighbor.Y*GameParams.MapSize.Width;
					if (player.Territory.Contains(neighbor))
					{
						moves[neighborCoord] = currentPathLength + 1;
						return GetPath(neighbor);
					}

					if (GameParams.MapSize.ContainsPoint(neighbor) && !visited[neighborCoord] && !player.Tail.AsPointsSet().Contains(neighbor))
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
				if (player.PathToNextPositionLength == 0)
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
				else
				{
					int currentLength = moves[currentPoint.X + currentPoint.Y*GameParams.MapSize.Width];
					var resultPath = new Point[currentLength + 1];

					for (int i = currentLength; i >= 1; i--)
					{
						resultPath[i] = currentPoint;
						var validNeighbors = currentPoint.GetNeighbors().Where(p => GameParams.MapSize.ContainsPoint(p));
						foreach (var validNeighbor in validNeighbors)
						{
							if (moves[validNeighbor.X + validNeighbor.Y*GameParams.MapSize.Width] == i - 1)
							{
								currentPoint = validNeighbor;
								break;
							}
						}
					}

					resultPath[0] = startPoint;

					return new Path(resultPath);
				}
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
