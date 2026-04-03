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

	public override string ToString()
	{
		return Full ?? "null";
	}
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
		// e.g. input: Iron Maiden - Run to the Hills
		// e.g. input: Bad Company
		if (input?.Length > 0 && !input.Contains('-'))
		{
			Artist = null;
			Title = input;
		}
		else
		{
			var split = input?.Split(" - ", 2);
			Artist = split?.Length == 2 ? split[0] : null;
			Title = split?.Length == 2 ? split[1] : null;
		}
	}

}

public sealed class JellyfinNowPlaying : NowPlaying
{

	public JellyfinNowPlaying(string input, string userName)
	{
		var parsed = JsonDocument.Parse(input);

		var nowPlaying = parsed.RootElement
			.EnumerateArray()
			.FirstOrDefault(item => item.GetProperty("UserName").GetString() == userName);

		Artist = nowPlaying.GetProperty("NowPlayingItem").GetProperty("Artists").EnumerateArray().FirstOrDefault().GetString();
		Title = nowPlaying.GetProperty("NowPlayingItem").GetProperty("Name").GetString();
	}
}

public sealed class PlexNowPlaying : NowPlaying
{
	public PlexNowPlaying(string input, string userName)
	{
		var parsed = JsonDocument.Parse(input);

		var nowPlaying = parsed.RootElement
			.GetProperty("MediaContainer")
			.GetProperty("Metadata")
			.EnumerateArray()
			.FirstOrDefault(item =>
					item.GetProperty("User").GetProperty("title").GetString() == $"{userName}"
				  );

		Artist = nowPlaying.GetProperty("grandparentTitle").GetString();
		Title = nowPlaying.GetProperty("title").GetString();
	}

}
