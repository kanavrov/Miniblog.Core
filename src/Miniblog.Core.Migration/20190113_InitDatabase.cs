using FluentMigrator;

namespace Miniblog.Core.Migration
{
	[Migration(1)]
	public class InitDatabase : FluentMigrator.Migration
	{
		public override void Up()
		{
			Create.Table("Post")
				.WithColumn("PKeyId").AsInt64().Identity().PrimaryKey("PK_Post")
				.WithColumn("Id").AsString().Unique("UQ_Post")
				.WithColumn("Title").AsString()
				.WithColumn("Slug").AsString().Indexed("IX_Post_Slug")
				.WithColumn("Excerpt").AsString()
				.WithColumn("Content").AsString()
				.WithColumn("PubDate").AsDateTime2()
				.WithColumn("LastModified").AsDateTime2()
				.WithColumn("IsPublished").AsBoolean()
				.WithColumn("IsDeleted").AsBoolean().WithDefaultValue(false);

			Create.Table("Category")
				.WithColumn("PKeyId").AsInt64().Identity().PrimaryKey("PK_Category")
				.WithColumn("Id").AsString().Unique("UQ_Category")
				.WithColumn("Name").AsString().Indexed()
				.WithColumn("LastModified").AsDateTime2();

			Create.Table("PostCategoryRel")
				.WithColumn("PKeyId").AsInt64().Identity().PrimaryKey("PK_PostCategory")
				.WithColumn("Id").AsString().Unique("UQ_PostCategory")
				.WithColumn("PostPKeyId").AsInt64().ForeignKey("FK_PostCategoryPost", "Post", "PKeyId")
				.WithColumn("CategoryPKeyId").AsInt64().ForeignKey("FK_PostCategoryCategory", "Category", "PKeyId");

			Create.Table("Comment")
				.WithColumn("PKeyId").AsInt64().Identity().PrimaryKey("PK_Comment")
                .WithColumn("Id").AsString().Unique("UQ_Comment")
				.WithColumn("PostPKeyId").AsInt64().ForeignKey("FK_CommentPost", "Post", "PKeyId")
                .WithColumn("Author").AsString()
				.WithColumn("Email").AsString()
				.WithColumn("Content").AsString()
				.WithColumn("PubDate").AsDateTime2()
				.WithColumn("IsAdmin").AsBoolean();
		}

		public override void Down()
		{
			Delete.Table("Comment");
			Delete.Table("PostCategory");
			Delete.Table("Category");
			Delete.Table("Post");
		}
	}
}