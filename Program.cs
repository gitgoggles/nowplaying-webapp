using Microsoft.EntityFrameworkCore;
using nowplaying_webapp.Components;
using nowplaying_webapp.Models;
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
		builder.Services.AddDbContext<AppDbContext>();

		var app = builder.Build();

		using (var scope = app.Services.CreateScope())
		{
			var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
			db.Database.Migrate();
		}

		app.UseStaticFiles();
		app.UseAntiforgery();

		app.MapRazorComponents<App>()
			.AddInteractiveServerRenderMode();

		app.Run();
	}
}
