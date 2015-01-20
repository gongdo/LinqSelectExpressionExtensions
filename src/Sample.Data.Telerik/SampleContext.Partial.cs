using System;
using System.Collections.Generic;
using System.Linq;
using Sample.Data.Models;

namespace Sample.Data.Telerik
{
	public partial class SampleContext
	{
		public IQueryable<User> Users
		{
			get
			{
				return this.GetAll<User>();
			}
		}

		public IQueryable<UserLogin> UserUserLogins
		{
			get
			{
				return this.GetAll<UserLogin>();
			}
		}

		public IQueryable<Article> Articles
		{
			get
			{
				return this.GetAll<Article>();
			}
		}
	}
}
