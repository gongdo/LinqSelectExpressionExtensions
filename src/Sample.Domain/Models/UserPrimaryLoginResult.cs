namespace Sample.Domain.Models
{
	public class UserPrimaryLoginResult : UserSimpleResult
	{
		public UserLoginResult PrimaryLogin { get; set; }
	}
}
