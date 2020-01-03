namespace Kritikos.PureMap
{
	using System;
	using System.Linq;

	using Kritikos.PureMap.Contracts;

	public static class Querable
	{
		public static IQueryable<TDestination> Project<TSource, TDestination>(
			this IQueryable<TSource> source,
			IPureMapper mapper)
			where TSource : class
			where TDestination : class
			=> source.Select(mapper?.Map<TSource, TDestination>()
							?? throw new ArgumentNullException(nameof(mapper)));
	}
}
