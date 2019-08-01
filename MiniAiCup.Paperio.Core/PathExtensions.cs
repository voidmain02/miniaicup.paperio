using System.Linq;

namespace MiniAiCup.Paperio.Core
{
	public static class PathExtensions
	{
		public static Path Append(this Path path, Point point)
		{
			if (path.AsPointsSet().Contains(point))
			{
				return path;
			}

			var pointsList = path.ToList();
			pointsList.Add(point);
			return new Path(pointsList);
		}

		public static Path RemoveFromEnd(this Path path, int count)
		{
			return path.Length <= count
				? Path.Empty
				: new Path(path.Take(path.Length - count));
		}

		public static Path RemoveFromStart(this Path path, int count)
		{
			return path.Length <= count
				? Path.Empty
				: new Path(path.Skip(count));
		}
	}
}
