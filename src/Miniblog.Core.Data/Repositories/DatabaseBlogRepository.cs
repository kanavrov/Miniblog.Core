using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Dapper;
using Miniblog.Core.Contract.Models;
using Miniblog.Core.Data.Models;
using Miniblog.Core.Framework.Common;
using Miniblog.Core.Framework.Users;

namespace Miniblog.Core.Data.Repositories
{
	public class DatabaseBlogRepository : DatabaseRepositoryBase, IBlogRepository
	{
		private readonly IUserRoleResolver _userRoleResolver;
		private readonly IDateTimeProvider _dateTimeProvider;
		private readonly IMapper _mapper;

		public DatabaseBlogRepository(IDbTransaction transaction, IUserRoleResolver userRoleResolver, 
			IDateTimeProvider dateTimeProvider, IMapper mapper)
			: base(transaction)
		{
			_dateTimeProvider = dateTimeProvider;
			_mapper = mapper;
			_userRoleResolver = userRoleResolver;

		}
		public async Task AddCategory(ICategory category)
		{
			var existing = await GetCategoryByName(category.Name);

			if(existing == null)
			{
				await ExecuteNonQueryAsync("INSERT INTO Category (Id, Name) VALUES (@Id, @Name)", category);
			}			
		}

		public async Task AddComment(IComment comment, Guid postId)
		{
			var postPKeyId = await GetPostPKeyId(postId);

			if(postPKeyId != null)
			{
				var commentEntity = _mapper.Map<Comment>(comment);
				commentEntity.PostPKeyId = postPKeyId.Value;

				var command = new StringBuilder(512);
				command.AppendLine("INSERT INTO Comment (Id, PostPKeyId, Author, Email, Content, PubDate, IsAdmin)");
				command.AppendLine("VALUES (@Id, @PostPKeyId, @Author, @Email, @Content, @PubDate, @IsAdmin)");
				await ExecuteNonQueryAsync(command.ToString(), commentEntity);
			}
		}

		public async Task AddPost(IPost post)
		{
			var command = new StringBuilder(512);
			command.AppendLine("INSERT INTO Post");
			command.AppendLine("(Id, Title, Slug, Excerpt, Content, PubDate, LastModified, IsPublished) VALUES");
			command.AppendLine("(@Id, @Title, @Slug, @Excerpt, @Content, @PubDate, @LastModified, @IsPublished)");

			await ExecuteNonQueryAsync(command.ToString(), post);

			var postPKeyId = await GetPostPKeyId(post.Id);
			
			await AddPostCategoryRelations(post, postPKeyId.Value);
		}

		private async Task<long?> GetPostPKeyId(Guid postId)
		{
			return await ExecuteScalarAsync<long?>("SELECT PKeyId FROM Post WHERE Id = @Id AND IsDeleted = 0", new { Id = postId });
		}

		private async Task<long?> GetCategoryPKeyId(Guid categoryId)
		{
			return await ExecuteScalarAsync<long?>("SELECT PKeyId FROM Category WHERE Id = @Id", new { Id = categoryId });
		}

		public async Task UpdateCategory(ICategory category)
		{
			await ExecuteNonQueryAsync("UPDATE Category SET Name = @Name WHERE Id = @Id", category);
		}

		public async Task UpdatePost(IPost post)
		{
			var postPKeyId = await GetPostPKeyId(post.Id);
			if(postPKeyId == null)
			{
				return;
			}

			var command = new StringBuilder(512);
			command.AppendLine("UPDATE Post");
			command.AppendLine("SET Title = @Title");
			command.AppendLine(", Slug = @Slug");
			command.AppendLine(", Excerpt = @Excerpt");
			command.AppendLine(", Content = @Content");
			command.AppendLine(", PubDate = @PubDate");
			command.AppendLine(", LastModified = @LastModified");
			command.AppendLine(", IsPublished = @IsPublished");
			command.AppendLine("WHERE Id = @Id");
			await ExecuteNonQueryAsync(command.ToString(), post);
			
			await DeletePostCategoryRelationsByPost(postPKeyId.Value);
			await AddPostCategoryRelations(post, postPKeyId.Value);
		}

		private async Task DeletePostCategoryRelationsByPost(long postPKeyId)
		{
			await ExecuteNonQueryAsync("DELETE FROM PostCategoryRel WHERE PostPKeyId = @PostPKeyId", new { PostPKeyId = postPKeyId });
		}

		private async Task DeletePostCategoryRelationsByCategory(long categoryPKeyId)
		{
			await ExecuteNonQueryAsync("DELETE FROM PostCategoryRel WHERE CategoryPKeyId = @CategoryPKeyId", new { CategoryPKeyId = categoryPKeyId });
		}

