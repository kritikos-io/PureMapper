namespace Kritikos.PureMapper.Contracts
{
	using System;
	using System.Linq.Expressions;

	public interface IPureMapper
	{
		public TDestination Map<TSource, TDestination>(TSource source) where TSource : class
																	   where TDestination : class;
	}

	public interface IPureMapperResolver
	{
		Expression<Func<TSource, TDestination>> Resolve<TSource, TDestination>() where TSource : class
																				 where TDestination : class;
	}
}
