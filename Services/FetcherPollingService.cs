using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace nowplaying_webapp;

public sealed class FetcherPollingService(
	IServiceProvider serviceProvider,
	NowPlayingStore store,
	ILogger<FetcherPollingService> logger)
	: BackgroundService
{
	private readonly IServiceProvider _serviceProvider = serviceProvider;
	private readonly NowPlayingStore _store = store;
	private readonly ILogger<FetcherPollingService> _logger = logger;

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		using var scope = _serviceProvider.CreateScope();
		var registry = scope.ServiceProvider.GetRequiredService<FetcherRegistry>();
		var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(250));

		try
		{
			while (await timer.WaitForNextTickAsync(stoppingToken))
			{
				foreach (var fetcher in registry.Fetchers)
				{
					try
					{
						var next = await fetcher.GetNowPlayingAsync(stoppingToken);
						_store.Update(fetcher.Name, next);
					}
					catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
					{
						return;
					}
					catch (Exception ex)
					{
						_logger.LogDebug(ex, "Failed polling fetcher {FetcherName}", fetcher.Name);
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
