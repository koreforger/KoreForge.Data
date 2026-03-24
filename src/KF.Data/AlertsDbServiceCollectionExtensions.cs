using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace KF.Data;

/// <summary>
/// Extension methods for registering the AlertsDB context.
/// </summary>
public static class AlertsDbServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="AlertsDbContext"/> using a pre-configured <see cref="AlertsDbOptions"/>.
    /// </summary>
    public static IServiceCollection AddAlertsDb(
        this IServiceCollection services,
        Action<AlertsDbOptions> configure)
    {
        var options = new AlertsDbOptions();
        configure(options);

        services.AddDbContext<AlertsDbContext>(db =>
            db.UseSqlServer(options.ConnectionString));

        return services;
    }

    /// <summary>
    /// Registers <see cref="AlertsDbContext"/> with a raw connection string.
    /// </summary>
    public static IServiceCollection AddAlertsDb(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddDbContext<AlertsDbContext>(db =>
            db.UseSqlServer(connectionString));

        return services;
    }
}
