using System.Diagnostics;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Caching.Memory;

namespace nowplaying_webapp;

// The fetchers are for obtaining the data ONLY.
// Parsing of the data is done by the NowPlaying class.
public abstract class Fetcher
{
	public abstract string Name { get; }

	protected async Task<string> RunProcess(string file, string args, CancellationToken ct)
	{
		var psi = new ProcessStartInfo(file, args)
		{
			RedirectStandardOutput = true,
			UseShellExecute = false,
		};

		using var p = Process.Start(psi) ?? throw new InvalidOperationException($"Failed to start process: {file}");
		try
		{
			var output = await p.StandardOutput.ReadToEndAsync(ct);
			await p.WaitForExitAsync(ct);
			return output;
		}
		catch (TaskCanceledException)
		{
			if (!p.HasExited)
			{
				p.Kill(entireProcessTree: true);
			}

			throw;
		}
	}

	public abstract Task<NowPlaying?> GetNowPlayingAsync(CancellationToken ct = default);
}

// this is only partial to satisfy the regex generation
public sealed partial class HyprlandMixxxFetcher : Fetcher
{
	public override string Name => "hyprland-mixxx";

	[GeneratedRegex(@"title:\s*(?<t>.*?)\s*\|\s*Mixxx\s*$", RegexOptions.Multiline)]
	private static partial Regex MixxxTitleRegex();

	public override async Task<NowPlaying?> GetNowPlayingAsync(CancellationToken ct = default)
	{
		var hyprctlOutput = await RunProcess("hyprctl", "clients", ct);

		var m = MixxxTitleRegex().Match(hyprctlOutput);
		var parsed = m.Success ? m.Groups["t"].Value : null;

		return new MixxxNowPlaying(parsed);
	}
}

public sealed class JellyfinFetcher(IMemoryCache cache, HttpClient http) : Fetcher
{
	private readonly IMemoryCache _cache = cache;
	private readonly HttpClient _http = http;

	public override string Name => "jellyfin";

	private readonly string? _jellyfinUrl = Environment.GetEnvironmentVariable("JELLY_URL");
	private readonly string? _jellyfinApiToken = Environment.GetEnvironmentVariable("JELLY_API");

	public override async Task<NowPlaying?> GetNowPlayingAsync(CancellationToken ct = default)
	{
		return await _cache.GetOrCreateAsync("jellyfin:nowplaying", entry =>
		{
			entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1);

			return FetchFromJellyfinAsync();
		});
	}

	private async Task<NowPlaying?> FetchFromJellyfinAsync()
	{
		if (string.IsNullOrWhiteSpace(_jellyfinUrl) || string.IsNullOrWhiteSpace(_jellyfinApiToken))
			return null;

		var url = $"{_jellyfinUrl.TrimEnd("/")}/Sessions?activeWithinSeconds=60";

		try
		{
			using var req = new HttpRequestMessage(HttpMethod.Get, url);
			req.Headers.Add("Authorization", $@"Mediabrowser Token=""{_jellyfinApiToken}""");
			req.Headers.UserAgent.ParseAdd("nowplaying_webapp");

			using var resp = await _http.SendAsync(req, CancellationToken.None);
			resp.EnsureSuccessStatusCode();
			var json = await resp.Content.ReadAsStringAsync(CancellationToken.None);

			return new JellyfinNowPlaying(json);
		}
		catch (HttpRequestException)
		{
			return null;
		}
		catch (TaskCanceledException)
		{
			return null;
		}
	}
}
