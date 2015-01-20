using Telerik.OpenAccess.Metadata.Fluent;

namespace Sample.Data.Models.Mapping
{
	public class UserLoginMapping : MappingConfiguration<UserLogin>
	{
		public UserLoginMapping()
		{
			MapType(o => new
			{
				Id = o.Id,
				Priority = o.Priority,
				ProviderKey = o.ProviderKey,
				ProviderName = o.ProviderName,
				UserId = o.UserId
			}).ToTable("UserLogins");
			HasProperty(o => o.Id).IsIdentity();
			HasAssociation(us => us.User)
				.WithOpposite(u => u.UserLogins)
				.HasConstraint((us, u) => us.UserId == u.Id);
		}
	}
}
