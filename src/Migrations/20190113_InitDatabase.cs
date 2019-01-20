using FluentMigrator;

namespace Miniblog.Core.Migrations
{
	[Migration(1)]
	public class InitDatabase : Migration
	{
		public override void Up()
		{
			Create.Table("Post")
				.WithColumn("Id").AsGuid().PrimaryKey("PK_Post")
				.WithColumn("Title").AsString()
				.WithColumn("Slug").AsString()
				.WithColumn("Excerpt").AsString()
				.WithColumn("Content").AsString()
				.WithColumn("PubDate").AsDateTime2()
				.WithColumn("LastModified").AsDateTime2()
				.WithColumn("IsPublished").AsBoolean();

			Create.Table("Category")
				.WithColumn("Id").AsGuid().PrimaryKey("PK_Category")
				.WithColumn("Name").AsString();

			Create.Table("PostCategoryRel")
				.WithColumn("Id").AsGuid().PrimaryKey("PK_PostCategory")
				.WithColumn("PostId").AsGuid().ForeignKey("FK_PostCategoryPost", "Post", "Id")
				.WithColumn("CategoryId").AsGuid().ForeignKey("FK_PostCategoryCategory", "Category", "Id");

			Create.Table("Comment")
                .WithColumn("Id").AsGuid().PrimaryKey("PK_Comment")
				.WithColumn("PostId").AsGuid().ForeignKey("FK_CommentPost", "Post", "Id")
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