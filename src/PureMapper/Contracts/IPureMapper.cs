namespace Kritikos.PureMap.Contracts
{
	using System;
	using System.Linq.Expressions;

	public interface IPureMapper
	{
		/// <summary>
		/// Creates a new instance of <typeparamref name="TDestination"/> type and populates it with values from <paramref name="source"/> using named map <paramref name="name"/>.
		/// </summary>
		/// <typeparam name="TSource">Type of <paramref name="source"/> to take values from.</typeparam>
		/// <typeparam name="TDestination">Type of object that will be created.</typeparam>
		/// <param name="source">Object to take values from.</param>
		/// <param name="name">Name of the map to use (or default map if empty).</param>
		/// <returns>An instance of <typeparamref name="TDestination"/> with values populated from <paramref name="source"/> as dictated by map <paramref name="name"/>.</returns>
		/// <exception cref="KeyNotFoundException"></exception>
		TDestination Map<TSource, TDestination>(TSource source, string name = "")
			where TSource : class
			where TDestination : class;

		/// <summary>
		/// Expression based form of <see cref="Map{TSource,TDestination}(TSource,string)"/>.
		/// </summary>
		/// <typeparam name="TSource">Type of object to take values from.</typeparam>
		/// <typeparam name="TDestination">Type of object that will be created.</typeparam>
		/// <param name="name">Name of the map to use (or default map if empty).</param>
		/// <returns>An <see cref="Expression"/> capable of mapping <typeparamref name="TSource"/> objects to <typeparamref name="TDestination"/>.</returns>
		/// <exception cref="KeyNotFoundException"></exception>
		Expression<Func<TSource, TDestination>> Map<TSource, TDestination>(string name = "")
			where TSource : class
			where TDestination : class;

		/// <summary>
		/// Updates <paramref name="destination"/> values from <paramref name="source"/> using a map named <paramref name="name"/>.
		/// </summary>
		/// <typeparam name="TSource">Type of <paramref name="source"/> to take values from.</typeparam>
		/// <typeparam name="TDestination">Type of <paramref name="destination"/> object that will be updated.</typeparam>
		/// <param name="source">Object to take values from.</param>
		/// <param name="destination">Object to update from <paramref name="source"/> values.</param>
		/// <param name="name">Name of the map to use (or default map if empty).</param>
		/// <returns><paramref name="destination"/> object with updated values.</returns>
		/// <exception cref="KeyNotFoundException"></exception>
		TDestination Map<TSource, TDestination>(TSource source, TDestination destination, string name = "");
	}
}
