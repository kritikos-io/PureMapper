namespace Kritikos.PureMap
{
	using System;
	using System.Linq;

	using Kritikos.PureMap.Contracts;

	public static class Queryable
	{
		public static IQueryable<TDestination> Project<TSource, TDestination>(
			this IQueryable<TSource> source,
			IPureMapper mapper,
			string mapName = "")
			where TSource : class
			where TDestination : class
			=> source.Select(mapper?.Map<TSource, TDestination>(mapName)
							?? throw new ArgumentNullException(nameof(mapper)));
	}
}
