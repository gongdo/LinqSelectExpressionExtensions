using System.Data.Entity;
using Sample.Data.Models;

namespace Sample.Data
{
	public class SampleContext : DbContext
	{
		public IDbSet<User> Users { get; set; }
		public IDbSet<UserLogin> UserLogins { get; set; }
		public IDbSet<Article> Comments { get; set; }
	}
}
