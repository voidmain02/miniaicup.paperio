using System;
using BenchmarkDotNet.Attributes;
using MiniAiCup.Paperio.Core;

namespace MiniAiCup.Paperio.Benchmark
{
	[ClrJob, MonoJob]
	[RankColumn]
	public class BuildInsideDistanceMapBenchmark
	{
		private readonly int _size;

		private readonly Point _position;

		private readonly Direction _direction;

		private readonly int[][,] _arraysToCopy;

		public BuildInsideDistanceMapBenchmark()
		{
			_size = 31;

			_position = new Point(4, 17);

			_direction = Direction.Up;

			_arraysToCopy = new int[4][,];
			var center = new Point(_size - 1, _size - 1);
			for (int i = 0; i < 4; i++)
			{
				_arraysToCopy[i] = new int[_size*2 - 1, _size*2 - 1];
				for (int y = 0; y < _size*2 - 1; y++)
				{
					for (int x = 0; x < _size*2 - 1; x++)
					{
						_arraysToCopy[i][x, y] = GetDistanceBetweenPoints(center, new Point(x, y));
					}
				}

				switch ((Direction)i)
				{
					case Direction.Left:
						for (int x = center.X + 1; x < _size*2 - 1; x++)
						{
							_arraysToCopy[i][x, center.Y] += 2;
						}
						break;
					case Direction.Up:
						for (int y = center.Y - 1; y >= 0; y--)
						{
							_arraysToCopy[i][center.X, y] += 2;
						}
						break;
					case Direction.Right:
						for (int x = center.X - 1; x >= 0; x--)
						{
							_arraysToCopy[i][x, center.Y] += 2;
						}
						break;
					case Direction.Down:
						for (int y = center.Y + 1; y < _size*2 - 1; y++)
						{
							_arraysToCopy[i][center.X, y] += 2;
						}
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
		}

		[Benchmark]
		public int[,] BuildInsideDistanceMap()
		{
			var map = new int[_size, _size];
			for (int y = 0; y < _size; y++)
			{
				for (int x = 0; x < _size; x++)
				{
					map[x, y] = GetDistanceBetweenPoints(_position, new Point(x, y));
				}
			}

			switch (_direction)
			{
				case Direction.Left:
					for (int x = _position.X + 1; x < _size; x++)
					{
						map[x, _position.Y] += 2;
					}
					break;
				case Direction.Up:
					for (int y = _position.Y - 1; y >= 0; y--)
					{
						map[_position.X, y] += 2;
					}
					break;
				case Direction.Right:
					for (int x = _position.X - 1; x >= 0; x--)
					{
						map[x, _position.Y] += 2;
					}
					break;
				case Direction.Down:
					for (int y = _position.Y + 1; y < _size; y++)
					{
						map[_position.X, y] += 2;
					}
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			return map;
		}

		[Benchmark]
		public int[,] CopyInsideDistanceMap()
		{
			var array = new int[_size, _size];

			var arrayToCopy = _arraysToCopy[(int)_direction];

			for (int i = 0; i < _size; i++)
			{
				Buffer.BlockCopy(arrayToCopy, ((_size*2 - 1)*(_size - _position.X - 1 + i) + (_size - _position.Y - 1))*sizeof(int), array, i*_size*sizeof(int), _size*sizeof(int));
			}

			return array;
		}

		private static int GetDistanceBetweenPoints(Point src, Point dst)
		{
			return Math.Abs(src.X - dst.X) + Math.Abs(src.Y - dst.Y);
		}
	}
}
