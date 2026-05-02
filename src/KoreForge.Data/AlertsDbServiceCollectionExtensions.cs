using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace KoreForge.Data;

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

    /// <summary>
    /// Registers an <see cref="IDbContextFactory{AlertsDbContext}"/> (singleton) for creating
    /// contexts outside of a request scope — background workers, hosted services, parallel work.
    /// </summary>
    public static IServiceCollection AddAlertsDbFactory(
        this IServiceCollection services,
        Action<AlertsDbOptions> configure)
    {
        var options = new AlertsDbOptions();
        configure(options);

        services.AddDbContextFactory<AlertsDbContext>(db =>
            db.UseSqlServer(options.ConnectionString));

        return services;
    }

    /// <summary>
    /// Registers an <see cref="IDbContextFactory{AlertsDbContext}"/> (singleton) with a raw connection string.
    /// </summary>
    public static IServiceCollection AddAlertsDbFactory(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddDbContextFactory<AlertsDbContext>(db =>
            db.UseSqlServer(connectionString));

        return services;
    }
}
