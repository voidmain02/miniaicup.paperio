using System;
using System.Linq;
using BenchmarkDotNet.Attributes;
using MiniAiCup.Paperio.Core;
using Newtonsoft.Json.Linq;

namespace MiniAiCup.Paperio.Benchmark
{
	[ClrJob, MonoJob]
	[RankColumn]
	public class TrajectoryFindBenchmark
	{
		private readonly GameStateInternal _initialState;
		private const int Depth = 6;
		private readonly BestTrajectoryFinder _bestTrajectoryFinder;

		public TrajectoryFindBenchmark()
		{
			Game.Initialize();

			_bestTrajectoryFinder = new BestTrajectoryFinder(Depth);

			string json = "{\"type\":\"tick\",\"players\":{\"i\":{\"score\":72,\"direction\":\"up\",\"territory\":[[15,225],[15,255],[15,285],[15,315],[15,345]," +
				"[15,375],[15,405],[45,195],[45,225],[45,255],[45,285],[45,315],[45,345],[45,375],[45,405],[75,165],[75,195],[75,225],[75,255],[75,285],[75,315]," +
				"[75,345],[75,375],[75,405],[105,165],[105,195],[105,225],[105,255],[105,285],[105,315],[105,345],[105,375],[135,165],[135,195],[135,225],[135,255]," +
				"[135,285],[135,315],[135,345],[135,375],[165,165],[165,195],[165,225],[165,255],[165,285],[165,315],[165,345],[165,375],[165,405],[195,165],[195,195]," +
				"[195,225],[195,255],[195,285],[195,315],[195,345],[195,375],[195,405],[225,255],[225,285],[225,315],[225,345],[225,375],[225,405],[255,285],[255,315]," +
				"[255,345],[255,375],[255,405],[285,315],[285,345],[285,375],[285,405],[315,315],[315,345],[315,375],[315,405],[345,315],[345,345],[345,375],[345,405]]," +
				"\"lines\":[[225,225],[255,225],[285,225],[315,225],[345,225],[375,225]],\"position\":[375,250],\"bonuses\":[]},\"2\":{\"score\":25,\"direction\":\"right\"," +
				"\"territory\":[[45,615],[45,645],[45,675],[75,615],[75,645],[75,675],[75,705],[105,645],[105,675],[105,705],[135,645],[135,675],[135,705],[135,735],[165,645]," +
				"[165,675],[165,705],[165,735],[165,765],[195,645],[195,675],[195,705],[195,735],[195,765],[225,645],[225,675],[225,705],[225,735],[255,645],[255,675]," +
				"[255,705],[255,735],[285,645],[285,675]],\"lines\":[[135,615],[165,615],[195,615],[225,615]],\"position\":[250,615],\"bonuses\":[]},\"3\":{\"score\":16," +
				"\"direction\":\"up\",\"territory\":[[405,705],[405,735],[435,675],[435,705],[435,735],[435,765],[435,795],[465,675],[465,705],[465,735],[465,765],[465,795]," +
				"[495,675],[495,705],[495,735],[495,765],[495,795],[525,705],[525,735],[525,765],[525,795],[555,705],[555,735],[555,765],[555,795]],\"lines\":[]," +
				"\"position\":[525,730],\"bonuses\":[]},\"4\":{\"score\":25,\"direction\":\"right\",\"territory\":[[375,45],[375,75],[375,105],[375,135],[405,15],[405,45]," +
				"[405,75],[405,105],[405,135],[405,165],[435,15],[435,45],[435,75],[435,105],[435,135],[435,165],[435,195],[465,15],[465,45],[465,75],[465,105],[465,135]," +
				"[465,165],[465,195],[495,45],[495,75],[495,105],[495,135],[495,165],[495,195],[525,75],[525,105],[525,135],[525,165]],\"lines\":[],\"position\":[430,45]," +
				"\"bonuses\":[]},\"5\":{\"score\":35,\"direction\":\"left\",\"territory\":[[555,135],[555,165],[555,195],[555,225],[555,255],[585,135],[585,165],[585,195]," +
				"[585,225],[585,255],[615,135],[615,165],[615,195],[615,225],[615,255],[645,135],[645,165],[645,195],[645,225],[645,255],[645,285],[645,315],[675,135]," +
				"[675,165],[675,195],[675,225],[675,255],[675,285],[675,315],[705,225],[705,255],[705,285],[705,315],[735,225],[735,255],[735,285],[735,315],[765,225]," +
				"[765,255],[765,285],[765,315],[795,225],[795,255],[795,285]],\"lines\":[[615,285]],\"position\":[590,285],\"bonuses\":[]},\"6\":{\"score\":32,\"direction\":\"up\"," +
				"\"territory\":[[645,705],[645,735],[645,765],[645,795],[645,825],[675,555],[675,585],[675,615],[675,645],[675,675],[675,705],[675,735],[675,765],[675,795]," +
				"[675,825],[705,555],[705,585],[705,615],[705,645],[705,675],[705,705],[705,735],[705,765],[705,795],[705,825],[735,645],[735,675],[735,705],[735,735]," +
				"[735,765],[735,795],[735,825],[765,645],[765,675],[765,705],[765,735],[765,765],[765,795],[795,645],[795,675],[795,705]],\"lines\":[],\"position\":[675,790]," +
				"\"bonuses\":[]}},\"bonuses\":[],\"tick_num\":282}";

			var jState = JObject.Parse(json);
			var state = ParseGameState(jState);
			_initialState = new GameStateInternal(state);
		}

