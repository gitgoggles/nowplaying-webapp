using nowplaying_webapp.Components;

namespace nowplaying_webapp;

public partial class Program
{
	private static void Main(string[] args)
	{
		var builder = WebApplication.CreateBuilder(args);

		builder.Services.AddRazorComponents()
			.AddInteractiveServerComponents();
		builder.Services.AddMemoryCache();
		builder.Services.AddSingleton<HyprlandMixxxFetcher>();
		builder.Services.AddHttpClient<JellyfinFetcher>(client =>
			client.Timeout = TimeSpan.FromSeconds(5));
		builder.Services.AddScoped<FetcherRegistry>();
		builder.Services.AddSingleton<NowPlayingStore>();
		builder.Services.AddHostedService<FetcherPollingService>();

		var app = builder.Build();

		app.UseStaticFiles();
		app.UseAntiforgery();

		app.MapRazorComponents<App>()
			.AddInteractiveServerRenderMode();

		app.Run();
	}
}
