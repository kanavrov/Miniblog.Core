namespace Miniblog.Core.Service.Rendering
{
	public class YouTubeEmbedSettings : IYouTubeEmbedSettings
    {
		public int Width { get; set; } = 560;

		public int Height { get; set; } = 315;

		public bool AllowFullscreen { get; set; } = true;

		public string Title { get; set; } = "YouTube embed";

		public string EmbedUrlTemplate { get; set; } = "https://www.youtube-nocookie.com/embed/{0}?modestbranding=1&hd=1&rel=0";
	}
}