#nullable disable
namespace Kritikos.PureMap
{
	using System;
	using System.Collections.Generic;
	using System.Linq.Expressions;
	using System.Reflection;

	using Kritikos.PureMap.Contracts;

	using Nessos.Expressions.Splicer;

	public class PureMapper : IPureMapperUpdateResolver, IPureMapperResolver, IPureMapper
	{
		private readonly Dictionary<(Type Source, Type Dest, string Name), int> visitedMappings =
			new Dictionary<(Type Source, Type Dest, string Name), int>();

		private readonly Dictionary<(Type Source, Type Dest, string Name), MapValue> mappings
			= new Dictionary<(Type Source, Type Dest, string Name), MapValue>();

		private readonly Dictionary<(Type Source, Type Dest, string Name), int> visitedUpdateMappings =
			new Dictionary<(Type Source, Type Dest, string Name), int>();

		private readonly Dictionary<(Type Source, Type Dest, string Name), UpdateValue> updateMappings
			= new Dictionary<(Type Source, Type Dest, string Name), UpdateValue>();

		public PureMapper(IPureMapperConfig cfg)
		{
			if (cfg == null)
			{
				throw new ArgumentNullException(nameof(cfg));
			}

			RegisterMappings(cfg.Maps);
			RegisterUpdateMappings(cfg.UpdateMaps);
		}

		/// <inheritdoc />
		public TDestination Map<TSource, TDestination>(TSource source, string name = "")
			where TSource : class
			where TDestination : class
		{
			var key = (typeof(TSource), typeof(TDestination), name);
			if (!mappings.ContainsKey(key))
			{
				throw new KeyNotFoundException($"{key}");
			}

			return ((Func<TSource, TDestination>)mappings[key].SplicedFunc).Invoke(source);
		}

		/// <inheritdoc />
		public TDestination Map<TSource, TDestination>(TSource source, TDestination destination, string name = "")
		{
			var key = (typeof(TSource), typeof(TDestination), name);
			if (!updateMappings.ContainsKey(key))
			{
				throw new KeyNotFoundException($"{key}");
			}

			return ((Func<TSource, TDestination, TDestination>)updateMappings[key].SplicedFunc).Invoke(
				source,
				destination);
		}

		/// <inheritdoc />
		public Expression<Func<TSource, TDestination>> Map<TSource, TDestination>(string name = "")
			where TSource : class
			where TDestination : class
		{
			var key = (typeof(TSource), typeof(TDestination), name);
			if (!mappings.ContainsKey(key))
			{
				throw new KeyNotFoundException($"{key}");
			}

			return (Expression<Func<TSource, TDestination>>)mappings[key].SplicedExpr;
		}

		/// <inheritdoc />
		Expression<Func<TSource, TDestination>> IPureMapperResolver.Resolve<TSource, TDestination>()
			where TSource : class
			where TDestination : class
			=> ((IPureMapperResolver)this).Resolve<TSource, TDestination>(string.Empty);

		/// <inheritdoc />
		Expression<Func<TSource, TDestination>> IPureMapperResolver.Resolve<TSource, TDestination>(string name)
			where TSource : class
			where TDestination : class
		{
			var key = (typeof(TSource), typeof(TDestination), name);
			if (!mappings.ContainsKey(key))
			{
				throw new KeyNotFoundException($"{key}");
			}

			var mapValue = mappings[key];

			if (!visitedMappings.ContainsKey(key))
			{
				visitedMappings.Add(key, 0);
			}

			if (visitedMappings[key] > mapValue.RecInlineDepth)
			{
				return (Expression<Func<TSource, TDestination>>)mapValue.Rec;
			}

			visitedMappings[key]++;

			var splicer = new Splicer();
			var splicedExpr = (LambdaExpression)splicer.Visit(mapValue.OriginalExpr);

			return (Expression<Func<TSource, TDestination>>)splicedExpr;
		}

		/// <inheritdoc />
		Expression<Func<TSource, TDestination, TDestination>> IPureMapperUpdateResolver.
			Resolve<TSource, TDestination>()
			=> ((IPureMapperUpdateResolver)this).Resolve<TSource, TDestination>(string.Empty);

