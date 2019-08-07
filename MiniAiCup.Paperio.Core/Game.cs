using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace MiniAiCup.Paperio.Core
{
	public class Game
	{
		public static GameParams Params { get; private set; }

		public static PointsSet AllMapPoints { get; private set; }

		private Move _lastMove;

		private GameStateInternal _lastState;

		private readonly BestTrajectoryFinder _bestTrajectoryFinder = new BestTrajectoryFinder(5);

		public static void Initialize(GameParams gameParams)
		{
			Params = gameParams;
			AllMapPoints = Params.MapLogicSize.GetAllLogicPoints();
		}

		public Direction GetNextDirection(GameState state)
		{
#if DEBUG
			var stopwatch = new Stopwatch();;
			stopwatch.Start();
			GameDebugData.Current.Reset();
#endif

			if (state.TickNumber == 1)
			{
				return GetStartDirection(state);
			}

			var currentState = _lastState == null
				? new GameStateInternal(state)
				: new GameStateInternal(state, _lastState, _lastMove);

			var bestState = _bestTrajectoryFinder.FindBestState(currentState);

			var statesList = GetStates(bestState, currentState);
			var nextState = statesList.First();
			var nextDirection = nextState.Me.Direction;

			_lastState = currentState;
			_lastMove = nextState.PreviousMove;

#if DEBUG
			stopwatch.Stop();

			GameDebugData.Current.UsedTime = stopwatch.Elapsed;
			GameDebugData.Current.DangerousMap = currentState.DangerousMap;
			GameDebugData.Current.BestTrajectory = statesList.Select(s => s.Me.Position.ConvertToReal(Params.CellSize)).ToArray();
#endif

			return nextDirection.Value;
		}

		public static T[,] GetNewMap<T>()
		{
			return new T[Params.MapLogicSize.Width, Params.MapLogicSize.Height];
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
			var currentPosition = state.Players.First(p => p.Id == Constants.MyId).Position.ConvertToLogic(Params.CellSize);

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
				case Direction.Up: return Params.MapLogicSize.Height - point.Y - 1;
				case Direction.Right: return Params.MapLogicSize.Width - point.X - 1;
				case Direction.Down: return point.Y;
				default:
					throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
			}
		}
	}
}
