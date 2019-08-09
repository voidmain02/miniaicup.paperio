using System;
using BenchmarkDotNet.Attributes;
using MiniAiCup.Paperio.Core;

namespace MiniAiCup.Paperio.Benchmark
{
	[ClrJob, MonoJob]
	[RankColumn]
	public class FillDistanceArrayBenchmark
	{
		private Size _mapSize;

		private Point _position;
		private Point2 _position2;

		private Direction _direction;
		
		[GlobalSetup]
		public void Setup()
		{
			_mapSize = new Size(31, 31);
			_position = new Point(12, 27);
			_position2 = new Point2(12, 27);
			_direction = Direction.Left;
		}

		[Benchmark(Baseline = true)]
		public int[,] BuildInsideDistanceMap()
		{
			var map = new int[_mapSize.Width, _mapSize.Height];
			for (int y = 0; y < _mapSize.Height; y++)
			{
				for (int x = 0; x < _mapSize.Width; x++)
				{
					map[x, y] = GetDistanceBetweenPoints(_position, new Point(x, y), _direction);
				}
			}

			return map;
		}

		[Benchmark]
		public int[,] BuildInsideDistanceMapWithTwoSteps()
		{
			var map = new int[_mapSize.Width, _mapSize.Height];
			for (int y = 0; y < _mapSize.Height; y++)
			{
				for (int x = 0; x < _mapSize.Width; x++)
				{
					map[x, y] = GetDistanceBetweenPoints(_position, new Point(x, y));
				}
			}

			switch (_direction)
			{
				case Direction.Left:
					for (int x = _position.X + 1; x < _mapSize.Width; x++)
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
					for (int y = _position.X + 1; y < _mapSize.Height; y++)
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
		public int[,] BuildInsideDistanceMapWithTwoStepsFields()
		{
			var map = new int[_mapSize.Width, _mapSize.Height];
			for (int y = 0; y < _mapSize.Height; y++)
			{
				for (int x = 0; x < _mapSize.Width; x++)
				{
					map[x, y] = GetDistanceBetweenPoints(_position2, new Point2(x, y));
				}
			}

			switch (_direction)
			{
				case Direction.Left:
					for (int x = _position.X + 1; x < _mapSize.Width; x++)
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
					for (int y = _position.X + 1; y < _mapSize.Height; y++)
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
		public int[,] BuildInsideDistanceMapWithTwoStepsAndNoClasses()
		{
			var map = new int[_mapSize.Width, _mapSize.Height];
			for (int y = 0; y < _mapSize.Height; y++)
			{
				for (int x = 0; x < _mapSize.Width; x++)
				{
					map[x, y] = GetDistanceBetweenPoints(_position.X, _position.Y, x, y);
				}
			}

			switch (_direction)
			{
				case Direction.Left:
					for (int x = _position.X + 1; x < _mapSize.Width; x++)
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
					for (int y = _position.X + 1; y < _mapSize.Height; y++)
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
		public int[,] BuildInsideDistanceMapWithTwoStepsAndTuples()
		{
			var map = new int[_mapSize.Width, _mapSize.Height];
			for (int y = 0; y < _mapSize.Height; y++)
			{
				for (int x = 0; x < _mapSize.Width; x++)
				{
					map[x, y] = GetDistanceBetweenPoints((_position.X, _position.Y), (x, y));
				}
			}

			switch (_direction)
			{
				case Direction.Left:
					for (int x = _position.X + 1; x < _mapSize.Width; x++)
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
					for (int y = _position.X + 1; y < _mapSize.Height; y++)
					{
						map[_position.X, y] += 2;
					}
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			return map;
		}

		private static int GetDistanceBetweenPoints(int srcX, int srcY, int dstX, int dstY)
		{
			return Math.Abs(srcX - dstX) + Math.Abs(srcY - dstY);
		}

		private static int GetDistanceBetweenPoints((int X, int Y) src, (int X, int Y) dst)
		{
			return Math.Abs(src.X - dst.X) + Math.Abs(src.Y - dst.Y);
		}

		private static int GetDistanceBetweenPoints(Point src, Point dst)
		{
			return Math.Abs(src.X - dst.X) + Math.Abs(src.Y - dst.Y);
		}

		private static int GetDistanceBetweenPoints(Point2 src, Point2 dst)
		{
			return Math.Abs(src.X - dst.X) + Math.Abs(src.Y - dst.Y);
		}

		private static int GetDistanceBetweenPoints(Point src, Point dst, Direction? direction)
		{
			int distance = GetDistanceBetweenPoints(src, dst);

			if (dst.X == src.X && (dst.Y > src.Y && direction == Direction.Down || dst.Y < src.Y && direction == Direction.Up) ||
				dst.Y == src.Y && (dst.X > src.X && direction == Direction.Left || dst.X < src.X && direction == Direction.Right))
			{
				distance += 2;
			}

			return distance;
		}
	}

	[Serializable]
	public struct Point2 : IEquatable<Point>
	{
		/// <summary>
		/// X-координата
		/// </summary>
		public int X;

		/// <summary>
		/// Y-координата
		/// </summary>
		public int Y;

		public Point2(int x, int y)
		{
			X = x;
			Y = y;
		}

		public bool Equals(Point other)
		{
			return X == other.X && Y == other.Y;
		}

		public override bool Equals(object obj)
		{
			return obj is Point other && Equals(other);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return X*397 ^ Y;
			}
		}

		public static bool operator ==(Point2 left, Point2 right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(Point2 left, Point2 right)
		{
			return !left.Equals(right);
		}

		public override string ToString()
		{
			return $"[{X}, {Y}]";
		}
	}
}
