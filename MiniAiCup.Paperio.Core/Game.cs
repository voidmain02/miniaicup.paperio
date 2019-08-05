using System;
using System.Collections.Generic;
using System.Linq;

namespace MiniAiCup.Paperio.Core
{
	public class Game
	{
		private readonly GameParams _gameParams;

		private readonly GameSimulator _simulator;

		private readonly GameStateScorer _scorer;

		private bool _isInitialized;

		private Move _lastMove;

		private GameStateInternal _lastState;

		public Game(GameParams gameParams)
		{
			_gameParams = gameParams;

			_simulator = new GameSimulator(_gameParams.MapLogicSize);
			_scorer = new GameStateScorer();
		}

		public Direction GetNextDirection(GameState state)
		{
			if (_isInitialized == false)
			{
				_isInitialized = true;
				_lastMove = Move.Forward;
				return GetStartDirection(state);
			}

			var currentState = _lastState == null
				? new GameStateInternal(state, _gameParams)
				: new GameStateInternal(state, _lastState, _lastMove);

			var bestMove = Move.Forward;
			int bestScore = Int32.MinValue;
			GameStateInternal stateAfterBestMove = null;

			var movesScores = new Dictionary<Move, int>();
			foreach (var move in EnumValues.GetAll<Move>())
			{
				var nextState = _simulator.Simulate(currentState, move);
				int nextStateScore = _scorer.Score(nextState);
				movesScores[move] = nextStateScore;
				if (nextStateScore > bestScore)
				{
					bestScore = nextStateScore;
					bestMove = move;
					stateAfterBestMove = nextState;
				}
			}

			_lastState = currentState;
			_lastMove = bestMove;

#if DEBUG
			GameDebugData.Current.GameParams = _gameParams;
			GameDebugData.Current.PathToHome = stateAfterBestMove.Me.PathToHome.Select(p => p.ConvertToReal(_gameParams.CellSize)).ToArray();
			GameDebugData.Current.MoveScores = movesScores;
			GameDebugData.Current.DangerousMap = currentState.DangerousMap;
#endif

			return currentState.Me.Direction.Value.GetMoved(bestMove);
		}

		private Direction GetStartDirection(GameState state)
		{
			var currentPosition = state.Players.First(p => p.Id == Constants.MyId).Position.ConvertToLogic(_gameParams.CellSize);

			int maxDistance = 0;
			var maxDistanceDirection = Direction.Left;
			foreach (var direction in EnumValues.GetAll<Direction>())
			{
				int distance = GetDistanceToBorder(currentPosition, direction);
				if (distance > maxDistance)
				{
					maxDistance = distance;
					maxDistanceDirection = direction;
				}
			}

			return maxDistanceDirection;
		}

		private int GetDistanceToBorder(Point point, Direction direction)
		{
			switch (direction)
			{
				case Direction.Left: return point.X;
				case Direction.Up: return _gameParams.MapLogicSize.Height - point.Y - 1;
				case Direction.Right: return _gameParams.MapLogicSize.Width - point.X - 1;
				case Direction.Down: return point.Y;
				default:
					throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
			}
		}
	}
}
