using Telerik.OpenAccess.Metadata.Fluent;

namespace Sample.Data.Models.Mapping
{
	public class ArticleMapping : MappingConfiguration<Article>
	{
		public ArticleMapping()
		{
			MapType(o => new
			{
				Id = o.Id,
				Text = o.Text,
				UserId = o.UserId,
			}).ToTable("Articles");
			HasProperty(o => o.Id).IsIdentity();
			HasAssociation(c => c.User)
				.WithOpposite(u => u.Articles)
				.HasConstraint((c, u) => c.UserId == u.Id);
		}
	}
}