		/// <inheritdoc />
		Expression<Func<TSource, TDestination, TDestination>> IPureMapperUpdateResolver.Resolve<TSource, TDestination>(
			string name)
		{
			var key = (typeof(TSource), typeof(TDestination), name);
			if (!updateMappings.ContainsKey(key))
			{
				throw new KeyNotFoundException($"{key}");
			}

			var updateValue = updateMappings[key];

			if (!visitedUpdateMappings.ContainsKey(key))
			{
				visitedUpdateMappings.Add(key, 0);
			}

			if (visitedUpdateMappings[key] > updateValue.RecInlineDepth)
			{
				return (Expression<Func<TSource, TDestination, TDestination>>)updateValue.Rec;
			}

			visitedUpdateMappings[key]++;

			var splicer = new Splicer();
			var splicedExpr = (LambdaExpression)splicer.Visit(updateValue.OriginalExpr);

			return (Expression<Func<TSource, TDestination, TDestination>>)splicedExpr;
		}

#pragma warning disable IDE0051 // Used by reflection
		private Expression<Func<TSource, TDestination>> ResolveExpr<TSource, TDestination>(string name = "")
#pragma warning restore IDE0051 // Remove unused private members
			where TSource : class
			where TDestination : class
		{
			var key = (typeof(TSource), typeof(TDestination), name);
			if (!mappings.ContainsKey(key))
			{
				throw new KeyNotFoundException($"{key}");
			}

			var mapValue = mappings[key];
			mapValue.Rec = (Expression<Func<TSource, TDestination>>)(x => null);
			return ((IPureMapperResolver)this).Resolve<TSource, TDestination>(name);
		}

#pragma warning disable IDE0051 // Used by reflection
		private Expression<Func<TSource, TDestination>> ResolveFunc<TSource, TDestination>(string name = "")
			where TSource : class
			where TDestination : class
		{
			var key = (typeof(TSource), typeof(TDestination), name);
			if (!mappings.ContainsKey(key))
			{
				throw new KeyNotFoundException($"{key}");
			}

			var mapValue = mappings[key];
			mapValue.Rec =
				(Expression<Func<TSource, TDestination>>)(x => ((Func<TSource, TDestination>)mapValue.SplicedFunc)(x));
			return ((IPureMapperResolver)this).Resolve<TSource, TDestination>(name);
		}

#pragma warning disable IDE0051 // Used by reflection
		private Expression<Func<TSource, TDestination, TDestination>> ResolveUpdateFunc<TSource, TDestination>(
			string name = "")
			where TSource : class
			where TDestination : class
		{
			var key = (typeof(TSource), typeof(TDestination), name);
			if (!updateMappings.ContainsKey(key))
			{
				throw new KeyNotFoundException($"{key}");
			}

			var updateValue = updateMappings[key];
			updateValue.Rec = (Expression<Func<TSource, TDestination, TDestination>>)((x, y) =>
				((Func<TSource, TDestination, TDestination>)updateValue.SplicedFunc)(x, y));

			return ((IPureMapperUpdateResolver)this).Resolve<TSource, TDestination>(name);
		}

		private void RegisterMappings(
			List<(Type Source, Type Dest, string Name, Func<IPureMapperResolver, LambdaExpression> Map, int
				RecInlineDepth)> maps)
		{
			foreach (var (source, destination, name, map, recInlineDepth) in maps)
			{
				var key = (source, destination, name);

				var mapValue = new MapValue { OriginalExpr = map(this), RecInlineDepth = recInlineDepth };

				if (mappings.ContainsKey(key))
				{
					mappings[key] = mapValue;
				}
				else
				{
					mappings.Add(key, mapValue);
				}
			}

			// force resolve
			foreach (var keyValue in mappings)
			{
				var (src, dest, name) = keyValue.Key;
				var mapValue = keyValue.Value;

				var resolve = this.GetType().GetTypeInfo().GetDeclaredMethod("ResolveExpr");
				var resolvedGeneric = resolve.MakeGenericMethod(src, dest);
				var lambdaExpression = (LambdaExpression)resolvedGeneric.Invoke(this, new object[] { name });
				mapValue.SplicedExpr = lambdaExpression;
				visitedMappings.Clear();

				resolve = this.GetType().GetTypeInfo().GetDeclaredMethod("ResolveFunc");
				resolvedGeneric = resolve.MakeGenericMethod(src, dest);
				lambdaExpression = (LambdaExpression)resolvedGeneric.Invoke(this, new object[] { name });
				mapValue.SplicedFunc = lambdaExpression.Compile();
				visitedMappings.Clear();
			}
		}

		private void RegisterUpdateMappings(
			List<(Type Source, Type Dest, string name, Func<IPureMapperUpdateResolver, LambdaExpression> Expr, int
				reclineDepth)> maps)
		{
			foreach (var (source, destination, name, map, recInlineDepth) in maps)
			{
				var key = (source, destination, name);

				var updateValue = new UpdateValue { OriginalExpr = map(this), RecInlineDepth = recInlineDepth };

				if (updateMappings.ContainsKey(key))
				{
					updateMappings[key] = updateValue;
				}
				else
				{
					updateMappings.Add(key, updateValue);
				}
			}

			// force resolve
			foreach (var keyValue in updateMappings)
			{
				var (src, dest, name) = keyValue.Key;
				var updateValue = keyValue.Value;

				var resolve = this.GetType().GetTypeInfo().GetDeclaredMethod("ResolveUpdateFunc");
				var resolvedGeneric = resolve.MakeGenericMethod(src, dest);
				var lambdaExpression = (LambdaExpression)resolvedGeneric.Invoke(this, new object[] { name });
				updateValue.SplicedFunc = lambdaExpression.Compile();
				visitedMappings.Clear();
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

		private class UpdateValue
		{
			public LambdaExpression OriginalExpr { get; set; }

			public Delegate SplicedFunc { get; set; }

			public LambdaExpression Rec { get; set; }

			public int RecInlineDepth { get; set; }
		}
	}
}
