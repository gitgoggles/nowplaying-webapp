using System.Collections.Concurrent;
using nowplaying_webapp.Models;

namespace nowplaying_webapp.Services;

public sealed class NowPlayingStore
{
	private readonly ConcurrentDictionary<string, NowPlaying?> _nowPlayingByFetcher = new();

	public ICollection<string> ListFetchers()
	{
		return _nowPlayingByFetcher.Keys;
	}

	public NowPlaying? Get(string? fetcherName)
	{
		if (fetcherName is null)
		{
			return _nowPlayingByFetcher.FirstOrDefault(x => x.Value?.ArtistAndTitleAcquired == true).Value;
		}

		return _nowPlayingByFetcher.TryGetValue(fetcherName, out var current)
			? current
			: null;
	}

	public void Update(string fetcherName, NowPlaying? nowPlaying)
	{
		NowPlaying? existing = Get(fetcherName);
		NowPlaying? next = nowPlaying;

		if (existing?.Full != next?.Full)
		{
			_nowPlayingByFetcher[fetcherName] = nowPlaying;
		}
		if ((existing?.Full != next?.Full) && (next?.Full is not null))
		{
			Console.WriteLine($"{fetcherName}: {nowPlaying?.Full}");

		}
	}
}
