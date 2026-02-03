using System.Collections.Concurrent;

namespace nowplaying_webapp;

public sealed class NowPlayingStore
{
	private readonly ConcurrentDictionary<string, NowPlaying?> _nowPlayingByFetcher = new();

	public NowPlaying? Get(string fetcherName)
	{
		return _nowPlayingByFetcher.TryGetValue(fetcherName, out var current)
			? current
			: null;
	}

	public void Update(string fetcherName, NowPlaying? nowPlaying)
	{
		_nowPlayingByFetcher[fetcherName] = nowPlaying;
	}
}
