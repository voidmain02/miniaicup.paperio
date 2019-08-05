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
				var freeTerritory = state.AllMapPoints.ExceptWith(state.Me.Territory);
				var obstacles = new PointsSet(new[] { state.Me.Position.MoveLogic(state.Me.Direction.Value.GetOpposite()) });
				var pathToOutside = PathFinder.GetShortestPath(state.Me.Position, freeTerritory, obstacles, state.MapSize);

				int pathToOutsidePenalty = 1 - pathToOutside.Length;
				int backToHomeBonus = state.PreviousState?.Me.Tail.Length ?? 0;
				return backToHomeBonus + pathToOutsidePenalty;
			}

			if (state.Me.PathToHome == null)
			{
				return -900;
			}

			var myTailWithShortestPathToHome = state.Me.Tail.AsPointsSet().UnionWith(state.Me.PathToHome.Take(state.Me.PathToHome.Length - 1));
			int minPathFromEnemyToMyTail = state.Enemies.Length == 0
				? Int32.MaxValue
				: myTailWithShortestPathToHome.Min(p => state.DangerousMap[p.X, p.Y]) - 1;

			if (minPathFromEnemyToMyTail <= state.Me.PathToHome.Length)
			{
				return (minPathFromEnemyToMyTail - state.Me.PathToHome.Length - 2)*10;
			}

			int outsideBonus = 10;
			int longPathPenalty = state.Enemies.Length > 0 ? Math.Min(20 - state.Me.Tail.Length, 0) : 0;
			int longPathToHomePenalty = state.Enemies.Length > 0 ? Math.Min(6 - state.Me.PathToHome.Length, 0) : 0;
			int forwardMoveBonus = state.PreviousMove == Move.Forward ? 1 : 0;
			int movesLeft = (Constants.MaxTickCount - state.TickNumber)/(state.CellSize/state.Speed);
			int notEnoughTimePenalty = Math.Min((movesLeft - state.Me.PathToHome.Length)*10, 0);
			return outsideBonus + longPathPenalty + longPathToHomePenalty + forwardMoveBonus + notEnoughTimePenalty;
		}
	}
}
