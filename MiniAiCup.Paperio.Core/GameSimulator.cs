using System.Collections.Generic;
using System.Linq;

namespace MiniAiCup.Paperio.Core
{
	public class GameSimulator
	{
		private Size _mapSize;

		public GameStateInternal Simulate(GameStateInternal state, Move move)
		{
			_mapSize = state.MapSize;

			int tickNumber = state.TickNumber + state.Speed;
			var bonuses = state.Bonuses;
			var players = state.Players.Values.Select(p => p == state.Me ? SimulatePlayer(p, move) : p).Where(p => p != null);

			return new GameStateInternal(tickNumber, players, bonuses, state, move);
		}

		private PlayerInternal SimulatePlayer(PlayerInternal player, Move move)
		{
			var nextDirection = player.Direction?.GetMoved(move);
			var nextPos = nextDirection == null
				? player.Position
				: player.Position.MoveLogic(nextDirection.Value);

			if (!_mapSize.ContainsPoint(nextPos))
			{
				return null;
			}

			if (player.Lines.Contains(nextPos))
			{
				return null;
			}

			var lines = !player.Territory.Contains(nextPos)
				? player.Lines.Append(nextPos)
				: Path.Empty;

			return new PlayerInternal {
				Direction = nextDirection,
				Lines = lines,
				Position = nextPos,
				Territory = new HashSet<Point>(player.Territory),
				Id = player.Id,
				Bonuses = player.Bonuses,
				Score = player.Score
			};
		}
	}
}
