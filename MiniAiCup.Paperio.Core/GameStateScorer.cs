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
				if (state.Me.Territory.Count == GameParams.MapSize.Width*GameParams.MapSize.Height)
				{
					return state.Me.Score*scoresMultiplicator;
				}

				int pathToOutsideLength = PathFinder.GetShortestPathToOutsideLength(state.Me.Position, state.Me.Direction.Value, state.Me.Territory);

				int pathToOutsidePenalty = 1 - pathToOutsideLength;
				return state.Me.Score*scoresMultiplicator + pathToOutsidePenalty;
			}

			if (state.Me.PathToHome == null)
			{
				return -900;
			}

			int stepsLeft = (Constants.MaxTickCount - state.TickNumber)/(GameParams.CellSize/GameParams.Speed);
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
			if (state.Me.Tail.Length == 0)
			{
				return 0;
			}

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
			int enemyTerritoryPoints = 0;
			foreach (var point in capturedTerritory)
			{
				foreach (var enemy in state.Enemies)
				{
					if (enemy.Territory.Contains(point))
					{
						enemyTerritoryPoints++;
						break;
					}
				}
			}
			score += enemyTerritoryPoints*(Constants.EnemyTerritoryScore - Constants.NeutralTerritoryScore);

			return score;
		}
	}
}
