using System;
using BenchmarkDotNet.Attributes;

namespace MiniAiCup.Paperio.Benchmark
{
	[CoreJob]
	[RankColumn]
	public class FillArrayBenchmark
	{
		private const int Value = 42;

		private readonly int _size;

		private readonly int[,] _arrayToCopy;

		public FillArrayBenchmark()
		{
			_size = 31;

			_arrayToCopy = new int[_size, _size];
			for (int i = 0; i < _size; i++)
			{
				for (int j = 0; j < _size; j++)
				{
					_arrayToCopy[i, j] = Value;
				}
			}
		}

		[Benchmark(Baseline = true)]
		public int[,] ForLoop()
		{
			var array = new int[_size, _size];
			for (int i = 0; i < _size; i++)
			{
				for (int j = 0; j < _size; j++)
				{
					array[i, j] = Value;
				}
			}

			return array;
		}

		[Benchmark]
		public int[,] CopyArray()
		{
			var array = new int[_size, _size];
			Array.Copy(_arrayToCopy, array, _size*_size);
			return array;
		}

		[Benchmark]
		public int[,] BufferCopyArray()
		{
			var array = new int[_size, _size];
			Buffer.BlockCopy(_arrayToCopy, 0, array, 0, _size*_size*sizeof(int));
			return array;
		}
	}
}
