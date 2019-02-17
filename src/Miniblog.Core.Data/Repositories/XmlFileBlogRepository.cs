using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;
using Miniblog.Core.Contract.Models;
using Miniblog.Core.Data.Models;
using Miniblog.Core.Framework.Users;

namespace Miniblog.Core.Data.Repositories
{
	public class XmlFileBlogRepository : IBlogRepository
	{
		private const string POSTS = "Posts";
		private const string FILES = "files";

		private readonly List<IPost> _cache = new List<IPost>();

		private readonly string _folder;
		private readonly IUserRoleResolver _userRoleResolver;

		public XmlFileBlogRepository(string webRootPath, IUserRoleResolver userRoleResolver)
		{
			_folder = Path.Combine(webRootPath, POSTS);
			_userRoleResolver = userRoleResolver;
			Initialize();
		}
		public async Task AddComment(IComment comment, Guid postId)
		{
			var post = await GetPostById(postId);
			if (post != null)
			{
				post.Comments.Add(comment);
				await SavePost(post);
			}
		}

		public async Task DeleteComment(Guid commentId, Guid postId)
		{
			var post = await GetPostById(postId);
			if (post != null)
			{
				var comment = post.Comments.FirstOrDefault(c => c.Id == commentId);
				if (comment != null)
				{
					post.Comments.Remove(comment);
					await SavePost(post);
				}
			}
		}

		public async Task AddPost(IPost post)
		{
			await SavePost(post);
		}

		public async Task UpdatePost(IPost post)
		{
			await SavePost(post);
		}

		public async Task DeletePost(Guid id)
		{
			var post = await GetPostById(id);

			if (post == null)
			{
				return;
			}

			string filePath = GetFilePath(post);

			if (File.Exists(filePath))
			{
				File.Delete(filePath);
			}

			if (_cache.Contains(post))
			{
				_cache.Remove(post);
			}
		}

		public virtual Task<IEnumerable<ICategory>> GetCategories()
		{
			bool isAdmin = _userRoleResolver.IsAdmin();

			var categories = _cache
				.Where(p => p.IsPublished || isAdmin)
				.SelectMany(post => post.Categories)
				.GroupBy(category => category.Id)
  				.Select(group => group.First());

			return Task.FromResult(categories);
		}

		public virtual async Task<ICategory> GetCategoryById(Guid id)
		{
			var categories = await GetCategories();
			return categories.FirstOrDefault(c => c.Id == id);
		}

		public virtual Task<IPost> GetPostById(Guid id)
		{
			var post = _cache.FirstOrDefault(p => p.Id == id);
			bool isAdmin = _userRoleResolver.IsAdmin();

			if (post != null && post.PubDate <= DateTime.UtcNow && (post.IsPublished || isAdmin))
			{
				return Task.FromResult(post);
			}

			return Task.FromResult<IPost>(null);
		}

		public virtual Task<IPost> GetPostBySlug(string slug)
		{
			var post = _cache.FirstOrDefault(p => p.Slug.Equals(slug, StringComparison.OrdinalIgnoreCase));
			bool isAdmin = _userRoleResolver.IsAdmin();

			if (post != null && post.PubDate <= DateTime.UtcNow && (post.IsPublished || isAdmin))
			{
				return Task.FromResult(post);
			}

			return Task.FromResult<IPost>(null);
		}

		public virtual Task<IEnumerable<IPost>> GetPosts(int count, int skip = 0)
		{
			bool isAdmin = _userRoleResolver.IsAdmin();

			var posts = _cache
				.Where(p => p.PubDate <= DateTime.UtcNow && (p.IsPublished || isAdmin))
				.Skip(skip)
				.Take(count);

			return Task.FromResult(posts);
		}

		public virtual Task<IEnumerable<IPost>> GetPostsByCategory(Guid categoryId, int count, int skip = 0)
		{
			bool isAdmin = _userRoleResolver.IsAdmin();
			var posts = _cache
				.Where(p => p.PubDate <= DateTime.UtcNow && (p.IsPublished || isAdmin))
				.Where(p => p.Categories.Any(c => c.Id == categoryId))
				.Skip(skip)
				.Take(count);			

			return Task.FromResult(posts);
		}

