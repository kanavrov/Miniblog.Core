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

			if (existing == null)
			{
				category.Id = Guid.NewGuid();
				category.LastModified = _dateTimeProvider.Now;
				var command = new StringBuilder(512);
				command.AppendLine($"INSERT INTO {nameof(Category)}");
				command.AppendLine($"({nameof(Category.Id)}, {nameof(Category.Name)}, {nameof(Category.LastModified)})");
				command.AppendLine($"VALUES (@{nameof(Category.Id)}, @{nameof(Category.Name)}, @{nameof(Category.LastModified)})");
				await ExecuteNonQueryAsync(command.ToString(), category);
			}
		}

		public async Task AddComment(IComment comment, Guid postId)
		{
			var postPKeyId = await GetPostPKeyId(postId);

			if (postPKeyId != null)
			{
				var commentEntity = _mapper.Map<Comment>(comment);
				commentEntity.Id = Guid.NewGuid();
				commentEntity.PostPKeyId = postPKeyId.Value;

				var command = new StringBuilder(512);
				command.AppendLine($"INSERT INTO {nameof(Comment)}");
				command.AppendLine($"({nameof(Comment.Id)}, {nameof(Comment.PostPKeyId)}, {nameof(Comment.Author)}");
				command.AppendLine($", {nameof(Comment.Email)}, {nameof(Comment.Content)}, {nameof(Comment.PubDate)}, {nameof(Comment.IsAdmin)})");
				command.AppendLine($"VALUES (@{nameof(Comment.Id)}, @{nameof(Comment.PostPKeyId)}, @{nameof(Comment.Author)}");
				command.AppendLine($", @{nameof(Comment.Email)}, @{nameof(Comment.Content)}, @{nameof(Comment.PubDate)}, @{nameof(Comment.IsAdmin)})");
				await ExecuteNonQueryAsync(command.ToString(), commentEntity);
			}
		}

		public async Task AddPost(IPost post)
		{
			post.Id = Guid.NewGuid();
			post.LastModified = _dateTimeProvider.Now;

			var command = new StringBuilder(512);
			command.AppendLine($"INSERT INTO {nameof(Post)}");
			command.AppendLine($"({nameof(Post.Id)}, {nameof(Post.Title)}, {nameof(Post.Slug)}, {nameof(Post.Excerpt)}");
			command.AppendLine($", {nameof(Post.Content)}, {nameof(Post.PubDate)}, {nameof(Post.LastModified)}, {nameof(Post.IsPublished)}) VALUES");
			command.AppendLine($"(@{nameof(Post.Id)}, @{nameof(Post.Title)}, @{nameof(Post.Slug)}, @{nameof(Post.Excerpt)}");
			command.AppendLine($", @{nameof(Post.Content)}, @{nameof(Post.PubDate)}, @{nameof(Post.LastModified)}, @{nameof(Post.IsPublished)})");

			await ExecuteNonQueryAsync(command.ToString(), post);

			var postPKeyId = await GetPostPKeyId(post.Id);

			await AddPostCategoryRelations(post, postPKeyId.Value);
		}

		private async Task<long?> GetPostPKeyId(Guid postId)
		{
			return await ExecuteScalarAsync<long?>(
				$"SELECT {nameof(Post.PKeyId)} FROM {nameof(Post)} WHERE {nameof(Post.Id)} = @{nameof(Post.Id)} AND {nameof(Post.IsDeleted)} = 0",
			 	new { Id = postId }
			);
		}

		private async Task<long?> GetCategoryPKeyId(Guid categoryId)
		{
			return await ExecuteScalarAsync<long?>(
				$"SELECT {nameof(Category.PKeyId)} FROM {nameof(Category)} WHERE {nameof(Category.Id)} = @{nameof(Category.Id)}",
				new { Id = categoryId }
			);
		}

		public async Task UpdateCategory(ICategory category)
		{
			category.LastModified = _dateTimeProvider.Now;
			var command = new StringBuilder(512);
			command.AppendLine($"UPDATE {nameof(Category)}");
			command.AppendLine($"SET {nameof(Category.Name)} = @{nameof(Category.Name)}, {nameof(Category.LastModified)} = @{nameof(Category.LastModified)}");
			command.AppendLine($"WHERE {nameof(Category.Id)} = @{nameof(Category.Id)}");

			await ExecuteNonQueryAsync(command.ToString(), category);
		}

		public async Task UpdatePost(IPost post)
		{
			var postPKeyId = await GetPostPKeyId(post.Id);
			if (postPKeyId == null)
			{
				return;
			}

			post.LastModified = _dateTimeProvider.Now;

			var command = new StringBuilder(512);
			command.AppendLine($"UPDATE {nameof(Post)}");
			command.AppendLine($"SET {nameof(Post.Title)} = @{nameof(Post.Title)}");
			command.AppendLine($", {nameof(Post.Slug)} = @{nameof(Post.Slug)}");
			command.AppendLine($", {nameof(Post.Excerpt)} = @{nameof(Post.Excerpt)}");
			command.AppendLine($", {nameof(Post.Content)} = @{nameof(Post.Content)}");
			command.AppendLine($", {nameof(Post.PubDate)} = @{nameof(Post.PubDate)}");
			command.AppendLine($", {nameof(Post.LastModified)} = @{nameof(Post.LastModified)}");
			command.AppendLine($", {nameof(Post.IsPublished)} = @{nameof(Post.IsPublished)}");
			command.AppendLine($"WHERE {nameof(Post.Id)} = @{nameof(Post.Id)}");
			await ExecuteNonQueryAsync(command.ToString(), post);

			await DeletePostCategoryRelationsByPost(postPKeyId.Value);
			await AddPostCategoryRelations(post, postPKeyId.Value);
		}

		private async Task DeletePostCategoryRelationsByPost(long postPKeyId)
		{
			await ExecuteNonQueryAsync(
				$"DELETE FROM {nameof(PostCategoryRel)} WHERE {nameof(PostCategoryRel.PostPKeyId)} = @{nameof(PostCategoryRel.PostPKeyId)}",
				new { PostPKeyId = postPKeyId }
			);
		}

		private async Task DeletePostCategoryRelationsByCategory(long categoryPKeyId)
		{
			await ExecuteNonQueryAsync(
				$"DELETE FROM {nameof(PostCategoryRel)} WHERE {nameof(PostCategoryRel.CategoryPKeyId)} = @{nameof(PostCategoryRel.CategoryPKeyId)}",
				new { CategoryPKeyId = categoryPKeyId }
			);
		}

		private async Task AddPostCategoryRelations(IPost post, long postPKeyId)
		{
			var categories = await GetCategoriesByIds(post.Categories.Select(c => c.Id).ToArray());
			var command = new StringBuilder(512);
			command.AppendLine($"INSERT INTO {nameof(PostCategoryRel)}");
			command.AppendLine($"({nameof(PostCategoryRel.Id)}, {nameof(PostCategoryRel.PostPKeyId)}, {nameof(PostCategoryRel.CategoryPKeyId)}) VALUES");
			command.AppendLine($"(@{nameof(PostCategoryRel.Id)}, @{nameof(PostCategoryRel.PostPKeyId)}, @{nameof(PostCategoryRel.CategoryPKeyId)})");
			foreach (var category in categories)
			{
				await ExecuteNonQueryAsync(command.ToString(),
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
			if (categoryPKeyId == null)
			{
				return;
			}
			await DeletePostCategoryRelationsByCategory(categoryPKeyId.Value);
			await ExecuteNonQueryAsync(
				$"DELETE FROM {nameof(Category)} WHERE {nameof(Category.Id)} = @{nameof(Category.Id)}",
				new { Id = categoryId }
			);
		}

		public async Task DeleteComment(Guid commentId, Guid postId)
		{
			await ExecuteNonQueryAsync(
				$"DELETE FROM {nameof(Comment)} WHERE {nameof(Comment.Id)} = @{nameof(Comment.Id)}",
				new { Id = commentId }
			);
		}

		public async Task DeletePost(Guid id)
		{
			await ExecuteNonQueryAsync(
				$"UPDATE {nameof(Post)} SET {nameof(Post.IsDeleted)} = 1 WHERE {nameof(Post.Id)} = @{nameof(Post.Id)}",
				new { Id = id }
			);
		}

		public async Task<IEnumerable<ICategory>> GetCategories()
		{
			var command = new StringBuilder(512);
			AppendGetCategoryCommand(command, string.Empty);
			return await ExecuteQueryAsync<Category>(command.ToString(), new { PubDate = _dateTimeProvider.Now.Date });
		}

		public async Task<ICategory> GetCategoryById(Guid id)
		{
			var command = new StringBuilder(512);
			AppendGetCategoryCommand(command, $"{nameof(Category)}.{nameof(Category.Id)} = @{nameof(Category.Id)}");
			return await ExecuteQueryFirstOrDefaultAsync<Category>(command.ToString(), new { Id = id, PubDate = _dateTimeProvider.Now.Date });
		}

		private void AppendGetCategoryCommand(StringBuilder command, string whereCondition)
		{
			command.AppendLine($"SELECT {nameof(Category)}.{nameof(Category.PKeyId)}, {nameof(Category)}.{nameof(Category.Id)},");
			command.AppendLine($"{nameof(Category)}.{nameof(Category.Name)}, COUNT({nameof(Post)}.{nameof(Post.Id)}) AS {nameof(Category.PostCount)}");
			command.AppendLine($"FROM {nameof(Category)}");
			command.AppendLine($"LEFT JOIN {nameof(PostCategoryRel)}");
			command.AppendLine($"ON {nameof(PostCategoryRel)}.{nameof(PostCategoryRel.CategoryPKeyId)} = {nameof(Category)}.{nameof(Category.PKeyId)}");
			command.AppendLine($"LEFT JOIN {nameof(Post)}");
			command.AppendLine($"ON {nameof(PostCategoryRel)}.{nameof(PostCategoryRel.PostPKeyId)} = {nameof(Post)}.{nameof(Post.PKeyId)}");
			command.AppendLine($"AND {nameof(Post)}.{nameof(Post.IsDeleted)} = 0");

			if (!_userRoleResolver.IsAdmin())
			{
				command.AppendLine($"AND {nameof(Post)}.{nameof(Post.PubDate)} <= @{nameof(Post.PubDate)} AND {nameof(Post)}.{nameof(Post.IsPublished)} = 1");
			}
			if(!string.IsNullOrEmpty(whereCondition))
			{
				command.AppendLine($"WHERE {whereCondition}");
			}			
			command.AppendLine($"GROUP BY {nameof(Category)}.{nameof(Category.PKeyId)}, {nameof(Category)}.{nameof(Category.Id)}, {nameof(Category)}.{nameof(Category.Name)}");
		}

		private async Task<Category> GetCategoryByName(string name)
		{
			var command = new StringBuilder(512);
			command.AppendLine($"SELECT {nameof(Category.PKeyId)}, {nameof(Category.Id)}, {nameof(Category.Name)}");
			command.AppendLine($"FROM {nameof(Category)}");
			command.AppendLine($"WHERE UPPER({nameof(Category.Name)}) = @{nameof(Category.Name)}");
			return await ExecuteQueryFirstOrDefaultAsync<Category>(command.ToString(), new { Name = name.ToUpper() });
		}

		private async Task<IEnumerable<Category>> GetCategoriesByIds(Guid[] ids)
		{
			var command = new StringBuilder(512);
			command.AppendLine($"SELECT {nameof(Category.PKeyId)}, {nameof(Category.Id)}, {nameof(Category.Name)}");
			command.AppendLine($"FROM {nameof(Category)}");
			command.AppendLine($"WHERE {nameof(Category.Id)} IN @Ids");
			return await ExecuteQueryAsync<Category>(command.ToString(), new { Ids = ids });
		}

		public async Task<IPost> GetPostById(Guid id)
		{
			var command = new StringBuilder(512);
			command.AppendLine($"SELECT {nameof(Post)}.*");
			command.AppendLine($"FROM {nameof(Post)}");
			command.AppendLine($"WHERE {nameof(Post.Id)} = @{nameof(Post.Id)} AND");
			AppendPostCommonConditions(command);
			var post = await ExecuteQueryFirstOrDefaultAsync<Post>(command.ToString(), new { Id = id });

			if (post != null)
			{
				await PopulateRelatedComments(post);
				await PopulateRelatedCategories(post);
			}

			return post;
		}

		public async Task<IPost> GetPostBySlug(string slug)
		{
			var command = new StringBuilder(512);
			command.AppendLine($"SELECT {nameof(Post)}.*");
			command.AppendLine($"FROM {nameof(Post)}");
			command.AppendLine($"WHERE UPPER({nameof(Post.Slug)}) = @{nameof(Post.Slug)} AND");
			AppendPostCommonConditions(command);
			var post = await ExecuteQueryFirstOrDefaultAsync<Post>(command.ToString(), new { Slug = slug.ToUpper() });

			if (post != null)
			{
				await PopulateRelatedComments(post);
				await PopulateRelatedCategories(post);
			}

			return post;
		}

		public async Task<IEnumerable<IPost>> GetPosts(int count, int skip = 0)
		{
			var command = new StringBuilder(512);
			command.AppendLine($"SELECT {nameof(Post)}.*");
			command.AppendLine($"FROM {nameof(Post)}");
			command.AppendLine($"WHERE");
			AppendPostCommonConditions(command);
			AppendPostPaging(command);

			var posts = (await ExecuteQueryAsync<Post>(command.ToString(), new
			{
				PubDate = _dateTimeProvider.Now.Date,
				Count = count,
				Skip = skip
			})).ToArray();

			await PopulateRelatedComments(posts);
			await PopulateRelatedCategories(posts);

			return posts;
		}

		public async Task<IEnumerable<IPost>> GetPostsByCategory(Guid categoryId, int count, int skip = 0)
		{
			var command = new StringBuilder(512);
			command.AppendLine($"SELECT {nameof(Post)}.*");
			command.AppendLine($"FROM {nameof(Post)}");
			command.AppendLine("INNER JOIN (");
			command.AppendLine($"	SELECT {nameof(PostCategoryRel.PostPKeyId)} FROM {nameof(PostCategoryRel)}");
			command.AppendLine($"	INNER JOIN {nameof(Category)}");
			command.AppendLine($"	ON {nameof(PostCategoryRel)}.{nameof(PostCategoryRel.CategoryPKeyId)} = {nameof(Category)}.{nameof(Category.PKeyId)}");
			command.AppendLine($"	WHERE {nameof(Category)}.{nameof(Category.Id)} = @CategoryId");
			command.AppendLine($") {nameof(PostCategoryRel)}");
			command.AppendLine($"ON {nameof(PostCategoryRel)}.{nameof(PostCategoryRel.PostPKeyId)} = {nameof(Post)}.{nameof(Post.PKeyId)}");
			command.AppendLine($"WHERE");
			AppendPostCommonConditions(command);
			AppendPostPaging(command);

			var posts = (await ExecuteQueryAsync<Post>(command.ToString(), new
			{
				PubDate = _dateTimeProvider.Now.Date,
				CategoryId = categoryId,
				Count = count,
				Skip = skip
			})).ToArray();

			await PopulateRelatedComments(posts);
			await PopulateRelatedCategories(posts);

			return posts;
		}

		private void AppendPostCommonConditions(StringBuilder command)
		{
			command.AppendLine($"{nameof(Post.IsDeleted)} = 0");
			if (!_userRoleResolver.IsAdmin())
			{
				command.AppendLine($"AND {nameof(Post.PubDate)} <= @{nameof(Post.PubDate)} AND {nameof(Post.IsPublished)} = 1");
			}
		}

		private void AppendPostPaging(StringBuilder command)
		{
			command.AppendLine($"ORDER BY {nameof(Post.PubDate)} DESC");
			command.AppendLine("LIMIT @Count OFFSET @Skip");
		}

		private async Task PopulateRelatedComments(params Post[] posts)
		{
			var postIds = posts.Select(p => p.PKeyId);

			var command = new StringBuilder(512);
			command.AppendLine($"SELECT {nameof(Comment)}.*");
			command.AppendLine($"FROM {nameof(Comment)}");
			command.AppendLine($"WHERE {nameof(Comment.PostPKeyId)} IN @PostIds");

			var comments = (await ExecuteQueryAsync<Comment>(command.ToString(), new { PostIds = postIds })).ToArray();

			foreach (var post in posts)
			{
				post.Comments = comments.Where(c => c.PostPKeyId == post.PKeyId).Cast<IComment>().ToList();
			}
		}

		private async Task PopulateRelatedCategories(params Post[] posts)
		{
			var postIds = posts.Select(p => p.PKeyId);

			var command = new StringBuilder(512);
			command.AppendLine($"SELECT {nameof(Category)}.*, {nameof(PostCategoryRel)}.{nameof(PostCategoryRel.PostPKeyId)}");
			command.AppendLine($"FROM {nameof(Category)}");
			command.AppendLine($"INNER JOIN {nameof(PostCategoryRel)}");
			command.AppendLine($"ON {nameof(Category)}.{nameof(Category.PKeyId)} = {nameof(PostCategoryRel)}.{nameof(PostCategoryRel.CategoryPKeyId)}");
			command.AppendLine($"WHERE {nameof(PostCategoryRel)}.{nameof(PostCategoryRel.PostPKeyId)} IN @PostIds");

			var postCategories = (await ExecuteQueryAsync<PostCategoryRel>(command.ToString(), new { PostIds = postIds })).ToArray();

			foreach (var post in posts)
			{
				post.Categories = postCategories.Where(c => c.PostPKeyId == post.PKeyId).Cast<ICategory>().ToList();
			}
		}
	}
}