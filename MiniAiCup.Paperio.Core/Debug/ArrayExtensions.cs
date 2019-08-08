using System.Text;

namespace MiniAiCup.Paperio.Core.Debug
{
	public static class ArrayExtensions
	{
		public static string ArrayToString<T>(this T[,] array)
		{
			var sb = new StringBuilder();
			for (int i = 0; i < array.GetLength(1); i++)
			{
				for (int j = 0; j < array.GetLength(0); j++)
				{
					sb.Append($"{array[j, i]}\t");
				}

				sb.Append("\n");
			}

			return sb.ToString();
		}
	}
}
