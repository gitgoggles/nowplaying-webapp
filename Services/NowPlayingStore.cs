using System.Collections.Concurrent;
using nowplaying_webapp.Models;

namespace nowplaying_webapp.Services;

public sealed class NowPlayingStore
{
	private readonly ConcurrentDictionary<string, NowPlaying?> _nowPlayingByFetcher = new();

	public ICollection<string> ListFetchers() => _nowPlayingByFetcher.Keys;

	public NowPlaying? Get(string? fetcherName)
	{
		if (fetcherName is not null)
		{
			return _nowPlayingByFetcher.TryGetValue(fetcherName, out var current)
				? current
				: null;
		}

		var realFetcher = _nowPlayingByFetcher
			.Where(x => x.Key != "Demo")
			.Select(x => x.Value)
			.FirstOrDefault(x => x?.ArtistAndTitleAcquired == true);

		if (realFetcher is not null)
		{
			return realFetcher;
		}

		// only return demo data if there is no real data
		return _nowPlayingByFetcher.TryGetValue("Demo", out var value) ? value : null;
	}

	public void Update(string fetcherName, NowPlaying? newValue)
	{
		NowPlaying? currentValue = _nowPlayingByFetcher.TryGetValue(fetcherName, out var value) ? value : null;
		bool changeDetected = currentValue?.Full != newValue?.Full;

		if (changeDetected)
		{
			_nowPlayingByFetcher[fetcherName] = newValue;
		}

		if (changeDetected && (newValue?.Full is not null))
		{
			Console.WriteLine($"{fetcherName}: {newValue.Full}");
		}
	}
}
