using System.Net.ServerSentEvents;

namespace nowplaying_webapp;

public partial class Program
{
	private static void Main(string[] args)
	{
		var builder = WebApplication.CreateBuilder(args);

		builder.Services.AddOpenApi();
		builder.Services.AddMemoryCache();

		builder.Services.AddHttpClient<JellyfinFetcher>(client =>
				client.Timeout = TimeSpan.FromSeconds(5)
				);

		var app = builder.Build();

		if (app.Environment.IsDevelopment())
		{
			app.MapOpenApi();
		}

		app.UseStaticFiles();
		string commonHead = @"
			<!DOCTYPE html>
			<head>
				<script defer src=""/htmx.2.0.8.min.js""></script>
				<script defer src=""/htmx-ext-sse@2.2.4.js""></script>
				<link rel=""preconnect"" href=""https://fonts.googleapis.com"">
				<link rel=""preconnect"" href=""https://fonts.gstatic.com"" crossorigin>
				<link href=""https://fonts.googleapis.com/css2?family=Permanent+Marker&display=swap"" rel=""stylesheet"">
				<link rel=""stylesheet"" href=""/style.css"">
			</head>
			";

		app.MapGet("/", () =>
		{
			string html = $"""
			{commonHead}
			<body class="main">
				<header>
				<h1>What is <i>playing</i> right now?</h1>
				<aside>Hopefully I can populate this with the currently playing song from Plex, Jellyfin or Mixxx.</aside>
				</header>
				<ul>
				<li>Mixxx: <span hx-ext="sse" sse-connect="/hyprland-mixxx/full-sse" sse-swap="newNowPlayingField"></span></li>
				<li>Jellyfin: <span hx-ext="sse" sse-connect="/jellyfin/full-sse" sse-swap="newNowPlayingField"></span></li>
				</ul>
				<div hx-get="/animated-sse" hx-trigger="load"></div>
			</body>
		""";
			return Results.Content(html, "text/html");
		});

		app.MapGet("/{fetcher}/{field}-sse", (string fetcher, string field, CancellationToken ct) =>
				{
					async IAsyncEnumerable<SseItem<string>> Stream()
					{
						string? currentHtml = null;

						Fetcher? fetcherInstance = fetcher switch
						{
							"hyprland-mixxx" => new HyprlandMixxxFetcher(),
							"jellyfin" => app.Services.GetRequiredService<JellyfinFetcher>(),
							_ => null
						};

						if (fetcherInstance is null)
						{
							yield break;
						}

						while (!ct.IsCancellationRequested)
						{
							var nowplaying = await fetcherInstance.GetNowPlayingAsync(ct);

							var newHtml = field switch
							{
								"artist" => nowplaying?.Artist,
								"title" => nowplaying?.Title,
								"full" => nowplaying?.Full,
								_ => null
							};

							if (currentHtml != newHtml)
							{
								yield return new SseItem<string>(newHtml ?? "", eventType: "newNowPlayingField");
								currentHtml = newHtml;
							}

							await Task.Delay(250, ct);

						}
					}
					return TypedResults.ServerSentEvents(Stream());
				});

		app.MapGet("/animated-sse", () =>
				{
					var winning_fetcher = "jellyfin";
					// var winning_fetcher = "hyprland-mixxx";
					var html = $"""
					{commonHead}
					<div hx-ext="sse" sse-connect="/{winning_fetcher}/card-sse" sse-swap="newNowPlaying" hx-swap="settle:3s">
					</div>
					""";
					return Results.Content(html, "text/html");
				});

		app.MapGet("/{fetcher}/card-sse", (string fetcher, CancellationToken ct) =>
				{
					async IAsyncEnumerable<SseItem<string>> Stream()
					{
						string? currentHtml = null;

						Fetcher? fetcherInstance = fetcher switch
						{
							"hyprland-mixxx" => new HyprlandMixxxFetcher(),
							"jellyfin" => app.Services.GetRequiredService<JellyfinFetcher>(),
							_ => null
						};

						if (fetcherInstance is null)
						{
							yield break;
						}

						while (!ct.IsCancellationRequested)
						{
							var nowplaying = await fetcherInstance.GetNowPlayingAsync(ct);

							var newHtml = nowplaying?.ArtistAndTitleAcquired switch
							{
								true => $"""
									<div id="card">
										<div id="title">{nowplaying.Title}</div>
										<div id="artist">{nowplaying.Artist}</div>
									</div>
									""",
								false => $"""
									<div id="card">
										<div id="title">{nowplaying.Full}</div>
									</div>
									""",
								_ => null
							};

							if (currentHtml != newHtml)
							{
								yield return new SseItem<string>(newHtml ?? "", eventType: "newNowPlaying");
								currentHtml = newHtml;
							}

							await Task.Delay(250, ct);
						}
					}

					return TypedResults.ServerSentEvents(Stream());

				});

		app.Run();
	}
}
