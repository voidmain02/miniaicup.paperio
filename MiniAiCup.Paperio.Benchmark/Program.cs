using BenchmarkDotNet.Running;

namespace MiniAiCup.Paperio.Benchmark
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var summary = BenchmarkRunner.Run<BuildOutsideDistanceMapBenchmark>();
		}
	}
}
