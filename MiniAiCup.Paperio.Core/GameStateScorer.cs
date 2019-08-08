using System.Collections.Generic;
using MiniAiCup.Paperio.Core.Debug;

namespace MiniAiCup.Paperio.Core
{
	public class GameStateScorer
	{
		public int Score(GameStateInternal state)
		{
#if DEBUG
			GameDebugData.Current.ScoringsCount++;
#endif

			if (state.Me == null)
			{
				return -1000;
			}

			if (state.Me.Direction == null)
			{
				return -800;
			}

			const int scoresMultiplicator = 10;

			if (state.Me.Tail.Length == 0)
			{
				var freeTerritory = Game.AllMapPoints.ExceptWith(state.Me.Territory);
				var obstacles = new PointsSet(new[] { state.Me.Position.MoveLogic(state.Me.Direction.Value.GetOpposite()) });
				var pathToOutside = PathFinder.GetShortestPath(state.Me.Position, freeTerritory, obstacles);

				int pathToOutsidePenalty = 1 - pathToOutside.Length;
				return state.Me.Score*scoresMultiplicator + pathToOutsidePenalty;
			}

			if (state.Me.PathToHome == null)
			{
				return -900;
			}

			int potentialScore = CalcPotentialTerritoryCaptureScore(state);
			int potentialScoreBonus = (int)(potentialScore*0.9);

			return (state.Me.Score + potentialScoreBonus)*scoresMultiplicator;
		}

		public int CalcPotentialTerritoryCaptureScore(GameStateInternal state)
		{
			var territoryCapturer = new BfsTerritoryCapturer();

			var tailWithPathToHome = new List<Point>(state.Me.Tail.Length + state.Me.PathToHome.Length);
			tailWithPathToHome.AddRange(state.Me.Tail);
			tailWithPathToHome.AddRange(state.Me.PathToHome);
			var capturedTerritory = territoryCapturer.Capture(state.Me.Territory, new Path(tailWithPathToHome));
			
			int score = capturedTerritory.Count*Constants.NeutralTerritoryScore;
			foreach (var enemy in state.Enemies)
			{
				int srcCount = enemy.Territory.Count;
				enemy.Territory = enemy.Territory.ExceptWith(capturedTerritory);
				int croppedCount = enemy.Territory.Count;
				score += (srcCount - croppedCount)*(Constants.EnemyTerritoryScore - Constants.NeutralTerritoryScore);
			}

			return score;
		}
	}
}
