using System.Text.Json;

namespace nowplaying_webapp.Models;

// NowPlaying objects should parse the data
// from the fetcher and return an object.
public abstract class NowPlaying
{
	public string? Artist { get; protected init; }
	public string? Title { get; protected init; }

	public string? Full => (Artist, Title) switch
	{
		(null, null) => null,
		(null, var t) => t,
		(var a, null) => a,
		(var a, var t) => $"{a} - {t}"
	};
	public bool ArtistAndTitleAcquired =>
		!string.IsNullOrWhiteSpace(Artist) && !string.IsNullOrWhiteSpace(Title);
}
public sealed class DemoNowPlaying : NowPlaying
{
	public DemoNowPlaying(string artist, string title)
	{
		Artist = artist;
		Title = title;
	}
}

public sealed class MixxxNowPlaying : NowPlaying
{
	public MixxxNowPlaying(string? input)
	{
		var split = input?.Split(" - ", 2);
		Artist = split?.Length == 2 ? split[0] : null;
		Title = split?.Length == 2 ? split[1] : null;
	}

}

public sealed class JellyfinNowPlaying : NowPlaying
{

	public JellyfinNowPlaying(string input)
	{
		var sessions = JsonSerializer.Deserialize<List<SessionDto>>(input, JsonSerializerOptions.Web);

		var nowPlaying = sessions?
			.FirstOrDefault(s => s.NowPlayingItem != null)?
			.NowPlayingItem;

		var artistName = nowPlaying?.Artists?.FirstOrDefault();
		var name = nowPlaying?.Name;

		Artist = artistName;
		Title = name;
	}

	public sealed record SessionDto
	{
		public NowPlayingItemDto? NowPlayingItem { get; init; }
	}

	public sealed record NowPlayingItemDto
	{
		public string? Name { get; init; }
		public string? Album { get; init; }
		public string[]? Artists { get; init; }
	}
}
