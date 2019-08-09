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
			_simulator = new SimpleGameSimulator();
			_scorer = new GameStateScorer();
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
				int stepsLeft = (Constants.MaxTickCount - initialState.TickNumber)/(Game.Params.CellSize/Game.Params.Speed);
				depth = Math.Min(_depth, stepsLeft + 1);
			}

			var simulationQueue = new Queue<(GameStateInternal State, int Depth)>();
			var finalSimulations = new List<GameStateInternal>();

			simulationQueue.Enqueue((initialState, 0));

			while (simulationQueue.Count > 0)
			{
				(var currentState, int currentDepth) = simulationQueue.Dequeue();

				foreach (var move in EnumValues.GetAll<Move>())
				{
					var nextState = _simulator.Simulate(currentState, currentDepth, move);
					if (nextState.Me == null)
					{
						continue;
					}
					if (currentDepth == depth - 1)
					{
						finalSimulations.Add(nextState);
					}
					else
					{
						simulationQueue.Enqueue((nextState, currentDepth + 1));
					}
				}
			}

			GameStateInternal bestState = null;
			int bestScore = Int32.MinValue;
			foreach (var state in finalSimulations)
			{
				int score = _scorer.Score(state);
				if (score > bestScore)
				{
					bestScore = score;
					bestState = state;
				}
			}

			return bestState;
		}
	}
}
