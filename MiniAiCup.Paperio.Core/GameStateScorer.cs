using System.Collections.Generic;
using MiniAiCup.Paperio.Core.Debug;

namespace MiniAiCup.Paperio.Core
{
	public class GameStateScorer
	{
		private readonly BfsTerritoryCapturer _territoryCapturer;

		public GameStateScorer(BfsTerritoryCapturer territoryCapturer)
		{
			_territoryCapturer = territoryCapturer;
		}

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
				if (freeTerritory.Count == 0)
				{
					return state.Me.Score*scoresMultiplicator;
				}

				var obstacles = new PointsSet(new[] { state.Me.Position.MoveLogic(state.Me.Direction.Value.GetOpposite()) });
				var pathToOutside = PathFinder.GetShortestPath(state.Me.Position, freeTerritory, obstacles);

				int pathToOutsidePenalty = 1 - pathToOutside.Length;
				return state.Me.Score*scoresMultiplicator + pathToOutsidePenalty;
			}

			if (state.Me.PathToHome == null)
			{
				return -900;
			}

			int stepsLeft = (Constants.MaxTickCount - state.TickNumber)/(Game.Params.CellSize/Game.Params.Speed);
			if (state.Me.PathToHome.Length >= stepsLeft)
			{
				return -500;
			}

			int potentialScore = CalcPotentialTerritoryCaptureScore(state);
			int potentialScoreBonus = (int)(potentialScore*0.9);

			return (state.Me.Score + potentialScoreBonus)*scoresMultiplicator;
		}

		public int CalcPotentialTerritoryCaptureScore(GameStateInternal state)
		{
			var tailWithPathToHome = new Point[state.Me.Tail.Length + state.Me.PathToHome.Length - 1];
			for (int i = 0; i < state.Me.Tail.Length; i++)
			{
				tailWithPathToHome[i] = state.Me.Tail[i];
			}

			for (int i = 0; i < state.Me.PathToHome.Length - 1; i++)
			{
				tailWithPathToHome[state.Me.Tail.Length + i] = state.Me.PathToHome[i];
			}

			var capturedTerritory = _territoryCapturer.Capture(state.Me.Territory, tailWithPathToHome);
			
			int score = capturedTerritory.Count*Constants.NeutralTerritoryScore;
			foreach (var enemy in state.Enemies)
			{
				int srcCount = enemy.Territory.Count;
				var enemyCroppedTerritory = enemy.Territory.ExceptWith(capturedTerritory);
				int croppedCount = enemyCroppedTerritory.Count;
				score += (srcCount - croppedCount)*(Constants.EnemyTerritoryScore - Constants.NeutralTerritoryScore);
			}

			return score;
		}
	}
}
