using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Sqlite;


namespace nowplaying_webapp.Models;

public class AppDbContext : DbContext
{
	public DbSet<Config> Configs { get; set; }

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

public class Config
{
	public int Id { get; set; }
	public string Name { get; set; }
	public string Value { get; set; }
}
