namespace Sample.Domain.Models
{
	public class ArticleResult
	{
		public int Id { get; set; }
		public string Text { get; set; }
		public int UserId { get; set; }
		public UserResult User { get; set; }
	}
}

