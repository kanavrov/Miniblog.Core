using System;
using System.Data;
using Dapper;
using FluentMigrator;

namespace Miniblog.Core.Migration
{
	[Migration(2)]
	public class TestData : FluentMigrator.Migration
	{
		public override void Up()
		{			
			Execute.WithConnection(GenerateTestData);
		}

		private void GenerateTestData(IDbConnection conn, IDbTransaction tran)
		{
			var postId = new Guid("cfe1dc89-35f8-47f4-b8d0-e44916fe2fde");
			var postPKey = AddPost(new
			{
				Id = postId,
				Title = "Welcome to your new blog",
				Slug = "welcome",
				Excerpt = "Congratulations on your new blog",
				Content = @"<p>Congratulations on installing your new blog!</p>
<p><a href=""https://github.com/madskristensen/Miniblog.Core"">Miniblog.Core</a> is optimized to give your visitors a fantastic reading experience on all types of devices. It is built to be fast and responsive so your visitors never have to wait for the content to load.</p>
<h2>Fonts and layout</h2>
<p>Instead of using custom fonts that visitors need to download, Miniblog.Core uses fonts native to each device and operating system. This ensures that the blog loads very fast and the fonts feel natural and beautiful on all devices.</p>
<blockquote>
<p>Common text layouts are being styled to look great. This is an example of what a blockquote looks like.</p>
</blockquote>
<p>Here is a list of just some of the features:</p>
<ul>
<li>Social media integration</li>
<li>User comments</li>
<li>Passing <a href=""http://webdevchecklist.com/"">Web Developer Checklist</a></li>
<li>Search engine optimized</li>
<li>Supported in all major browsers</li>
<li>Phone and tablet support</li>
<li>Fast and responsive</li>
<li>RSS/ATOM feeds</li>
<li>Windows/Open Live Writer/Markdown Monster support</li>
</ul>
<h2>Special formatting</h2>
<p>Pre-formatted text such as what is commonly used to display code syntax is styled beautifully.</p>
<pre><code>function code() {
   var msg = ""This is what a code snippet looks like"";
}</code></pre>
<p>And tables are formatted nicely with headings and alternative background color for rows.</p>
<table>
<tbody>
<tr>
<th>And tables</th>
<th>look</th>
<th>rally</th>
<th>nice</th>
<th>too</th>
</tr>
<tr>
<td>Numbers</td>
<td>2</td>
<td>3</td>
<td>4</td>
<td>5</td>
</tr>
<tr>
<td>Alphabet</td>
<td>b</td>
<td>c</td>
<td>d</td>
<td>e</td>
</tr>
<tr>
<td>Symbols</td>
<td>?</td>
<td>$</td>
<td>*</td>
<td>%</td>
</tr>
</tbody>
</table>
<p>Enjoy!</p>",
				PubDate = new DateTime(2019, 3, 9),
				LastModified = new DateTime(2019, 3, 10),
				IsPublished = true
			}, conn, tran);

			var categoryOneId = new Guid("3bfa98f8-9d6f-4a6d-8c0a-e3580096d527");
			var categoryOnePKey = AddCategory(new
			{
				Id = categoryOneId,
				Name = "welcome",
				LastModified = new DateTime(2019, 3, 10)
			}, conn, tran);

			var categoryTwoId = new Guid("4cf359b8-4d65-42e4-bc6d-aadbd4063e0b");
			var categoryTwoPKey = AddCategory(new
			{
				Id = categoryTwoId,
				Name = "miniblog",
				LastModified = new DateTime(2019, 3, 9)
			}, conn, tran);

			AddPostCategoryRelation(new
			{
				Id = new Guid("3aa38773-1ce9-4da3-b48f-8dc01cb34c57"),
				PostPKeyId = postPKey,
				CategoryPKeyId = categoryOnePKey
			}, conn, tran);

			AddPostCategoryRelation(new
			{
				Id = new Guid("16ce3e6d-2d06-4170-a059-cc2dc8926872"),
				PostPKeyId = postPKey,
				CategoryPKeyId = categoryTwoPKey
			}, conn, tran);

			AddComment(new
			{
				Id = new Guid("074be2b3-5441-475e-93b9-760c059392af"),
				PostPKeyId = postPKey,
				Author = "John Doe",
				Email = "john@gmail.com",
				Content = "This is a comment made by a visitor",
				PubDate = new DateTime(2019, 3, 9),
				IsAdmin = false
			}, conn, tran);

			AddComment(new
			{
				Id = new Guid("fa27aeff-08b8-4905-8114-3b6399f98f69"),
				PostPKeyId = postPKey,
				Author = "Mads Kristensen",
				Email = "post@madskristensen.net",
				Content = @"This is a comment made by the owner of the blog while logged in.

It looks slightly different to make it clear that this is a response from the owner.",
				PubDate = new DateTime(2019, 3, 9),
				IsAdmin = true
			}, conn, tran);
		}

		private long AddPost(object post, IDbConnection conn, IDbTransaction tran)
		{
			var insertCommand = @"INSERT INTO Post (Id, Title, Slug, Excerpt, Content, PubDate, LastModified, IsPublished) 
									VALUES (@Id, @Title, @Slug, @Excerpt, @Content, @PubDate, @LastModified, @IsPublished)";
			conn.Execute(insertCommand, post, tran);
			
			return conn.ExecuteScalar<long>("SELECT PKeyId FROM Post WHERE Id = @Id", post, tran);
		}

		private long AddCategory(object category, IDbConnection conn, IDbTransaction tran)
		{
			conn.Execute("INSERT INTO Category (Id, Name, LastModified) VALUES (@Id, @Name, @LastModified)", category, tran);
			return conn.ExecuteScalar<long>("SELECT PKeyId FROM Category WHERE Id = @Id", category, tran);
		}

		private void AddPostCategoryRelation(object rel, IDbConnection conn, IDbTransaction tran)
		{
			conn.Execute("INSERT INTO PostCategoryRel (Id, PostPKeyId, CategoryPKeyId) VALUES (@Id, @PostPKeyId, @CategoryPKeyId)", rel, tran);
		}

		private void AddComment(object comment, IDbConnection conn, IDbTransaction tran)
		{
			conn.Execute(@"INSERT INTO Comment (Id, PostPKeyId, Author, Email, Content, PubDate, IsAdmin) 
							VALUES (@Id, @PostPKeyId, @Author, @Email, @Content, @PubDate, @IsAdmin)", comment, tran);
		}

		public override void Down()
		{
			Delete.FromTable("PostCategoryRel").AllRows(); 
			Delete.FromTable("Comment").AllRows(); 
			Delete.FromTable("Post").AllRows(); 
			Delete.FromTable("Category").AllRows(); 
		}
	}
}