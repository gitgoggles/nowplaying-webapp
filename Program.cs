using nowplaying_webapp.Components;
using nowplaying_webapp.Services;

namespace nowplaying_webapp;

public partial class Program
{
	private static void Main(string[] args)
	{
		var builder = WebApplication.CreateBuilder(args);

		builder.Services.AddRazorComponents()
			.AddInteractiveServerComponents();
		builder.Services.AddMemoryCache();
		builder.Services.AddHttpClient<JellyfinFetcher>(client =>
			client.Timeout = TimeSpan.FromSeconds(5));


		builder.Services.AddSingleton<NowPlayingStore>();
		builder.Services.AddSingleton<Fetcher, HyprlandMixxxFetcher>();
		builder.Services.AddSingleton<Fetcher, JellyfinFetcher>();
		builder.Services.AddSingleton<Fetcher, DemoFetcher>();

		builder.Services.AddHostedService<FetcherPollingService>();

		var app = builder.Build();

		app.UseStaticFiles();
		app.UseAntiforgery();

		app.MapRazorComponents<App>()
			.AddInteractiveServerRenderMode();

		app.Run();
	}
}
