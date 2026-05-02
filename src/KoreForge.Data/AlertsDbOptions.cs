namespace KoreForge.Data;

/// <summary>
/// Connection options for the AlertsDB database.
/// </summary>
public sealed class AlertsDbOptions
{
    /// <summary>
    /// Configuration section name used in appsettings.json.
    /// </summary>
    public const string SectionName = "AlertsDb";

    /// <summary>
    /// SQL Server connection string for AlertsDB.
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;
}
