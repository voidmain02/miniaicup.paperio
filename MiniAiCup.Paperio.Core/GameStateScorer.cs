using System;
using System.Linq;

namespace MiniAiCup.Paperio.Core
{
	public class GameStateScorer
	{
		public int Score(GameStateInternal state)
		{
			if (state.Me == null)
			{
				return -1000;
			}

			if (state.Me.Direction == null)
			{
				return -800;
			}

			if (state.Me.Territory.Count == state.MapSize.Width*state.MapSize.Height)
			{
				return 0;
			}

			if (state.Me.Tail.Length == 0)
			{
				var freeTerritory = new PointsSet(GetAllPoints(state.MapSize)).ExceptWith(state.Me.Territory);
				var obstacles = new PointsSet(new[] { state.Me.Position.MoveLogic(state.Me.Direction.Value.GetOpposite()) });
				var pathToOutside = PathFinder.GetShortestPath(state.Me.Position, freeTerritory, obstacles, state.MapSize);

				int pathToOutsidePenalty = 1 - pathToOutside.Length;
				int backToHomeBonus = state.PreviousState?.Me.Tail.Length ?? 0;
				return backToHomeBonus + pathToOutsidePenalty;
			}

			if (state.PathToHome == null)
			{
				return -900;
			}

			var myTailWithShortestPathToHome = new PointsSet(state.Me.Tail).UnionWith(state.PathToHome.Take(state.PathToHome.Length - 1));
			int minPathFromEnemyToMyTail = state.Enemies.Length == 0
				? Int32.MaxValue
				: state.Enemies.Select(e => PathFinder.GetShortestPath(e.Position, myTailWithShortestPathToHome, e.Tail.AsPointsSet(), state.MapSize)?.Length ?? Int32.MaxValue).Min() - 1;

			if (minPathFromEnemyToMyTail <= state.PathToHome.Length)
			{
				return (minPathFromEnemyToMyTail - state.PathToHome.Length - 2)*10;
			}

			int outsideBonus = 10;
			int longPathPenalty = state.Enemies.Length > 0 ? Math.Min(20 - state.Me.Tail.Length, 0) : 0;
			int longPathToHomePenalty = state.Enemies.Length > 0 ? Math.Min(6 - state.PathToHome.Length, 0) : 0;
			int forwardMoveBonus = state.PreviousMove == Move.Forward ? 1 : 0;
			int movesLeft = (Constants.MaxTickCount - state.TickNumber)/(state.CellSize/state.Speed);
			int notEnoughTimePenalty = Math.Min((movesLeft - state.PathToHome.Length)*10, 0);
			return outsideBonus + longPathPenalty + longPathToHomePenalty + forwardMoveBonus + notEnoughTimePenalty;
		}

		public static Point[] GetAllPoints(Size size)
		{
			var points = new Point[size.Width*size.Height];
			for (int y = 0; y < size.Height; y++)
			{
				for (int x = 0; x < size.Width; x++)
				{
					points[size.Width*y + x] = new Point(x, y);
				}
			}

			return points;
		}
	}
}
