using System.Diagnostics;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Caching.Memory;
using nowplaying_webapp.Models;

namespace nowplaying_webapp.Services;

// The fetchers are for obtaining the data ONLY.
// Parsing of the data is done by the NowPlaying class.
public abstract class Fetcher
{
	public abstract string Name { get; }
	public abstract bool Enabled { get; }

	protected static async Task<string> RunProcess(string file, string args, CancellationToken ct)
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

public sealed class DemoFetcher(ConfigStore config) : Fetcher
{
	public override string Name => "Demo";
	private const int _swapThreshold = 16;
	private int _hitCount = 0;
	private int _index = 0;
	public override bool Enabled => config.Store?.DemoDataEnabled ?? false;

	private static readonly IReadOnlyList<DemoNowPlaying> fakeList = [
		new ("Metal Licker", "Exit Sandman"),
		new ("Nein Maiden", "Die Prophezeiung")
	];

	public override async Task<NowPlaying?> GetNowPlayingAsync(CancellationToken ct = default)
	{
		ct.ThrowIfCancellationRequested();

		// Cycle through demo data when _swapThreshold reached
		if (_hitCount == _swapThreshold)
		{
			_index = (_index + 1) % fakeList.Count;
			_hitCount = 0;
		}
		_hitCount++;
		return fakeList[_index];

	}
}

// this is only partial to satisfy the regex generation
public sealed partial class HyprlandMixxxFetcher(ConfigStore config) : Fetcher
{
	public override string Name => "hyprland-mixxx";
	public override bool Enabled => config.Store?.HyprlandMixxxEnabled ?? false;

	[GeneratedRegex(@"title:\s*(?<t>.*?)\s*\|\s*Mixxx\s*$", RegexOptions.Multiline)]
	private static partial Regex MixxxTitleRegex();

	public override async Task<NowPlaying?> GetNowPlayingAsync(CancellationToken ct = default)
	{
		var hyprctlOutput = await RunProcess("hyprctl", "clients", ct);

		var m = MixxxTitleRegex().Match(hyprctlOutput);
		var parsed = m.Success ? m.Groups["t"].Value : null;

		return string.IsNullOrWhiteSpace(parsed) ? null : new MixxxNowPlaying(parsed);
	}
}

public sealed class JellyfinFetcher(ConfigStore config, HttpClient http) : Fetcher
{
	private readonly HttpClient _http = http;

	public override string Name => "jellyfin";
	public override bool Enabled => config.Store?.JellyfinEnabled ?? false;

	private string? _jellyfinUrl = Environment.GetEnvironmentVariable("JELLYFIN_URL");
	private string? _jellyfinApiToken = Environment.GetEnvironmentVariable("JELLYFIN_API_TOKEN");
	private string? _jellyfinUserName = Environment.GetEnvironmentVariable("JELLYFIN_USER_NAME");

	public override async Task<NowPlaying?> GetNowPlayingAsync(CancellationToken ct = default)
	{
		_jellyfinUrl = config.Store?.JellyfinUrl ?? _jellyfinUrl;
		_jellyfinApiToken = config.Store?.JellyfinApi ?? _jellyfinApiToken;
		_jellyfinUserName = config.Store?.JellyfinUserName ?? _jellyfinUserName;

		if (string.IsNullOrWhiteSpace(_jellyfinUrl) || string.IsNullOrWhiteSpace(_jellyfinApiToken) || string.IsNullOrWhiteSpace(_jellyfinUserName))
		{
			return null;
		}

		var url = $"{_jellyfinUrl.TrimEnd('/')}/Sessions?activeWithinSeconds=60";

		try
		{
			using var req = new HttpRequestMessage(HttpMethod.Get, url);
			req.Headers.Add("Authorization", $"Mediabrowser Token={_jellyfinApiToken}");

			using var resp = await _http.SendAsync(req, ct);
			resp.EnsureSuccessStatusCode();
			var json = await resp.Content.ReadAsStringAsync(ct);

			return new JellyfinNowPlaying(json, _jellyfinUserName);
		}
		catch
		{
			return null;
		}
	}
}

public sealed class PlexFetcher(ConfigStore config, HttpClient http) : Fetcher
{
	private readonly HttpClient _http = http;
	public override string Name => "plex";
	public override bool Enabled => config.Store?.PlexEnabled ?? false;
	private string? _plexUrl = Environment.GetEnvironmentVariable("PLEX_URL");
	private string? _plexApiToken = Environment.GetEnvironmentVariable("PLEX_API_TOKEN");
	private string? _plexUserName = Environment.GetEnvironmentVariable("PLEX_USER_NAME");

	public override async Task<NowPlaying?> GetNowPlayingAsync(CancellationToken ct)
	{
		_plexUrl = config.Store?.PlexUrl ?? _plexUrl;
		_plexApiToken = config.Store?.PlexApi ?? _plexApiToken;
		_plexUserName = config.Store?.PlexUserName ?? _plexUserName;

		if (string.IsNullOrWhiteSpace(_plexUrl) || string.IsNullOrWhiteSpace(_plexApiToken) || string.IsNullOrWhiteSpace(_plexUserName))
		{
			return null;
		}

		var url = $"{_plexUrl.TrimEnd('/')}/status/sessions";

		try
		{
			using var request = new HttpRequestMessage(HttpMethod.Get, url);
			request.Headers.Add("X-Plex-Token", $"{_plexApiToken}");
			request.Headers.Add("Accept", "application/json");

			using var response = await _http.SendAsync(request, ct);
			response.EnsureSuccessStatusCode();
			var json = await response.Content.ReadAsStringAsync(ct);

			return new PlexNowPlaying(json, _plexUserName);

		}
		catch
		{
			return null;
		}
	}
}
