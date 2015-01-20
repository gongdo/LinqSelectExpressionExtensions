using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Gongdosoft.Linq.Extensions;
using Sample.Data.Models;
using Sample.Domain.Models;
using Newtonsoft.Json;
using System.Diagnostics;

namespace Sample
{
	class Program
	{
		private class ExpressionProvider
		{
			// Simple expressions that select dto
			public Expression<Func<User, UserSimpleResult>> UserSimpleSelector
			{
				get
				{
					return o => new UserSimpleResult
					{
						Id = o.Id,
						Nickname = o.Nickname ?? "(null)",
						UserName = o.UserName
					};
				}
			}

			public Expression<Func<User, UserPrimaryLoginResult>> UserPrimaryLoginSelector
			{
				get
				{
					return o => new UserPrimaryLoginResult
					{
						Id = o.Id,
						Nickname = o.Nickname ?? "(null)",
						UserName = o.UserName
					};
				}
			}

			public Expression<Func<User, UserPrimaryLoginResult>> PrimaryLoginAppendedUserSelector
			{
				get
				{
					Expression<Func<User, UserPrimaryLoginResult>> expression = o => new UserPrimaryLoginResult
					{
						Id = o.Id,
						Nickname = o.Nickname ?? "(null)",
						UserName = o.UserName,
					};
					return expression.Append(o => o.UserLogins.OrderByDescending(login => login.Priority).FirstOrDefault(),
						o => o.PrimaryLogin, UserLoginSelector);
				}
			}

			public Expression<Func<User, UserPrimaryLoginResult>> FullUserPrimaryLoginSelector
			{
				get
				{
					return o => new UserPrimaryLoginResult
					{
						Id = o.Id,
						Nickname = o.Nickname ?? "(null)",
						UserName = o.UserName,
						PrimaryLogin = o.UserLogins.AsQueryable().Select(UserLoginSelector).OrderByDescending(login => login.Priority).FirstOrDefault(),
					};
				}
			}

			public Expression<Func<User, UserResult>> UserSelector
			{
				get
				{
					return o => new UserResult
					{
						Description = o.Description,
						Grade = o.Grade,
						Slogan = o.Slogan,
						UserLogins = o.UserLogins.AsQueryable().Select(UserLoginSelector).ToList(),
						Articles = o.Articles.AsQueryable().Select(ArticleSelector).ToList(),
					};
				}
			}


			public Expression<Func<UserLogin, UserLoginResult>> UserLoginSelector
			{
				get
				{
					return o => new UserLoginResult
					{
						Id = o.Id,
						Priority = o.Priority,
						ProviderKey = o.ProviderKey,
						ProviderName = o.ProviderName,
						UserId = o.UserId
					};
				}
			}

			public Expression<Func<Article, ArticleResult>> ArticleSelector
			{
				get
				{
					return o => new ArticleResult
					{
						Id = o.Id,
						Text = o.Text,
						UserId = o.UserId
					};
				}
			}
		}

		private class UserSimpleResultExpressionProvider
		{
			private readonly ExpressionProvider provider = new ExpressionProvider();
			public Expression<Func<User, UserSimpleResult>> UserSimpleSelector { get { return provider.UserSimpleSelector; } }
		}

		private class UserPrimaryLoginResultExpressionProvider
		{
			private readonly ExpressionProvider provider = new ExpressionProvider();
			public Expression<Func<User, UserPrimaryLoginResult>> UserPrimaryLoginSelector { get { return provider.UserPrimaryLoginSelector; } }
			public Expression<Func<User, UserPrimaryLoginResult>> PrimaryLoginAppendedUserSelector { get { return provider.PrimaryLoginAppendedUserSelector; } }

			private readonly UserLoginResultExpressionProvider userLoginResultProvider = new UserLoginResultExpressionProvider();
			public Expression<Func<User, UserPrimaryLoginResult>> FullUserPrimaryLoginSelector
			{
				get
				{
					return o => new UserPrimaryLoginResult
					{
						Id = o.Id,
						Nickname = o.Nickname ?? "(null)",
						UserName = o.UserName,
						PrimaryLogin = o.UserLogins.AsQueryable().Select(userLoginResultProvider.UserLoginSelector).OrderByDescending(login => login.Priority).FirstOrDefault(),
					};
				}
			}
		}

		private class UserResultExpressionProvider
		{
			private readonly ExpressionProvider provider = new ExpressionProvider();
			public Expression<Func<User, UserResult>> UserSelector { get { return provider.UserSelector; } }
		}

		private class UserLoginResultExpressionProvider
		{
			private readonly ExpressionProvider provider = new ExpressionProvider();
			public Expression<Func<UserLogin, UserLoginResult>> UserLoginSelector { get { return provider.UserLoginSelector; } }
		}

		private class ArticleResultExpressionProvider
		{
			private readonly ExpressionProvider provider = new ExpressionProvider();
			public Expression<Func<Article, ArticleResult>> ArticleSelector { get { return provider.ArticleSelector; } }
		}

