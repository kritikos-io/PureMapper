#nullable disable
namespace Kritikos.PureMapper
{
	using System;
	using System.Collections.Generic;
	using System.Linq.Expressions;

	using Kritikos.PureMapper.Contracts;

	using Nessos.Expressions.Splicer;

	public class PureMapper : IPureMapperResolver, IPureMapper
	{
		private readonly Dictionary<(Type Source, Type Dest), int> visited =
			new Dictionary<(Type Source, Type Dest), int>();

		private readonly Dictionary<(Type Source, Type Dest), MapValue> dict = new Dictionary<(Type, Type), MapValue>();

		public PureMapper(IPureMapperConfig cfg)
			=> Map(cfg?.Maps ?? throw new ArgumentNullException(nameof(cfg)));

		public TDestination Map<TSource, TDestination>(TSource source)
			where TSource : class
			where TDestination : class
		{
			var key = (typeof(TSource), typeof(TDestination));
			if (!dict.ContainsKey(key))
			{
				throw new KeyNotFoundException($"{key}");
			}

			return ((Func<TSource, TDestination>)dict[key].SplicedFunc).Invoke(source);
		}

		public Expression<Func<TSource, TDestination>> ResolveExpr<TSource, TDestination>()
			where TSource : class
			where TDestination : class
		{
			var key = (typeof(TSource), typeof(TDestination));
			if (!dict.ContainsKey(key))
			{
				throw new KeyNotFoundException($"{key}");
			}

			var mapValue = dict[key];
			mapValue.Rec = (Expression<Func<TSource, TDestination>>)(x => null);
			return Resolve<TSource, TDestination>();
		}

		public Expression<Func<TSource, TDestination>> ResolveFunc<TSource, TDestination>()
			where TSource : class
			where TDestination : class
		{
			var key = (typeof(TSource), typeof(TDestination));
			if (!dict.ContainsKey(key))
			{
				throw new KeyNotFoundException($"{key}");
			}

			var mapValue = dict[key];
			mapValue.Rec =
				(Expression<Func<TSource, TDestination>>)(x => ((Func<TSource, TDestination>)mapValue.SplicedFunc)(x));
			return Resolve<TSource, TDestination>();
		}

		public Expression<Func<TSource, TDestination>> Resolve<TSource, TDestination>()
			where TSource : class
			where TDestination : class
		{
			var key = (typeof(TSource), typeof(TDestination));
			if (!dict.ContainsKey(key))
			{
				throw new KeyNotFoundException($"{key}");
			}

			var mapValue = dict[key];

			if (!visited.ContainsKey(key))
			{
				visited.Add(key, 0);
			}

			if (visited[key] > mapValue.RecInlineDepth)
			{
				return (Expression<Func<TSource, TDestination>>)mapValue.Rec;
			}

			visited[key]++;

			var splicer = new Splicer();
			var splicedExpr = (LambdaExpression)splicer.Visit(mapValue.OriginalExpr);

			return (Expression<Func<TSource, TDestination>>)splicedExpr;
		}

		private void Map(List<(Type Source, Type Dest, Func<IPureMapperResolver, LambdaExpression> Map, int RecInlineDepth)> maps)
		{
			foreach (var (source, destination, map, recInlineDepth) in maps)
			{
				var key = (source, destination);
				var splicer = new Splicer();

				var mapValue = new MapValue { OriginalExpr = map(this), RecInlineDepth = recInlineDepth };

				if (dict.ContainsKey(key))
				{
					dict[key] = mapValue;
				}
				else
				{
					dict.Add(key, mapValue);
				}
			}

			// force resolve
			foreach (var keyValue in dict)
			{
				var key = keyValue.Key;
				var mapValue = keyValue.Value;

				var resolve = typeof(PureMapper).GetMethod("ResolveExpr");
				var _resolve = resolve.MakeGenericMethod(keyValue.Key.Source, keyValue.Key.Dest);
				var lambdaExpression = (LambdaExpression)_resolve.Invoke(this, Array.Empty<object>());
				mapValue.SplicedExpr = lambdaExpression;
				visited.Clear();

				resolve = typeof(PureMapper).GetMethod("ResolveFunc");
				_resolve = resolve.MakeGenericMethod(keyValue.Key.Source, keyValue.Key.Dest);
				lambdaExpression = (LambdaExpression)_resolve.Invoke(this, Array.Empty<object>());
				mapValue.SplicedFunc = lambdaExpression.Compile();
				visited.Clear();
			}
		}

		private class MapValue
		{
			public LambdaExpression OriginalExpr { get; set; }

			public LambdaExpression SplicedExpr { get; set; }

			public Delegate SplicedFunc { get; set; }

			public LambdaExpression Rec { get; set; }

			public int RecInlineDepth { get; set; }
		}
	}
}
