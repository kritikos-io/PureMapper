namespace Kritikos.PureMapper.Contracts
{
	using System;
	using System.Collections.Generic;
	using System.Linq.Expressions;

	public interface IPureMapperConfig
	{
		List<(Type Source, Type Dest, Func<IPureMapperResolver, LambdaExpression> Expr)> Maps { get; }

		IPureMapperConfig Map<TSource, TDestination>(Func<IPureMapperResolver, Expression<Func<TSource, TDestination>>> map) where TSource : class
																															 where TDestination : class;
	}
}
