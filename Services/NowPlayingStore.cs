using System.Collections.Concurrent;
using nowplaying_webapp.Models;

namespace nowplaying_webapp.Services;

public sealed class NowPlayingStore
{
	private readonly ConcurrentDictionary<string, NowPlaying?> _nowPlayingByFetcher = new();
	private NowPlaying? FirstRealFetcher =>
		_nowPlayingByFetcher
			.FirstOrDefault(x => x.Key != "Demo" && x.Value?.Full is not null)
			.Value;
	private bool RealFetcherInStore => FirstRealFetcher is not null;

	public event Action<string, NowPlaying?>? StoreUpdated;

	public NowPlaying? Get(string? fetcherName)
	{
		if (fetcherName is not null)
		{
			return _nowPlayingByFetcher.GetValueOrDefault(fetcherName);
		}

		return FirstRealFetcher ??
			   // only return demo data if there is no real data
			   _nowPlayingByFetcher.GetValueOrDefault("Demo");
	}

	public void Update(string fetcherName, NowPlaying? newValue)
	{
		NowPlaying? currentValue = _nowPlayingByFetcher.GetValueOrDefault(fetcherName);
		bool changeDetected = currentValue?.Full != newValue?.Full;

		if (!changeDetected)
		{
			return;
		}

		// real data
		if (fetcherName != "Demo")
		{
			_nowPlayingByFetcher[fetcherName] = newValue;
			Console.WriteLine($"{fetcherName}: {newValue}");
			StoreUpdated?.Invoke(fetcherName, newValue);
			return;
		}

		// demo data
		if (fetcherName == "Demo" && !RealFetcherInStore)
		{
			_nowPlayingByFetcher[fetcherName] = newValue;
			StoreUpdated?.Invoke(fetcherName, newValue);
			return;
		}

	}
}
