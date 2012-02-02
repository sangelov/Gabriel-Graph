using System;
using System.Collections.Generic;
using System.Linq;

namespace Gabriel_Graph
{
	public static class IEnumerableExtensions
	{
		public static IEnumerable<T> Quicksort<T>(this IEnumerable<T> source) where T : IComparable<T>
		{
			if (!source.Any())
			{
				return source;
			}
			var pivot = source.First();
			var remaining = source.Skip(1);
			return remaining.Where(a => a.CompareTo(pivot) <= 0)
							.Quicksort()
							.Concat(new[] { pivot })
							.Concat(remaining.Where(a => a.CompareTo(pivot) > 0).Quicksort());
		}
	}
}