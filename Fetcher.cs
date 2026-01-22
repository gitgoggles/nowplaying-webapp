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

	public override MixxxNowPlaying? GetNowPlaying()
	{
		var hyprctlOutput = RunProcess("hyprctl", "clients");

		var m = MixxxTitleRegex.Match(hyprctlOutput);
		var parsed = m.Success ? m.Groups["t"].Value : null;
		var obj = new MixxxNowPlaying(parsed);
		return obj;
	}
}
