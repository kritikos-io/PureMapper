namespace Kritikos.PureMap.Contracts
{
	using System;
	using System.Linq.Expressions;

	public interface IPureMapper
	{
		TDestination Map<TSource, TDestination>(TSource source)
			where TSource : class
			where TDestination : class;

		Expression<Func<TSource, TDestination>> Map<TSource, TDestination>()
			where TSource : class
			where TDestination : class;
	}
}
