namespace Miniblog.Core.Service.Rendering
{
	public interface IYouTubeEmbedSettings
	{
		int Width { get; }

		int Height { get; }

		string Title { get; }

		bool AllowFullscreen { get; }

		string EmbedUrlTemplate { get; }
	}
}