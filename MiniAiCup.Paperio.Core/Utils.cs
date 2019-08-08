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

		public static void CopyArrayPart(int[,] src, int srcWidth, int srcHeight, int[,] dst, int dstWidth, int dstHeight, int offsetX, int offsetY)
		{
			for (int i = 0; i < dstWidth; i++)
			{
				Buffer.BlockCopy(src, (srcHeight*(i + offsetX) + offsetY)*sizeof(int),
					dst, dstHeight*i*sizeof(int), dstHeight*sizeof(int));
			}
		}
	}
}
