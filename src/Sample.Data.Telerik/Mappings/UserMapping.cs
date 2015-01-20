using Telerik.OpenAccess.Metadata.Fluent;

namespace Sample.Data.Models.Mapping
{
	public class UserMapping : MappingConfiguration<User>
	{
		public UserMapping()
		{
			MapType(o => new
			{
				Grade = o.Grade,
				Id = o.Id,
				Nickname = o.Nickname,
				Slogan = o.Slogan,
				UserName = o.UserName,
				Description = o.Description,
			}).ToTable("Users");
			HasProperty(o => o.Id).IsIdentity();
		}
	}
}
