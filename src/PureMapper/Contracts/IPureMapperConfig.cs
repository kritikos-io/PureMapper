namespace Kritikos.PureMap.Contracts
{
	using System;
	using System.Collections.Generic;
	using System.Linq.Expressions;

	/// <summary>
	/// Configuration of PureMapper behavior and collection of mapping registrations.
	/// </summary>
	public interface IPureMapperConfig
	{
		/// <summary>
		/// List of registered mappings.
		/// </summary>
		/// <remarks>
		/// Handle with care, access directly only if needed. Prefer usage via <see cref="IPureMapper"/>.
		/// </remarks>
		List<(Type Source, Type Dest, string name, Func<IPureMapperResolver, LambdaExpression> Expr, int RecInlineDepth)
		> Maps { get; }

		/// <summary>
		/// List of registered value updating maps.
		/// </summary>
		/// <remarks>
		/// Handle with care, access directly only if needed. Prefer usage via <see cref="IPureMapper"/>.
		/// </remarks>
		List<(Type Source, Type Dest, string name, Func<IPureMapperUpdateResolver, LambdaExpression> Expr, int
			reclineDepth)> UpdateMaps { get; }

		/// <summary>
		/// Registers a new mapping.
		/// </summary>
		/// <typeparam name="TSource"><see cref="Type"/> to map from.</typeparam>
		/// <typeparam name="TDestination"><see cref="Type"/> to map to.</typeparam>
		/// <param name="map"><see cref="IPureMapperResolver"/> to provide resolving of complex types via already registered maps.</param>
		/// <param name="recInlineDepth">Depth until which recursive objects will be unrolled to, defaults to no unrolling.</param>
		/// <param name="name">Map name or set as default (if empty).</param>
		/// <returns><see langword="this"/> <see cref="IPureMapperConfig"/>.</returns>
		/// <exception cref="ArgumentException"><paramref name="recInlineDepth"/> is not equal or greater than zero.</exception>
		IPureMapperConfig Map<TSource, TDestination>(
			Func<IPureMapperResolver, Expression<Func<TSource, TDestination>>> map,
			int recInlineDepth = 0,
			string name = "")
			where TSource : class
			where TDestination : class;

		/// <summary>
		/// Updates values of an existing object from a specified source.
		/// </summary>
		/// <typeparam name="TSource"><see cref="Type"/> to map from.</typeparam>
		/// <typeparam name="TDestination"><see cref="Type"/> to map to.</typeparam>
		/// <param name="map"><see cref="IPureMapperUpdateResolver"/> to provide resolving of complex types via already registered maps.</param>
		/// <param name="recInlineDepth">Depth until which recursive objects will be unrolled to, defaults to no unrolling.</param>
		/// <param name="name">Map name or set as default (if empty).</param>
		/// <returns><see langword="this"/> <see cref="IPureMapperConfig"/>.</returns>
		/// <exception cref="ArgumentException"><paramref name="recInlineDepth"/> is not equal or greater than zero.</exception>
		IPureMapperConfig Map<TSource, TDestination>(
			Func<IPureMapperUpdateResolver, Expression<Func<TSource, TDestination, TDestination>>> map,
			int recInlineDepth = 0,
			string name = "")
			where TSource : class
			where TDestination : class;
	}
}
