using System.Collections.Generic;

namespace Sample.Data.Models
{
	public class User
	{
		public int Id { get; set; }
		public string UserName { get; set; }
		public string Slogan { get; set; }
		public string Nickname { get; set; }
		public int Grade { get; set; }
		public string Description { get; set; }

		public virtual IList<UserLogin> UserLogins { get; set; }
		public virtual IList<Article> Articles { get; set; }
	}
}
