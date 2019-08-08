using System;

namespace MiniAiCup.Paperio.Core
{
	public static class Utils
	{
		public static void FastCopyArray(int[,] src, int[,] dst, int length)
		{
			Buffer.BlockCopy(src, 0, dst, 0, length*sizeof(int));
		}

		public static void FastCopyArray(bool[,] src, bool[,] dst, int length)
		{
			Buffer.BlockCopy(src, 0, dst, 0, length*sizeof(bool));
		}
	}
}
