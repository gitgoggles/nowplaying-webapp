using System.Diagnostics;
using System.Text.RegularExpressions;

namespace nowplaying_webapp;

public abstract class Fetcher
{
	protected string RunProcess(string file, string args)
	{
		var psi = new ProcessStartInfo(file, args)
		{
			RedirectStandardOutput = true,
			UseShellExecute = false,
		};

		using var p = Process.Start(psi) ?? throw new InvalidOperationException($"Failed to start process: {file}");
		var output = p.StandardOutput.ReadToEnd();
		p.WaitForExit();

		return output;
	}

	public abstract NowPlaying? GetNowPlaying();
}

public sealed class HyprlandMixxxFetcher : Fetcher
{
	private static readonly Regex MixxxTitleRegex =
		new(@"title:\s*(?<t>.*?)\s*\|\s*Mixxx\s*$", RegexOptions.Multiline);

	public override NowPlaying? GetNowPlaying()
	{
		var hyprctlOutput = RunProcess("hyprctl", "clients");

		var m = MixxxTitleRegex.Match(hyprctlOutput);
		var parsed = m.Success ? m.Groups["t"].Value : null;
		return new MixxxNowPlaying(parsed);
	}
}

public sealed class JellyfinFetcher : Fetcher
{
	static readonly HttpClient Http = new();

	public override NowPlaying? GetNowPlaying()
	{
		string? jellyfinBaseUrl = Environment.GetEnvironmentVariable("JELLY_URL");
		string? jellyfinApiToken = Environment.GetEnvironmentVariable("JELLY_API");

		if (string.IsNullOrWhiteSpace(jellyfinBaseUrl) || string.IsNullOrWhiteSpace(jellyfinApiToken))
			return null;

		var url = $"{jellyfinBaseUrl.TrimEnd("/")}/Sessions?activeWithinSeconds=60";

		try
		{
			using var req = new HttpRequestMessage(HttpMethod.Get, url);
			req.Headers.Add("Authorization", $@"Mediabrowser Token=""{jellyfinApiToken}""");
			req.Headers.UserAgent.ParseAdd("nowplaying_webapp");

			using var resp = Http.Send(req);
			resp.EnsureSuccessStatusCode();
			var json = resp.Content.ReadAsStringAsync().GetAwaiter().GetResult();

			return new JellyfinNowPlaying(json);
		}
		catch (HttpRequestException)
		{
			return null;
		}
		catch (TaskCanceledException) // timeout
		{
			return null;
		}
	}
}
