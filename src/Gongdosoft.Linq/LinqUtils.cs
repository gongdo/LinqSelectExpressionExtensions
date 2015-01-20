using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Gongdosoft.Linq
{
	public class LinqUtils
	{
		public static Expression<Func<TSource, TResult>> GenerateSelector<TSource, TResult>() where TResult : new()
		{
			var sourceMembers = typeof(TSource).GetProperties();
			var destinationMembers = typeof(TResult).GetProperties();

			const string name = "src";

			var parameterExpression = Expression.Parameter(typeof(TSource), name);
			var bindings =
				destinationMembers.Select(dest =>
				{
					// 이름과 타입이 같은 것만 바인딩
					var sourceMember = sourceMembers.FirstOrDefault(pi => pi.Name == dest.Name && dest.PropertyType == pi.PropertyType);
					if (sourceMember == null)
					{
						return null;
					}
					return Expression.Bind(dest, Expression.Property(parameterExpression, sourceMember));
				})
				.Where(o => o != null)
				.ToArray<MemberBinding>();

			return Expression.Lambda<Func<TSource, TResult>>(
				Expression.MemberInit(Expression.New(typeof(TResult)), bindings),
				parameterExpression
			);
		}
	}
}
