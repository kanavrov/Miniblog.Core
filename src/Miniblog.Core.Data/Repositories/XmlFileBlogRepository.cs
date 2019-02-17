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

		private const string CATEGORIES = "categories";

		private const string CATEGORIES_FILE = "categories.xml";

		private const string FILES = "files";

		private readonly List<IPost> _cache = new List<IPost>();

		private readonly List<ICategory> _categoryCache = new List<ICategory>();

		private readonly string _postFolder;

		private readonly string _categoryFolder;

		private readonly IUserRoleResolver _userRoleResolver;

		public XmlFileBlogRepository(string webRootPath, IUserRoleResolver userRoleResolver)
		{
			_postFolder = Path.Combine(webRootPath, POSTS);
			_categoryFolder = Path.Combine(_postFolder, CATEGORIES);

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

			var filePath = GetFilePath(post);

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
			var isAdmin = _userRoleResolver.IsAdmin();

			if(isAdmin)
				return Task.FromResult(_categoryCache.AsEnumerable());

			var categories = _cache
				.Where(p => p.IsPublished)
				.SelectMany(post => post.Categories)
				.GroupBy(category => category.Id)
  				.Select(group => group.First());

			return Task.FromResult(categories);
		}

		public virtual async Task<ICategory> GetCategoryById(Guid id)
		{
			var categories = await GetCategories();
			return _categoryCache.FirstOrDefault(c => c.Id == id);
		}

		public virtual Task<IPost> GetPostById(Guid id)
		{
			var post = _cache.FirstOrDefault(p => p.Id == id);
			var isAdmin = _userRoleResolver.IsAdmin();

			if (post != null && post.PubDate <= DateTime.UtcNow && (post.IsPublished || isAdmin))
			{
				return Task.FromResult(post);
			}

			return Task.FromResult<IPost>(null);
		}

		public virtual Task<IPost> GetPostBySlug(string slug)
		{
			var post = _cache.FirstOrDefault(p => p.Slug.Equals(slug, StringComparison.OrdinalIgnoreCase));
			var isAdmin = _userRoleResolver.IsAdmin();

			if (post != null && post.PubDate <= DateTime.UtcNow && (post.IsPublished || isAdmin))
			{
				return Task.FromResult(post);
			}

			return Task.FromResult<IPost>(null);
		}

		public virtual Task<IEnumerable<IPost>> GetPosts(int count, int skip = 0)
		{
			var isAdmin = _userRoleResolver.IsAdmin();

			var posts = _cache
				.Where(p => p.PubDate <= DateTime.UtcNow && (p.IsPublished || isAdmin))
				.Skip(skip)
				.Take(count);

			return Task.FromResult(posts);
		}

		public virtual Task<IEnumerable<IPost>> GetPostsByCategory(Guid categoryId, int count, int skip = 0)
		{
			var isAdmin = _userRoleResolver.IsAdmin();
			var posts = _cache
				.Where(p => p.PubDate <= DateTime.UtcNow && (p.IsPublished || isAdmin))
				.Where(p => p.Categories.Any(c => c.Id == categoryId))
				.Skip(skip)
				.Take(count);			

			return Task.FromResult(posts);
		}

		private async Task SavePost(IPost post)
		{
			var filePath = GetFilePath(post);
			post.LastModified = DateTime.UtcNow;

			var doc = new XDocument(
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

			var categories = doc.XPathSelectElement("post/categories");
			var existingCategories = new List<ICategory>();
			foreach (var category in post.Categories)
			{
				var existingCategory = _categoryCache.FirstOrDefault(c => c.Id == category.Id);
				if(existingCategory != null)
				{
					categories.Add(
					new XElement("category", 
						new XAttribute("id", category.Id)
					));
					existingCategories.Add(existingCategory);
				}				
			}
			post.Categories = existingCategories;

			var comments = doc.XPathSelectElement("post/comments");
			foreach (var comment in post.Comments)
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
			LoadCategories();
			LoadPosts();
			SortCache();
		}

		private void LoadCategories()
		{
			if (!Directory.Exists(_categoryFolder))
				Directory.CreateDirectory(_categoryFolder);

			var file = Path.Combine(_categoryFolder, CATEGORIES_FILE);

			if(!File.Exists(file))
				return;

			var doc = XElement.Load(file);

			foreach (var node in doc.Elements("category"))
			{
				var category = new Category() 
				{
					Id = Guid.Parse(ReadAttribute(node, "id")),
					Name = node.Value
				};
				_categoryCache.Add(category);				
			}
		}

		private void LoadPosts()
		{
			if (!Directory.Exists(_postFolder))
				Directory.CreateDirectory(_postFolder);

			// Can this be done in parallel to speed it up?
			foreach (var file in Directory.EnumerateFiles(_postFolder, "*.xml", SearchOption.TopDirectoryOnly))
			{
				var doc = XElement.Load(file);

				var post = new Post
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

				LoadCategoriesForPost(post, doc);
				LoadComments(post, doc);
				_cache.Add(post);
			}
		}

		private void LoadCategoriesForPost(IPost post, XElement doc)
		{
			var categories = doc.Element("categories");
			post.Categories = new List<ICategory>();

			if (categories == null)
				return;			

			foreach (var node in categories.Elements("category"))
			{
				var id = Guid.Parse(ReadAttribute(node, "id"));
				var category = _categoryCache.FirstOrDefault(c => c.Id == id);

				if(category != null)
					post.Categories.Add(category);				
			}
		}

		private static void LoadComments(IPost post, XElement doc)
		{
			var commentsElement = doc.Element("comments");
			post.Comments = new List<IComment>();

			if (commentsElement == null)
				return;
			
			foreach (var node in commentsElement.Elements("comment"))
			{
				var comment = new Comment()
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
			return Path.Combine(_postFolder, post.Id + ".xml");
		}

		protected void SortCache()
		{
			_cache.Sort((p1, p2) => p2.PubDate.CompareTo(p1.PubDate));
			_categoryCache.Sort((c1, c2) => c1.Name.CompareTo(c2.Name));
		}

		private static string FormatDateTime(DateTime dateTime)
		{
			const string UTC = "yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'";

			return dateTime.Kind == DateTimeKind.Utc
				? dateTime.ToString(UTC)
				: dateTime.ToUniversalTime().ToString(UTC);
		}

		public virtual async Task AddCategory(ICategory category)
		{
			if(!_categoryCache.Any(c => c.Id == category.Id))
			{
				_categoryCache.Add(category);
				await SaveCategories();
			}				
		}

		public virtual async Task UpdateCategory(ICategory category)
		{
			var existingCategory = _categoryCache.FirstOrDefault(c => c.Id == category.Id);
			if(existingCategory != null)
			{
				existingCategory.Name = category.Name;
				await SaveCategories();
			}				
		}

		public virtual async Task DeleteCategory(Guid categoryId)
		{
			if(_categoryCache.Any(c => c.Id == categoryId))
			{
				_categoryCache.RemoveAll(c => c.Id == categoryId);
				await SaveCategories();
			}	
			
		}

		private async Task SaveCategories()
		{
			var filePath = Path.Combine(_categoryFolder, CATEGORIES_FILE);

			var doc = new XDocument(
							new XElement("categories",
								new XAttribute("lastModified", FormatDateTime(DateTime.UtcNow)),
								string.Empty
							));

			var categories = doc.XPathSelectElement("categories");
			foreach (var category in _categoryCache)
			{
				categories.Add(
					new XElement("category", 
						new XAttribute("id", category.Id),
						category.Name
					));	
			}

			using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.ReadWrite))
			{
				await doc.SaveAsync(fs, SaveOptions.None, CancellationToken.None).ConfigureAwait(false);
			}

			SortCache();
		}
	}
}