using System;
using System.Collections.Generic;
using System.Linq;

namespace MiniAiCup.Paperio.Core
{
	public class Game
	{
		private readonly GameParams _gameParams;

		private bool _isInitialized;

		private Move _lastMove;

		private GameStateInternal _lastState;

		private readonly BestTrajectoryFinder _bestTrajectoryFinder;

		public Game(GameParams gameParams)
		{
			_gameParams = gameParams;
			_bestTrajectoryFinder = new BestTrajectoryFinder(_gameParams.MapLogicSize, 5);
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

			var bestState = _bestTrajectoryFinder.FindBestState(currentState);

			var statesList = GetStates(bestState, currentState);
			var nextState = statesList.First();
			var nextDirection = nextState.Me.Direction;

			_lastState = currentState;
			_lastMove = nextState.PreviousMove;

#if DEBUG
			GameDebugData.Current.GameParams = _gameParams;
			GameDebugData.Current.DangerousMap = currentState.DangerousMap;
			GameDebugData.Current.BestTrajectory = statesList.Select(s => s.Me.Position.ConvertToReal(_gameParams.CellSize)).ToArray();
#endif

			return nextDirection.Value;
		}

		private static List<GameStateInternal> GetStates(GameStateInternal lastState, GameStateInternal initialState)
		{
			var states = new List<GameStateInternal>();
			var currentState = lastState;
			while (currentState != initialState)
			{
				states.Add(currentState);
				currentState = currentState.PreviousState;
			}

			states.Reverse();
			return states;
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