		[Benchmark(Baseline = true)]
		public GameStateInternal FindBestState() => _bestTrajectoryFinder.FindBestState(_initialState);

		public static GameState ParseGameState(JObject jParams)
		{
			return new GameState {
				Players = jParams["players"].Cast<JProperty>().Select(x => ParsePlayer(x)).ToArray(),
				Bonuses = jParams["bonuses"].Cast<JObject>().Select(ParseBonus).ToArray(),
				TickNumber = (int)jParams["tick_num"]
			};
		}

		private static BonusInfo ParseBonus(JObject jBonus)
		{
			return new BonusInfo {
				Type = ParseBonusType((string)jBonus["type"]),
				Position = ParsePoint((JArray)jBonus["position"])
			};
		}

		private static PlayerInfo ParsePlayer(JProperty jIdentityPlayer)
		{
			string id = jIdentityPlayer.Name;
			var jPlayer = jIdentityPlayer.Value;
			return new PlayerInfo {
				Id = id,
				Score = (int)jPlayer["score"],
				Territory = jPlayer["territory"].Cast<JArray>().Select(ParsePoint).ToArray(),
				Position = ParsePoint((JArray)jPlayer["position"]),
				Lines = jPlayer["lines"].Cast<JArray>().Select(ParsePoint).ToArray(),
				Bonuses = jPlayer["bonuses"].Cast<JObject>().Select(ParseActiveBonus).ToArray(),
				Direction = ParseDirection((string)jPlayer["direction"])
			};
		}

		private static Direction? ParseDirection(string sDirection)
		{
			switch (sDirection)
			{
				case null: return null;
				case "left": return Direction.Left;
				case "right": return Direction.Right;
				case "up": return Direction.Up;
				case "down": return Direction.Down;
				default: throw new ArgumentOutOfRangeException(nameof(sDirection), sDirection, null);
			}
		}

		private static ActiveBonusInfo ParseActiveBonus(JObject jActiveBonus)
		{
			return new ActiveBonusInfo {
				Type = ParseBonusType((string)jActiveBonus["type"]),
				RemainingTicks = (int)jActiveBonus["ticks"]
			};
		}

		private static BonusType ParseBonusType(string sType)
		{
			switch (sType)
			{
				case "n": return BonusType.Nitro;
				case "s": return BonusType.Slowdown;
				case "saw": return BonusType.Saw;
				default: throw new ArgumentOutOfRangeException(nameof(sType), sType, null);
			}
		}

		private static Point ParsePoint(JArray jPointArray)
		{
			return new Point((int)jPointArray[0], (int)jPointArray[1]);
		}
	}
}
