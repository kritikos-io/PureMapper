namespace Kritikos.PureMapper.Contracts
{
	using System;
	using System.Collections.Generic;
	using System.Linq.Expressions;

	public interface IPureMapperConfig
	{
		List<(Type Source, Type Dest, Func<IPureMapper, LambdaExpression> Expr)> Maps { get; }

		IPureMapperConfig Map<TSource, TDestination>(Func<IPureMapper, Expression<Func<TSource, TDestination>>> map);
	}
}
