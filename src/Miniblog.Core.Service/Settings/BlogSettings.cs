namespace Miniblog.Core.Service.Settings
{
	public class BlogSettings
	{
		public string Owner { get; set; } = "The Owner";
		public int PostsPerPage { get; set; } = 2;
		public int CommentsCloseAfterDays { get; set; } = 10;
		public StorageType StorageType { get; set; } = StorageType.XML;
		public PostRenderType PostRenderType { get; set; } = PostRenderType.Html;
		public string ConnectionStringName { get; set; }
		public int ImageSizeLimitMegabytes { get; set; }
	}
}
