using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Sqlite;


namespace nowplaying_webapp.Models;

public class AppDbContext : DbContext
{
	public DbSet<ConfigModel> Configs { get; set; }

	public string DbPath { get; set; }

	public AppDbContext()
	{
		var destination = Environment.SpecialFolder.LocalApplicationData;
		var path = Environment.GetFolderPath(destination);
		var appFolderPath = Path.Join(path, "nowplayingapp");

		Directory.CreateDirectory(appFolderPath);
		DbPath = System.IO.Path.Join(appFolderPath, "nowplayingapp.db");
	}

	protected override void OnConfiguring(DbContextOptionsBuilder options)
		=> options.UseSqlite($"Data Source={DbPath}");
}

public class ConfigModel
{
	public int Id { get; set; }
	public string JellyfinUrl { get; set; } = string.Empty;
	public string JellyfinApi { get; set; } = string.Empty;
	public bool JellyfinEnabled { get; set; } = false;
	public string PlexUrl { get; set; } = string.Empty;
	public string PlexApi { get; set; } = string.Empty;
	public bool PlexEnabled { get; set; } = false;
}
