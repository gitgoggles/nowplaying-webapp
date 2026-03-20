using Microsoft.EntityFrameworkCore;
using nowplaying_webapp.Models;

namespace nowplaying_webapp.Services;

public sealed class ConfigStore(IServiceScopeFactory scopeFactory)
{
	public ConfigModel? Store { get; private set; }
	public event Action? ConfigUpdated;

	public async Task RefreshAsync(CancellationToken ct = default)
	{
		using var scope = scopeFactory.CreateScope();
		var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
		Store = await db.Configs.AsNoTracking().FirstOrDefaultAsync(ct);
		ConfigUpdated?.Invoke();
	}
}
