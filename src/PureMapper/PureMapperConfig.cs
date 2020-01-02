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
		private readonly List<(Type Source, Type Dest, Func<IPureMapperResolver, LambdaExpression> Map, int RecInlineDepth)> maps =
			new List<(Type Source, Type Dest, Func<IPureMapperResolver, LambdaExpression> Map, int RecInlineDepth)>();

		public List<(Type Source, Type Dest, Func<IPureMapperResolver, LambdaExpression> Expr, int RecInlineDepth)> Maps => maps;

		public IPureMapperConfig Map<TSource, TDestination>(
			Func<IPureMapperResolver, Expression<Func<TSource, TDestination>>> map, int recInlineDepth = 0)
			where TSource : class
			where TDestination : class
		{
			if (recInlineDepth < 0)
				throw new ArgumentException($"{nameof(recInlineDepth)} should be >= 0");

			Func<IPureMapperResolver, Expression<Func<TSource, TDestination>>> f = resolver
				=> x => (x == null)
					? null
					: map(resolver).Invoke(x);
			maps.Add((typeof(TSource), typeof(TDestination), f, recInlineDepth));

			return this;
		}
	}
}
