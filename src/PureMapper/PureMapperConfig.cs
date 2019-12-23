#nullable disable
namespace Kritikos.PureMapper
{
	using System;
	using System.Collections.Generic;
	using System.Linq.Expressions;

	using Kritikos.PureMapper.Contracts;

	using Nessos.Expressions.Splicer;

	public class PureMapperConfig : IPureMapperConfig
	{
		private readonly List<(Type Source, Type Dest, Func<IPureMapperResolver, LambdaExpression> Map)> maps =
			new List<(Type Source, Type Dest, Func<IPureMapperResolver, LambdaExpression> Map)>();

		public List<(Type Source, Type Dest, Func<IPureMapperResolver, LambdaExpression> Expr)> Maps => maps;

		public IPureMapperConfig Map<TSource, TDestination>(
			Func<IPureMapperResolver, Expression<Func<TSource, TDestination>>> map)
			where TSource : class
			where TDestination : class
		{
			Func<IPureMapperResolver, Expression<Func<TSource, TDestination>>> f = resolver
				=> x => (x == null)
					? null
					: map(resolver).Invoke(x);
			maps.Add((typeof(TSource), typeof(TDestination), f));

			return this;
		}
	}
}
