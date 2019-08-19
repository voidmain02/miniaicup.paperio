using System;
using System.Collections.Generic;

namespace MiniAiCup.Paperio.Core
{
	public class BestTrajectoryFinder
	{
		private readonly int _depth;
		private readonly SimpleGameSimulator _simulator;
		private readonly GameStateScorer _scorer;

		public BestTrajectoryFinder(int depth)
		{
			_depth = depth;
			var capturer = new BfsTerritoryCapturer();
			_simulator = new SimpleGameSimulator(capturer);
			_scorer = new GameStateScorer(capturer);
		}

		public GameStateInternal FindBestState(GameStateInternal initialState)
		{
			int depth;
			if (initialState.Enemies.Length == 0)
			{
				depth = 1;
			}
			else
			{
				int stepsLeft = initialState.Me.GetPathLengthForTime(Constants.MaxTickCount - initialState.TickNumber);
				depth = stepsLeft == 0 ? 1 : Math.Min(_depth, stepsLeft);
			}

			var simulationQueue = new Queue<(GameStateInternal State, int Depth)>();

			simulationQueue.Enqueue((initialState, 0));

			int startTickNumber = initialState.TickNumber;

			GameStateInternal bestState = null;
			int bestScore = Int32.MinValue;

			while (simulationQueue.Count > 0)
			{
				(var currentState, int currentDepth) = simulationQueue.Dequeue();

				foreach (var move in EnumValues.GetAll<Move>())
				{
					var nextState = _simulator.Simulate(currentState, currentState.TickNumber - startTickNumber, move);
					if (nextState == null)
					{
						continue;
					}
					if (currentDepth == depth - 1)
					{
						int score = _scorer.Score(nextState);
						if (score > bestScore)
						{
							bestScore = score;
							bestState = nextState;
						}
					}
					else
					{
						simulationQueue.Enqueue((nextState, currentDepth + 1));
					}
				}
			}

			return bestState;
		}
	}
}
