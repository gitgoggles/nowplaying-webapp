using System.Collections.Concurrent;
using nowplaying_webapp.Models;

namespace nowplaying_webapp.Services;

public sealed class NowPlayingStore
{
	private readonly ConcurrentDictionary<string, NowPlaying?> _nowPlayingByFetcher = new();

	// public ICollection<string> ListFetchers() => _nowPlayingByFetcher.Keys;
	public event Action<string, NowPlaying?>? StoreUpdated;

	public NowPlaying? Get(string? fetcherName)
	{
		if (fetcherName is not null)
		{
			return _nowPlayingByFetcher.GetValueOrDefault(fetcherName);
		}

		var realFetcher = _nowPlayingByFetcher
			.Where(x => x.Key != "Demo")
			.Select(x => x.Value)
			.FirstOrDefault(x => x?.Full is not null);

		return realFetcher ??
			   // only return demo data if there is no real data
			   _nowPlayingByFetcher.GetValueOrDefault("Demo");
	}

	public void Update(string fetcherName, NowPlaying? newValue)
	{
		NowPlaying? currentValue = _nowPlayingByFetcher.GetValueOrDefault(fetcherName);
		bool changeDetected = currentValue?.Full != newValue?.Full;

		if (changeDetected)
		{
			_nowPlayingByFetcher[fetcherName] = newValue;
			StoreUpdated?.Invoke(fetcherName, newValue);
		}

		if (changeDetected && (newValue?.Full is not null))
		{
			Console.WriteLine($"{fetcherName}: {newValue.Full}");
		}
	}
}
