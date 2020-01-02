using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kritikos.PureMapper.Contracts;

namespace Kritikos.PureMapper
{
	public static class Querable
	{
		public static IQueryable<TDestination> Project<TSource, TDestination>(this IQueryable<TSource> source, IPureMapper mapper)
			where TSource : class
			where TDestination : class
		{
			return source.Select(mapper.Map<TSource, TDestination>());
	    }
	}
}
