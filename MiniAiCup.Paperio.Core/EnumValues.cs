using System;

namespace MiniAiCup.Paperio.Core
{
	public static class EnumValues
	{
		public static T[] GetAll<T>() where T : struct, IConvertible
		{
			return AllEnumValuesArray<T>.Instance;
		}

		private static class AllEnumValuesArray<T>
		{
			private static readonly Lazy<T[]> AllValuesArrayLazy = new Lazy<T[]>(() => (T[])Enum.GetValues(typeof(T)));

			public static T[] Instance => AllValuesArrayLazy.Value;
		}
	}
}
