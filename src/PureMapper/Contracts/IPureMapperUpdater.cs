namespace Kritikos.PureMap.Contracts
{
	using System;
	using System.Linq.Expressions;

	public interface IPureMapperUpdater
	{
		Expression<Func<TSource, TDestination, TDestination>> Resolve<TSource, TDestination>(
			TSource source,
			TDestination destination)
			where TSource : class
			where TDestination : class;

		Expression<Func<TSource, TDestination, TDestination>> Resolve<TSource, TDestination>(
			TSource source,
			TDestination destination,
			string name)
			where TSource : class
			where TDestination : class;
	}
}