		private async Task AddPostCategoryRelations(IPost post, long postPKeyId)
		{
			var categories = await GetCategoriesByIds(post.Categories.Select(c => c.Id).ToArray());
			foreach(var category in categories)
			{
				await ExecuteNonQueryAsync("INSERT INTO PostCategoryRel (Id, PostPKeyId, CategoryPKeyId) VALUES (@Id, @PostPKeyId, @CategoryPKeyId)", 
					new 
					{ 
						PostPKeyId = postPKeyId,
						CategoryPKeyId = category.PKeyId,
						Id = Guid.NewGuid()
					});
			}
		}

		public async Task DeleteCategory(Guid categoryId)
		{
			var categoryPKeyId = await GetCategoryPKeyId(categoryId);
			if(categoryPKeyId == null)
			{
				return;
			}
			await DeletePostCategoryRelationsByCategory(categoryPKeyId.Value);
			await ExecuteNonQueryAsync("DELETE FROM Category WHERE Id = @Id", new { Id = categoryId });
		}

		public async Task DeleteComment(Guid commentId, Guid postId)
		{
			await ExecuteNonQueryAsync("DELETE FROM Comment WHERE Id = @Id", new { Id = commentId });
		}

		public async Task DeletePost(Guid id)
		{
			await ExecuteNonQueryAsync("UPDATE Post SET IsDeleted = 1 WHERE Id = @Id", new { Id = id });
		}

		public async Task<IEnumerable<ICategory>> GetCategories()
		{
			var command = new StringBuilder(512);
			command.AppendLine("SELECT Category.PKeyId, Category.Id, Category.Name, COUNT(Post.Id) AS PostCount");
			command.AppendLine("LEFT OUTER JOIN PostCategoryRel ON PostCategoryRel.CategoryPKeyId = Category.PKeyId");
			command.AppendLine("LEFT OUTER JOIN Post ON PostCategoryRel.PostPKeyId = Post.PKeyId AND Post.IsDeleted = 0");
			
			if (!_userRoleResolver.IsAdmin())
			{
				command.AppendLine("AND Post.PubDate <= @PubDate");
			}
			command.AppendLine("GROUP BY Category.PKeyId, Category.Id, Category.Name");
			return await ExecuteQueryAsync<Category>(command.ToString(), new { PubDate = _dateTimeProvider.Now.Date });
		}

		public async Task<ICategory> GetCategoryById(Guid id)
		{
			var command = new StringBuilder(512);
			command.AppendLine("SELECT Category.PKeyId, Category.Id, Category.Name, COUNT(Post.Id) AS PostCount");
			command.AppendLine("LEFT OUTER JOIN PostCategoryRel ON PostCategoryRel.CategoryPKeyId = Category.PKeyId");
			command.AppendLine("LEFT OUTER JOIN Post ON PostCategoryRel.PostPKeyId = Post.PKeyId AND Post.IsDeleted = 0");
			
			if (!_userRoleResolver.IsAdmin())
			{
				command.AppendLine("AND Post.PubDate <= @PubDate");
			}
			command.AppendLine("WHERE Id = @Id");
			command.AppendLine("GROUP BY Category.PKeyId, Category.Id, Category.Name");
			return await ExecuteQueryFirstOrDefaultAsync<Category>(command.ToString(), new { Id = id, PubDate = _dateTimeProvider.Now.Date });
		}

		private async Task<Category> GetCategoryByName(string name)
		{
			var command = new StringBuilder(512);
			command.AppendLine("SELECT Category.PKeyId, Category.Id, Category.Name");
			command.AppendLine("WHERE UPPER(Name) = @Name");
			return await ExecuteQueryFirstOrDefaultAsync<Category>(command.ToString(), new { Name = name.ToUpper() });
		}

		private async Task<IEnumerable<Category>> GetCategoriesByIds(Guid[] ids)
		{
			var command = new StringBuilder(512);
			command.AppendLine("SELECT Category.PKeyId, Category.Id, Category.Name");
			command.AppendLine("WHERE Id IN @Ids");
			return await ExecuteQueryAsync<Category>(command.ToString(), new { Ids = ids });
		}

