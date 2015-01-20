namespace Sample.Data.Models
{
	public class UserLogin
	{
		public int Id { get; set; }
		public int Priority { get; set; }
		public string ProviderName { get; set; }
		public string ProviderKey { get; set; }
		public int UserId { get; set; }
		public virtual User User { get; set; }
	}
}
