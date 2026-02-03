namespace nowplaying_webapp;

public sealed class FetcherRegistry
{
	private readonly HyprlandMixxxFetcher _mixxxFetcher;
	private readonly JellyfinFetcher _jellyfinFetcher;
	private readonly IReadOnlyList<Fetcher> _fetchers;

	public FetcherRegistry(HyprlandMixxxFetcher mixxxFetcher, JellyfinFetcher jellyfinFetcher)
	{
		_mixxxFetcher = mixxxFetcher;
		_jellyfinFetcher = jellyfinFetcher;
		_fetchers = new Fetcher[] { _mixxxFetcher, _jellyfinFetcher };
	}

	public IReadOnlyList<Fetcher> Fetchers => _fetchers;

	public Fetcher? GetFetcher(string name) => name switch
	{
		"hyprland-mixxx" => _mixxxFetcher,
		"jellyfin" => _jellyfinFetcher,
		_ => null
	};
}
