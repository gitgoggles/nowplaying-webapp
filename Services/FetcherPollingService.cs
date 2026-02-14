namespace nowplaying_webapp.Services;

public sealed class FetcherPollingService(
	IEnumerable<Fetcher> fetchers,
	NowPlayingStore store,
	ILogger<FetcherPollingService> logger)
	: BackgroundService
{
	private readonly IReadOnlyList<Fetcher> _fetcherList = [.. fetchers];

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(250));

		try
		{
			while (await timer.WaitForNextTickAsync(stoppingToken))
			{
				foreach (var fetcher in _fetcherList)
				{
					try
					{
						var next = await fetcher.GetNowPlayingAsync(stoppingToken);
						store.Update(fetcher.Name, next);
					}
					catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
					{
						return;
					}
					catch (Exception ex)
					{
						if (logger.IsEnabled(LogLevel.Debug))
						{
							logger.LogDebug(ex, "Failed polling fetcher {FetcherName}", fetcher.Name);
						}
					}
				}
			}
		}
		finally
		{
			timer.Dispose();
		}
	}
}