		private async Task SavePost(IPost post)
		{
			string filePath = GetFilePath(post);
			post.LastModified = DateTime.UtcNow;

			XDocument doc = new XDocument(
							new XElement("post",
								new XElement("title", post.Title),
								new XElement("slug", post.Slug),
								new XElement("pubDate", FormatDateTime(post.PubDate)),
								new XElement("lastModified", FormatDateTime(post.LastModified)),
								new XElement("excerpt", post.Excerpt),
								new XElement("content", post.Content),
								new XElement("ispublished", post.IsPublished),
								new XElement("categories", string.Empty),
								new XElement("comments", string.Empty)
							));

			XElement categories = doc.XPathSelectElement("post/categories");
			foreach (ICategory category in post.Categories)
			{
				categories.Add(
					new XElement("category", 
						new XElement("name", category.Name),
						new XAttribute("id", category.Id)
					));
			}

			XElement comments = doc.XPathSelectElement("post/comments");
			foreach (Comment comment in post.Comments)
			{
				comments.Add(
					new XElement("comment",
						new XElement("author", comment.Author),
						new XElement("email", comment.Email),
						new XElement("date", FormatDateTime(comment.PubDate)),
						new XElement("content", comment.Content),
						new XAttribute("isAdmin", comment.IsAdmin),
						new XAttribute("id", comment.Id)
					));
			}

			using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.ReadWrite))
			{
				await doc.SaveAsync(fs, SaveOptions.None, CancellationToken.None).ConfigureAwait(false);
			}

			if (!_cache.Contains(post))
			{
				_cache.Add(post);
				SortCache();
			}
		}

		private void Initialize()
		{
			LoadPosts();
			SortCache();
		}

		private void LoadPosts()
		{
			if (!Directory.Exists(_folder))
				Directory.CreateDirectory(_folder);

			// Can this be done in parallel to speed it up?
			foreach (string file in Directory.EnumerateFiles(_folder, "*.xml", SearchOption.TopDirectoryOnly))
			{
				XElement doc = XElement.Load(file);

				Post post = new Post
				{
					Id = Guid.Parse(Path.GetFileNameWithoutExtension(file)),
					Title = ReadValue(doc, "title"),
					Excerpt = ReadValue(doc, "excerpt"),
					Content = ReadValue(doc, "content"),
					Slug = ReadValue(doc, "slug").ToLowerInvariant(),
					PubDate = DateTime.Parse(ReadValue(doc, "pubDate")),
					LastModified = DateTime.Parse(ReadValue(doc, "lastModified", DateTime.UtcNow.ToString(CultureInfo.InvariantCulture))),
					IsPublished = bool.Parse(ReadValue(doc, "ispublished", "true")),
				};

				LoadCategories(post, doc);
				LoadComments(post, doc);
				_cache.Add(post);
			}
		}

		private static void LoadCategories(IPost post, XElement doc)
		{
			XElement categories = doc.Element("categories");
			if (categories == null)
				return;

			List<ICategory> list = new List<ICategory>();

			foreach (var node in categories.Elements("category"))
			{
				ICategory category = new Category() 
				{
					Id = Guid.Parse(ReadAttribute(node, "id")),
					Name = ReadValue(node, "name")
				};
				list.Add(category);
			}

			post.Categories = list.ToArray();
		}

		private static void LoadComments(IPost post, XElement doc)
		{
			var comments = doc.Element("comments");

			if (comments == null)
				return;

			foreach (var node in comments.Elements("comment"))
			{
				IComment comment = new Comment()
				{
					Id = Guid.Parse(ReadAttribute(node, "id")),
					Author = ReadValue(node, "author"),
					Email = ReadValue(node, "email"),
					IsAdmin = bool.Parse(ReadAttribute(node, "isAdmin", "false")),
					Content = ReadValue(node, "content"),
					PubDate = DateTime.Parse(ReadValue(node, "date", "2000-01-01")),
				};

				post.Comments.Add(comment);
			}
		}

		private static string ReadValue(XElement doc, XName name, string defaultValue = "")
		{
			if (doc.Element(name) != null)
				return doc.Element(name)?.Value;

			return defaultValue;
		}

		private static string ReadAttribute(XElement element, XName name, string defaultValue = "")
		{
			if (element.Attribute(name) != null)
				return element.Attribute(name)?.Value;

			return defaultValue;
		}
		private string GetFilePath(IPost post)
		{
			return Path.Combine(_folder, post.Id + ".xml");
		}

		protected void SortCache()
		{
			_cache.Sort((p1, p2) => p2.PubDate.CompareTo(p1.PubDate));
		}

		private static string FormatDateTime(DateTime dateTime)
		{
			const string UTC = "yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'";

			return dateTime.Kind == DateTimeKind.Utc
				? dateTime.ToString(UTC)
				: dateTime.ToUniversalTime().ToString(UTC);
		}		
	}
}