		private static ExpressionProvider expressions = new ExpressionProvider();

		static void Main(string[] args)
		{
			Seeding();
			SimpleSelecting();
			SimpleFullSelecting();
			MergeSelecting();
			AppendSingleSelecting();
			PreAppendedSelecting();
			AppendMultipleSelecting();

			Console.WriteLine("Completed");
			Console.ReadKey();
		}

		private static void SimpleFullSelecting()
		{
			DumpUserSelecting(expressions.FullUserPrimaryLoginSelector);
			var provider = new UserPrimaryLoginResultExpressionProvider();
			DumpUserSelecting(provider.FullUserPrimaryLoginSelector);
		}

		private static void PreAppendedSelecting()
		{
			DumpUserSelecting(expressions.PrimaryLoginAppendedUserSelector);
			var provider = new UserPrimaryLoginResultExpressionProvider();
			DumpUserSelecting(provider.PrimaryLoginAppendedUserSelector);
		}

		private static void AppendMultipleSelecting()
		{
			//var appendSelecting = UserSelector.Append(
			//	o => o.UserLogins.ToList(),
			//	o => o.UserLogins,
			//	ArticleSelector);
		}

		private static void AppendSingleSelecting()
		{
			var appendSelector = expressions.UserPrimaryLoginSelector.Append(
				o => o.UserLogins.AsQueryable().OrderByDescending(login => login.Priority).FirstOrDefault(),
				o => o.PrimaryLogin,
				expressions.UserLoginSelector);
			DumpUserSelecting(appendSelector);
			var provider = new UserPrimaryLoginResultExpressionProvider();
			var userLoginProvider = new UserLoginResultExpressionProvider();
			var appendSelector2 = provider.UserPrimaryLoginSelector.Append(
				o => o.UserLogins.AsQueryable().OrderByDescending(login => login.Priority).FirstOrDefault(),
				o => o.PrimaryLogin,
				userLoginProvider.UserLoginSelector);
			DumpUserSelecting(appendSelector2);
		}

		private static void MergeSelecting()
		{
			var mergingSelector = expressions.UserSimpleSelector.Merge(expressions.UserSelector);
			DumpUserSelecting(mergingSelector);
			var provider = new UserSimpleResultExpressionProvider();
			var provider2 = new UserResultExpressionProvider();
			var mergingSelector2 = provider.UserSimpleSelector.Merge(provider2.UserSelector);
			DumpUserSelecting(mergingSelector2);

			DumpUserSelecting(mergingSelector.Exclude(o => o.Articles, o => o.UserLogins, o => o.Nickname));
		}

		private static void SimpleSelecting()
		{
			DumpUserSelecting(expressions.UserSimpleSelector);
			var provider = new UserSimpleResultExpressionProvider();
			DumpUserSelecting(provider.UserSimpleSelector);
		}



		private static void DumpUserSelecting<TResult>(Expression<Func<User, TResult>> selector)
		{
			using (var context = new Sample.Data.SampleContext())
			{
				var results = context.Users
					.Where(o => o.Id > 3)
					.OrderByDescending(o => o.Id)
					.Skip(5)
					.Take(10)
					.Select(selector)
					.ToList();
				var json = JsonConvert.SerializeObject(results);
				Console.WriteLine("SimpleSelecting EF:\n{0}", json);
				Debug.WriteLine(string.Format("SimpleSelecting EF:\n{0}", json));
			}

			//using (var context = new Sample.Data.Telerik.SampleContext())
			//{
			//	var results = context.Users
			//		.Where(o => o.Id > 3)
			//		.OrderByDescending(o => o.Id)
			//		.Skip(5)
			//		.Take(10)
			//		.Select(selector)
			//		.ToList();
			//	var json = JsonConvert.SerializeObject(results);
			//	Console.WriteLine("SimpleSelecting DA:\n{0}", json);
			//	Debug.WriteLine(string.Format("SimpleSelecting DA:\n{0}", json));
			//}
		}

		private static void Seeding()
		{
			using (var context = new Sample.Data.SampleContext())
			{
				if (context.Users.Any())
				{
					return;
				}

				for (var i = 0; i < 100; i++)
				{
					var user = new User
					{
						UserName = Guid.NewGuid().ToString(),
						Nickname = (i % 5 == 0) ? null : Guid.NewGuid().ToString(),
						UserLogins = new List<UserLogin>
						{
							new UserLogin { Priority = 1, ProviderKey = "Sample" },
							new UserLogin { Priority = 0, ProviderKey = "Facebook" }
						},
						Articles = new List<Article>
						{
							new Article { Text = Guid.NewGuid().ToString() },
							new Article { Text = Guid.NewGuid().ToString() },
							new Article { Text = Guid.NewGuid().ToString() }
						}
					};
					context.Users.Add(user);
				}
				context.SaveChanges();
			}
		}

	}
}
