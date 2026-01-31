namespace nowplaying_webapp;

public sealed class FetcherRegistry
{
	private readonly HyprlandMixxxFetcher _mixxxFetcher;
	private readonly JellyfinFetcher _jellyfinFetcher;

	public FetcherRegistry(HyprlandMixxxFetcher mixxxFetcher, JellyfinFetcher jellyfinFetcher)
	{
		_mixxxFetcher = mixxxFetcher;
		_jellyfinFetcher = jellyfinFetcher;
	}

	public Fetcher? GetFetcher(string name) => name switch
	{
		"hyprland-mixxx" => _mixxxFetcher,
		"jellyfin" => _jellyfinFetcher,
		_ => null
	};
}
