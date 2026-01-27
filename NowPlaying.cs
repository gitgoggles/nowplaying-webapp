using System.Text.Json;

namespace nowplaying_webapp;

public abstract class NowPlaying
{
	public string? Artist { get; protected init; }
	public string? Title { get; protected init; }
	public string? Full { get; protected init; } = string.Empty;
	public bool ArtistAndTitleAcquired =>
		!string.IsNullOrWhiteSpace(Artist) && !string.IsNullOrWhiteSpace(Title);
}

public sealed class MixxxNowPlaying : NowPlaying
{
	public MixxxNowPlaying(string? input)
	{
		var split = input?.Split(" - ");
		Artist = split?.Length == 2 ? split[0] : null;
		Title = split?.Length == 2 ? split[1] : null;
		Full = input;
	}

}

public sealed class JellyfinNowPlaying : NowPlaying
{
	static readonly JsonSerializerOptions Options = new() { PropertyNameCaseInsensitive = true };

	public JellyfinNowPlaying(string input)
	{
		var sessions = JsonSerializer.Deserialize<List<SessionDto>>(input, Options);

		var nowPlaying = sessions?
			.FirstOrDefault(s => s.NowPlayingItem != null)?
			.NowPlayingItem;

		var artistName = nowPlaying?.Artists?.FirstOrDefault();
		var name = nowPlaying?.Name;

		Artist = artistName;
		Title = name;
		Full = (artistName, name) switch
		{
			(null, null) => string.Empty,
			(null, var t) => t,
			(var a, null) => a,
			(var a, var t) => $"{a} - {t}"
		};
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
