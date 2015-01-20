namespace Sample.Domain.Models
{
	public class UserLoginResult
	{
		public int Id { get; set; }
		public int Priority { get; set; }
		public string ProviderName { get; set; }
		public string ProviderKey { get; set; }
		public int UserId { get; set; }
		public UserResult User { get; set; }
	}
}
