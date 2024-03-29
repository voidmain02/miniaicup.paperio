using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using MiniAiCup.Paperio.Core.Debug;

namespace MiniAiCup.Paperio.Core
{
	public class Game
	{
		public static int[,] NoEnemiesDangerousMap { get; private set; }

		public static int[][,] NoTailStandardSpeedTimeMaps { get; private set; }

		private GameStateInternal _lastState;

		private readonly BestTrajectoryFinder _bestTrajectoryFinder = new BestTrajectoryFinder(9);

		public static void Initialize()
		{
			BuildNoEnemiesDangerousMap();
			BuildNoTailDistanceMaps();
		}

		private static void BuildNoEnemiesDangerousMap()
		{
			NoEnemiesDangerousMap = GetNewMap<int>();
			for (int y = 0; y < GameParams.MapSize.Height; y++)
			{
				for (int x = 0; x < GameParams.MapSize.Width; x++)
				{
					NoEnemiesDangerousMap[x, y] = Int32.MaxValue;
				}
			}
		}

		private static void BuildNoTailDistanceMaps()
		{
			NoTailStandardSpeedTimeMaps = new int[4][,];
			var center = new Point(GameParams.MapSize.Width - 1, GameParams.MapSize.Height - 1);
			int width = GameParams.MapSize.Width*2 - 1;
			int height = GameParams.MapSize.Height*2 - 1;
			for (int i = 0; i < 4; i++)
			{
				NoTailStandardSpeedTimeMaps[i] = new int[width, height];
				for (int y = 0; y < height; y++)
				{
					for (int x = 0; x < width; x++)
					{
						NoTailStandardSpeedTimeMaps[i][x, y] = center.GetDistanceTo(new Point(x, y))*GameParams.CellSize/GameParams.Speed;
					}
				}

				switch ((Direction)i)
				{
					case Direction.Left:
						for (int x = center.X + 1; x < width; x++)
						{
							NoTailStandardSpeedTimeMaps[i][x, center.Y] += 2*GameParams.CellSize/GameParams.Speed;
						}
						break;
					case Direction.Up:
						for (int y = center.Y - 1; y >= 0; y--)
						{
							NoTailStandardSpeedTimeMaps[i][center.X, y] += 2*GameParams.CellSize/GameParams.Speed;
						}
						break;
					case Direction.Right:
						for (int x = center.X - 1; x >= 0; x--)
						{
							NoTailStandardSpeedTimeMaps[i][x, center.Y] += 2*GameParams.CellSize/GameParams.Speed;
						}
						break;
					case Direction.Down:
						for (int y = center.Y + 1; y < height; y++)
						{
							NoTailStandardSpeedTimeMaps[i][center.X, y] += 2*GameParams.CellSize/GameParams.Speed;
						}
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
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
				: new GameStateInternal(state, _lastState);

			var bestState = _bestTrajectoryFinder.FindBestState(currentState);
			_lastState = currentState;

			Direction nextDirection;
			if (bestState != null)
			{
				var nextState = GetNextState(bestState, currentState);
				nextDirection = nextState.Me.Direction.Value;
			}
			else if (currentState.Me.PathToHome != null && currentState.Me.PathToHome.Length > 0)
			{
				nextDirection = currentState.Me.Position.GetDirectionTo(currentState.Me.PathToHome[0]);
			}
			else
			{
				nextDirection = currentState.Me.Direction ?? Direction.Left;
			}

#if DEBUG
			stopwatch.Stop();

			GameDebugData.Current.UsedTime = stopwatch.Elapsed;
			GameDebugData.Current.DangerousMap = currentState.DangerousMap;
			GameDebugData.Current.BestTrajectory = bestState != null
				? GetStates(bestState, currentState).Select(s => s.Me.Position.ConvertToReal(GameParams.CellSize)).ToArray()
				: currentState.Me.PathToHome.Select(p => p.ConvertToReal(GameParams.CellSize)).ToArray();
#endif

			return nextDirection;
		}

		public static T[,] GetNewMap<T>()
		{
			return new T[GameParams.MapSize.Width, GameParams.MapSize.Height];
		}

		private static GameStateInternal GetNextState(GameStateInternal lastState, GameStateInternal initialState)
		{
			var currentState = lastState;
			while (currentState.PreviousState != initialState)
			{
				currentState = currentState.PreviousState;
			}

			return currentState;
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
			var currentPosition = state.Players.First(p => p.Id == Constants.MyId).Position.ConvertToLogic(GameParams.CellSize);

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
				case Direction.Up: return GameParams.MapSize.Height - point.Y - 1;
				case Direction.Right: return GameParams.MapSize.Width - point.X - 1;
				case Direction.Down: return point.Y;
				default:
					throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
			}
		}
	}
}