		public async Task<IPost> GetPostById(Guid id)
		{
			var command = new StringBuilder(512);
			command.AppendLine("SELECT Post.*");
			command.AppendLine("FROM Post");
			command.AppendLine("WHERE Id = @Id AND IsDeleted = 0");
			if (!_userRoleResolver.IsAdmin())
			{
				command.AppendLine("AND PubDate <= @PubDate");
			}
			var post = await ExecuteQueryFirstOrDefaultAsync<Post>(command.ToString(), new { Id = id });

			if(post != null)
			{
				await PopulateRelatedComments(post);
				await PopulateRelatedCategories(post);
			}			

			return post;
		}

		public async Task<IPost> GetPostBySlug(string slug)
		{
			var command = new StringBuilder(512);
			command.AppendLine("SELECT Post.*");
			command.AppendLine("FROM Post");
			command.AppendLine("WHERE UPPER(Slug) = @Slug AND IsDeleted = 0");
			if (!_userRoleResolver.IsAdmin())
			{
				command.AppendLine("AND PubDate <= @PubDate");
			}
			var post = await ExecuteQueryFirstOrDefaultAsync<Post>(command.ToString(), new { Slug = slug.ToUpper() });

			if(post != null)
			{
				await PopulateRelatedComments(post);
				await PopulateRelatedCategories(post);
			}			

			return post;
		}

		public async Task<IEnumerable<IPost>> GetPosts(int count, int skip = 0)
		{
			var command = new StringBuilder(512);
			command.AppendLine("SELECT Post.*");
			command.AppendLine("FROM Post");
			command.AppendLine("WHERE IsDeleted = 0");
			if (!_userRoleResolver.IsAdmin())
			{
				command.AppendLine("AND PubDate <= @PubDate");
			}
			command.AppendLine("LIMIT @Count OFFSET @Skip");
			
			var posts = (await ExecuteQueryAsync<Post>(command.ToString(), new { PubDate = _dateTimeProvider.Now.Date })).ToArray();
			await PopulateRelatedComments(posts);
			await PopulateRelatedCategories(posts);

			return posts;
		}

		public async Task<IEnumerable<IPost>> GetPostsByCategory(Guid categoryId, int count, int skip = 0)
		{
			var command = new StringBuilder(512);
			command.AppendLine("SELECT Post.*");
			command.AppendLine("FROM Post");
			command.AppendLine("INNER JOIN (");
			command.AppendLine("	SELECT PostPKeyId FROM PostCategoryRel");
			command.AppendLine("	INNER JOIN Category ON PostCategoryRel.CategoryPKeyId = Category.PKeyId");
			command.AppendLine("	WHERE Category.Id = @CategoryId");
			command.AppendLine(") PostCategory ON PostCategory.PostPKeyId = Post.PKeyId");
			command.AppendLine("WHERE IsDeleted = 0");
			if (!_userRoleResolver.IsAdmin())
			{
				command.AppendLine("AND PubDate <= @PubDate");
			}
			command.AppendLine("LIMIT @Count OFFSET @Skip");
			
			var posts = (await ExecuteQueryAsync<Post>(command.ToString(), new 
			{ 
				PubDate = _dateTimeProvider.Now.Date,
				CategoryId = categoryId
			})).ToArray();

			await PopulateRelatedComments(posts);
			await PopulateRelatedCategories(posts);

			return posts;
		}

		private async Task PopulateRelatedComments(params Post[] posts)
		{
			var postIds = posts.Select(p => p.PKeyId);

			var command = new StringBuilder(512);
			command.AppendLine("SELECT Comment.*");
			command.AppendLine("FROM Comment");
			command.AppendLine("WHERE Comment.PostPKeyId IN @PostIds");

			var comments = (await ExecuteQueryAsync<Comment>(command.ToString(), new { PostIds = postIds })).ToArray();

			foreach(var post in posts)
			{
				post.Comments = comments.Where(c => c.PostPKeyId == post.PKeyId).Cast<IComment>().ToList();
			}
		}

		private async Task PopulateRelatedCategories(params Post[] posts)
		{
			var postIds = posts.Select(p => p.PKeyId);

			var command = new StringBuilder(512);
			command.AppendLine("SELECT Category.*, PostCategoryRel.PostPKeyId");
			command.AppendLine("FROM Category");
			command.AppendLine("INNER JOIN PostCategoryRel ON Category.PKeyId = PostCategoryRel.CategoryPKeyId");
			command.AppendLine("WHERE PostCategoryRel.PostPKeyId IN @PostIds");

			var postCategories = (await ExecuteQueryAsync<PostCategory>(command.ToString(), new { PostIds = postIds })).ToArray();

			foreach(var post in posts)
			{
				post.Categories = postCategories.Where(c => c.PostPKeyId == post.PKeyId).Cast<ICategory>().ToList();
			}
		}		
	}
}