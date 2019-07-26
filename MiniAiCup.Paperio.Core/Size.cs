using System;

namespace MiniAiCup.Paperio.Core
{
	/// <summary>
	/// Размер
	/// </summary>
	[Serializable]
	public struct Size : IEquatable<Size>
	{
		/// <summary>
		/// Ширина
		/// </summary>
		public int Width { get; }

		/// <summary>
		/// Высота
		/// </summary>
		public int Height { get; }

		public Size(int width, int height)
		{
			Width = width;
			Height = height;
		}

		public bool Equals(Size other)
		{
			return Width == other.Width && Height == other.Height;
		}

		public override bool Equals(object obj)
		{
			return obj is Size other && Equals(other);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return Width*397 ^ Height;
			}
		}

		public static bool operator ==(Size left, Size right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(Size left, Size right)
		{
			return !left.Equals(right);
		}
	}
}
