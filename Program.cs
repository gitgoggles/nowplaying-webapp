public partial class Program
{
	private static void Main(string[] args)
	{
		var builder = WebApplication.CreateBuilder(args);

		// Add services to the container.
		// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
		builder.Services.AddOpenApi();

		var app = builder.Build();

		// Configure the HTTP request pipeline.
		if (app.Environment.IsDevelopment())
		{
			app.MapOpenApi();
		}

		app.MapGet("/", () =>
		{
			string hot_reload = """<script src="/_framework/aspnetcore-browser-refresh.js"></script>""";
			string html = @"
			<head>
			<style>
			body{margin:40px
			auto;max-width:36em;line-height:1.6;font-size:1.125rem;color:#444;padding:0
			10px}h1,h2,h3{line-height:1.2}
			</style>
			</head>
			<body>
			<header>
			<h1>What is <i>playing</i> right now?</h1>
			<aside>Hopefully I can populate this with the currently playing song from Plex, Jellyfin or Mixxx.</aside>
			</header>
			</body>
		";
			html += hot_reload;
			return Results.Content(html, "text/html");
		});

		app.Run();
	}
}
