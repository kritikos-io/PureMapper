namespace Kritikos.PureMapper.Contracts
{
	using System;
	using System.Linq.Expressions;

	public interface IPureMapperResolver
	{
		Expression<Func<TSource, TDestination>> Resolve<TSource, TDestination>()
			where TSource : class
			where TDestination : class;
	}
}
