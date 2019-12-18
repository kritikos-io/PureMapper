using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using Nessos.Expressions.Splicer;

namespace Kritikos.PureMapper
{

	public interface IPureMapperConfig
	{
		IPureMapperConfig Map<TSource, TDestination>(Func<IPureMapper, Expression<Func<TSource, TDestination>>> map);
		List<(Type Source, Type Dest, Func<IPureMapper, LambdaExpression> Expr)> Maps { get; }
	}

	public interface IPureMapper
	{
		Expression<Func<TSource, TDestination>> ResolveExpr<TSource, TDestination>();
		Func<TSource, TDestination> ResolveFunc<TSource, TDestination>();


	}

	public class PureMapperConfig : IPureMapperConfig
	{

		private readonly List<(Type Source, Type Dest, Func<IPureMapper, LambdaExpression> Map)> maps = new List<(Type Source, Type Dest, Func<IPureMapper, LambdaExpression> Map)>();
		public List<(Type Source, Type Dest, Func<IPureMapper, LambdaExpression> Expr)> Maps => maps;

		public IPureMapperConfig Map<TSource, TDestination>(Func<IPureMapper, Expression<Func<TSource, TDestination>>> map)
		{
			maps.Add((typeof(TSource), typeof(TDestination), map));

			return this;
		}

	}

	public class PureMapper : IPureMapper
	{
		private readonly Dictionary<(Type Source, Type Dest), (Lazy<Expression> Expr, Lazy<Delegate> Func)> dict = new Dictionary<(Type, Type), (Lazy<Expression> Expr, Lazy<Delegate> Func)>();
		
		public PureMapper(IPureMapperConfig cfg)
		{
			Map(cfg.Maps);
		}

		private void Map(List<(Type Source, Type Dest, Func<IPureMapper, LambdaExpression> Map)> maps)
		{
			foreach (var (source, destination, map) in maps)
			{
				var key = (source, destination);
				var splicer = new Splicer();
				
				var exprValue = new Lazy<Expression>(() => splicer.Visit(map(this)));
				var funcValue = new Lazy<Delegate>(() => ((LambdaExpression)splicer.Visit(map(this))).Compile());

				if (dict.ContainsKey(key))
					dict[key] = (exprValue, funcValue);
				else
					dict.Add(key, (exprValue, funcValue));
			}
		}

		public Expression<Func<TSource, TDestination>> ResolveExpr<TSource, TDestination>()
		{
			var key = (typeof(TSource), typeof(TDestination));
			if (!dict.ContainsKey(key))
				throw new KeyNotFoundException($"{key}");

			return (Expression<Func<TSource, TDestination>>)dict[key].Expr.Value;
		}

		public Func<TSource, TDestination> ResolveFunc<TSource, TDestination>()
		{
			var key = (typeof(TSource), typeof(TDestination));
			if (!dict.ContainsKey(key))
				throw new KeyNotFoundException($"{key}");

			return (Func<TSource, TDestination>)dict[key].Func.Value;
		}
	}
}
