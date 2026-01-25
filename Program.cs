using System.Net.ServerSentEvents;

namespace nowplaying_webapp;

public partial class Program
{
	private static void Main(string[] args)
	{
		var builder = WebApplication.CreateBuilder(args);

		// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
		builder.Services.AddOpenApi();

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
				<script defer src=""/only_swap_on_change.js""></script>
				<link rel=""preconnect"" href=""https://fonts.googleapis.com"">
				<link rel=""preconnect"" href=""https://fonts.gstatic.com"" crossorigin>
				<link href=""https://fonts.googleapis.com/css2?family=Permanent+Marker&display=swap"" rel=""stylesheet"">
				<link rel=""stylesheet"" href=""/style.css"">
			</head>
			";

		app.MapGet("/", () =>
		{
			string html = $@"
			{commonHead}
			<body class=""main"">
				<header>
				<h1>What is <i>playing</i> right now?</h1>
				<aside>Hopefully I can populate this with the currently playing song from Plex, Jellyfin or Mixxx.</aside>
				</header>
				<p>Mixxx: <span hx-get=""/hyprland-mixxx/full"" hx-trigger=""load, every 500ms""></span></p>
				<div hx-get=""/animated-sse"" hx-trigger=""load""></div>
			</body>
		";
			return Results.Content(html, "text/html");
		});

		app.MapGet("/{fetcher}/card", (string fetcher) =>
				{
					var nowplaying = fetcher switch
					{
						"hyprland-mixxx" => new HyprlandMixxxFetcher().GetNowPlaying(),
						_ => null
					};

					var html = nowplaying?.artistAndTitleAquired switch
					{
						true => $@"
					<div id=""card"">
						<div id=""title"">{nowplaying?.title}</div>
						<div id=""artist"">{nowplaying?.artist}</div>
					</div>
					",
						false => $@"
					<div id=""card"">
						<div id=""title"">{nowplaying?.full}</div>
					</div>
					",
						_ => null
					};

					return Results.Content(html ?? "", "text/html");
				});

		app.MapGet("/{fetcher}/{field}", (string fetcher, string field) =>
				{
					var nowplaying = fetcher switch
					{
						"hyprland-mixxx" => new HyprlandMixxxFetcher().GetNowPlaying(),
						_ => null
					};

					var html = field switch
					{
						"artist" => nowplaying?.artist,
						"title" => nowplaying?.title,
						"full" => nowplaying?.full,
						_ => null
					};
					return Results.Content(html ?? "", "text/html");
				});

		app.MapGet("/animated", () =>
				{
					var winning_fetcher = "hyprland-mixxx";
					var html = $@"
					{commonHead}
					<div id=""card"" hx-get=""/{winning_fetcher}/card"" hx-trigger=""load, every 2s"" hx-swap=""settle:1s"">
					</div>
					";
					return Results.Content(html, "text/html");
				});

		app.MapGet("/animated-sse", () =>
				{
					var winning_fetcher = "hyprland-mixxx";
					var html = $@"
					{commonHead}
					<div id=""card"" hx-ext=""sse"" sse-connect=""/{winning_fetcher}/card-sse"" sse-swap=""newNowPlaying"" hx-swap=""settle:3s"">
					</div>
					";
					return Results.Content(html, "text/html");
				});

		app.MapGet("/{fetcher}/card-sse", (string fetcher, CancellationToken ct) =>
				{
					async IAsyncEnumerable<SseItem<string>> Stream()
					{
						string currentHtml = "";
						while (!ct.IsCancellationRequested)
						{

							var nowplaying = fetcher switch
							{
								"hyprland-mixxx" => new HyprlandMixxxFetcher().GetNowPlaying(),
								_ => null
							};

							var newHtml = nowplaying?.artistAndTitleAquired switch
							{
								true => $@"
									<div id=""card"">
										<div id=""title"">{nowplaying?.title}</div>
										<div id=""artist"">{nowplaying?.artist}</div>
									</div>
									",
								false => $@"
									<div id=""card"">
										<div id=""title"">{nowplaying?.full}</div>
									</div>
									",
								_ => null
							};

							if (!string.Equals(currentHtml, newHtml, StringComparison.Ordinal))
							{
								yield return new SseItem<string>(newHtml, eventType: "newNowPlaying")
								{
								};
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
