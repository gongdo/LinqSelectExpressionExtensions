using System.Collections.Generic;

namespace Sample.Domain.Models
{
	public class UserResult : UserSimpleResult
	{
		public string Slogan { get; set; }
		public int Grade { get; set; }
		public string Description { get; set; }

		public IEnumerable<UserLoginResult> UserLogins { get; set; }
		public IEnumerable<ArticleResult> Articles { get; set; }
	}
}
