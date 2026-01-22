
namespace nowplaying_webapp;

public abstract class NowPlaying
{
	public string? artist;
	public string? full;
	public string? title;
	public bool artistandtitleaquired;
}

public sealed class MixxxNowPlaying : NowPlaying
{
	public MixxxNowPlaying(string? input)
	{
		var split = input?.Split(" - ");
		artist = split?.Length == 2 ? split[0] : null;
		title = split?.Length == 2 ? split[1] : null;
		full = input;
		artistandtitleaquired = artist is not null && title is not null;
	}

}
