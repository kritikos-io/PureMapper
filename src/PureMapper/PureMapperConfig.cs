namespace Kritikos.PureMapper
{
	using System;
	using System.Collections.Generic;
	using System.Linq.Expressions;

	using Kritikos.PureMapper.Contracts;

	public class PureMapperConfig : IPureMapperConfig
	{
		private readonly List<(Type Source, Type Dest, Func<IPureMapper, LambdaExpression> Map)> maps =
			new List<(Type Source, Type Dest, Func<IPureMapper, LambdaExpression> Map)>();

		public List<(Type Source, Type Dest, Func<IPureMapper, LambdaExpression> Expr)> Maps => maps;

		public IPureMapperConfig Map<TSource, TDestination>(
			Func<IPureMapper, Expression<Func<TSource, TDestination>>> map)
		{
			maps.Add((typeof(TSource), typeof(TDestination), map));

			return this;
		}
	}
}
