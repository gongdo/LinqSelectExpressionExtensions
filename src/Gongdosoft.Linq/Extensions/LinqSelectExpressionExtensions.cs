namespace Gongdosoft.Linq.Extensions
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Linq;
	using System.Linq.Expressions;
	using System.Reflection;

	/// <summary>
	/// Provides a set of static extension methods for Linq Select expression.
	/// </summary>
	public static class LinqSelectExpressionExtensions
	{
		public static Expression<Func<TSource, TResult>> Append<TSource, TResult, TAppendSource, TAppendResult>(
			this Expression<Func<TSource, TResult>> resultSelector,
			Expression<Func<TSource, TAppendSource>> appendSourceSelector,
			Expression<Func<TResult, TAppendResult>> appendTargetSelector,
			Expression<Func<TAppendSource, TAppendResult>> appendResultSelector)
		{
			var visitor = new AppendVisitor<TSource, TResult, TAppendSource, TAppendResult>(appendSourceSelector, appendTargetSelector, appendResultSelector);
			var result = visitor.Visit(resultSelector);
			return (Expression<Func<TSource, TResult>>)result;
		}

		private class AppendVisitor<TSource, TResult, TAppendSource, TAppendResult> : ExpressionVisitor
		{
			private readonly Expression<Func<TSource, TAppendSource>> appendSourceSelector;
			private readonly Expression<Func<TResult, TAppendResult>> appendTargetSelector;
			private readonly Expression<Func<TAppendSource, TAppendResult>> appendResultSelector;
			private ReadOnlyCollection<ParameterExpression> rootParameters;
			public AppendVisitor(Expression<Func<TSource, TAppendSource>> appendSourceSelector, Expression<Func<TResult, TAppendResult>> appendTargetSelector, Expression<Func<TAppendSource, TAppendResult>> appendResultSelector)
			{
				this.appendSourceSelector = appendSourceSelector;
				this.appendTargetSelector = appendTargetSelector;
				this.appendResultSelector = appendResultSelector;
			}

			protected override Expression VisitLambda<T>(Expression<T> node)
			{
				rootParameters = node.Parameters;
				return base.VisitLambda(node);
			}

			protected override Expression VisitMemberInit(MemberInitExpression node)
			{
				var appendResultSelectorBody = appendResultSelector.Body as MemberInitExpression;
				if (appendResultSelectorBody == null)
				{
					throw new NotSupportedException(
						"AppendVisitor supports only MemberInitExpression as an appendResultSelector.Body");
				}
				var appendTargetSelectorBody = appendTargetSelector.Body as MemberExpression;
				if (appendTargetSelectorBody == null)
				{
					throw new NotSupportedException(
						"AppendVisitor supports only MemberExpression as an appendTargetSelectorBody.Body");
				}

				var replaceMemberVisitor = new ReplaceMemberVisitor(rootParameters, appendSourceSelector.Body);

				// supports only MemberAssignemnt
				var newBindings = appendResultSelectorBody.Bindings.OfType<MemberAssignment>().Select(o =>
				{
					var result = replaceMemberVisitor.Visit(o.Expression);
					return Expression.Bind(o.Member, result);
				});

				var memberInit = Expression.MemberInit(appendResultSelectorBody.NewExpression, newBindings);
				var binding = Expression.Bind(appendTargetSelectorBody.Member, memberInit);

				// add or replace existing binding
				var nodeBindings = new Dictionary<MemberInfo, MemberBinding>(node.Bindings.ToDictionary(o => o.Member, o => o));
				nodeBindings[binding.Member] = binding;
				return Expression.MemberInit(node.NewExpression, nodeBindings.Values);
			}

			private class ReplaceMemberVisitor : ExpressionVisitor
			{
				private readonly ReadOnlyCollection<ParameterExpression> rootParameters;
				private readonly Expression appendSourceSelectorBody;

				public ReplaceMemberVisitor(ReadOnlyCollection<ParameterExpression> rootParameters, Expression appendSourceSelectorBody)
				{
					this.rootParameters = rootParameters;
					this.appendSourceSelectorBody = appendSourceSelectorBody;
				}

				protected override Expression VisitMember(MemberExpression node)
				{
					if (node.Member.DeclaringType == appendSourceSelectorBody.Type)
					{
						var result = new ReplaceParameterVisitor(rootParameters).Visit(appendSourceSelectorBody);
						return Expression.MakeMemberAccess(result, node.Member);
					}
					return base.VisitMember(node);
				}
			}

			private class ReplaceParameterVisitor : ExpressionVisitor
			{
				private readonly ReadOnlyCollection<ParameterExpression> rootParameters;

				public ReplaceParameterVisitor(ReadOnlyCollection<ParameterExpression> rootParameters)
				{
					this.rootParameters = rootParameters;
				}

				protected override Expression VisitParameter(ParameterExpression node)
				{
					foreach (var parameter in rootParameters)
					{
						if (parameter.Type == node.Type)
						{
							return parameter;
						}
					}
					return base.VisitParameter(node);
				}
			}
		}
	}
}
