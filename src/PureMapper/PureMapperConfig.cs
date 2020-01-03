#nullable disable
namespace Kritikos.PureMap
{
	using System;
	using System.Collections.Generic;
	using System.Linq.Expressions;

	using Kritikos.PureMap.Contracts;

	using Nessos.Expressions.Splicer;

	public class PureMapperConfig : IPureMapperConfig
	{
		public List<(Type Source, Type Dest, Func<IPureMapperResolver, LambdaExpression> Expr, int RecInlineDepth)> Maps { get; }
			= new List<(Type Source, Type Dest, Func<IPureMapperResolver, LambdaExpression> Map, int RecInlineDepth)>();

		public IPureMapperConfig Map<TSource, TDestination>(
			Func<IPureMapperResolver, Expression<Func<TSource, TDestination>>> map, int recInlineDepth = 0)
			where TSource : class
			where TDestination : class
		{
			if (recInlineDepth < 0)
			{
				throw new ArgumentException($"{nameof(recInlineDepth)} should be >= 0");
			}

#pragma warning disable IDE0039 // Use local function
			Func<IPureMapperResolver, Expression<Func<TSource, TDestination>>> f = resolver
#pragma warning restore IDE0039 // Use local function
				=> x => (x == null)
					? null
					: map(resolver).Invoke(x);
			Maps.Add((typeof(TSource), typeof(TDestination), f, recInlineDepth));

			return this;
		}
	}
}
