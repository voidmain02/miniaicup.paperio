using System.Collections.Generic;

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

			if (state.Me.Tail.Length == 0)
			{
				var freeTerritory = state.AllMapPoints.ExceptWith(state.Me.Territory);
				var obstacles = new PointsSet(new[] { state.Me.Position.MoveLogic(state.Me.Direction.Value.GetOpposite()) });
				var pathToOutside = PathFinder.GetShortestPath(state.Me.Position, freeTerritory, obstacles, state.MapSize);

				int pathToOutsidePenalty = 1 - pathToOutside.Length;
				return state.Me.Score + pathToOutsidePenalty;
			}

			if (state.Me.PathToHome == null)
			{
				return -900;
			}

			var territoryCapturer = new BfsTerritoryCapturer(state.MapSize);
			var points = new List<Point>();
			points.AddRange(state.Me.Tail);
			points.AddRange(state.Me.PathToHome);
			var captured = territoryCapturer.Capture(state.Me.Territory, new Path(points));
			int potentialScore = captured.Count*Constants.NeutralTerritoryScore;
			int potentialScoreBonus = (int)(potentialScore*0.9);

			return state.Me.Score + potentialScoreBonus;
		}
	}
}
