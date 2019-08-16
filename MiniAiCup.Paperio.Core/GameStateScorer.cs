using System;
using MiniAiCup.Paperio.Core.Debug;

namespace MiniAiCup.Paperio.Core
{
	public class GameStateScorer
	{
		private const int SlowdownInScores = -30;

		private const int NitroInScores = 10;

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

			if (state.Me.GetTimeForPath(state.Me.PathToHome.Length) >= Constants.MaxTickCount - state.TickNumber)
			{
				return -500;
			}

			int potentialScore = CalcPotentialTerritoryCaptureScore(state);
			int potentialScoreBonus = (int)(potentialScore*0.9);
			int bonusScore = state.Me.NitroStepsLeft > state.Me.SlowdownStepsLeft
				? NitroInScores
				: state.Me.SlowdownStepsLeft > state.Me.NitroStepsLeft
					? SlowdownInScores
					: 0;

			return (state.Me.Score + potentialScoreBonus + bonusScore)*scoresMultiplicator;
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

			int nitroCount = 0;
			int slowdownCount = 0;
			foreach (var bonus in state.Bonuses)
			{
				if (capturedTerritory.Contains(bonus.Position))
				{
					switch (bonus.Type)
					{
						case BonusType.Nitro:
							nitroCount++;
							break;
						case BonusType.Slowdown:
							slowdownCount++;
							break;
						case BonusType.Saw:
							break;
						default:
							throw new ArgumentOutOfRangeException();
					}
				}
			}

			int bonusScore = nitroCount > slowdownCount
				? NitroInScores
				: slowdownCount > nitroCount
					? SlowdownInScores
					: 0;

			return score + bonusScore;
		}
	}
}
