namespace Kritikos.PureMap.Contracts
{
	using System;
	using System.Linq.Expressions;

	public interface IPureMapperUpdateResolver
	{
		Expression<Func<TSource, TDestination, TDestination>> Resolve<TSource, TDestination>()
			where TSource : class
			where TDestination : class;

		Expression<Func<TSource, TDestination, TDestination>> Resolve<TSource, TDestination>(string name)
			where TSource : class
			where TDestination : class;
	}
}
