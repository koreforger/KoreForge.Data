using KF.Data.Alerts;
using KF.Data.Alerts.Generated;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace KF.Data.Tests;

public class RegistrationTests
{
    [Fact]
    public void AddAlertsDb_WithOptions_RegistersContext()
    {
        var services = new ServiceCollection();
        services.AddAlertsDb(opts => opts.ConnectionString = "Server=.;Database=AlertsDB;Trusted_Connection=True;TrustServerCertificate=True;");

        var provider = services.BuildServiceProvider();
        var context = provider.GetService<AlertsDbContext>();

        Assert.NotNull(context);
    }

    [Fact]
    public void AddAlertsDb_WithConnectionString_RegistersContext()
    {
        var services = new ServiceCollection();
        services.AddAlertsDb("Server=.;Database=AlertsDB;Trusted_Connection=True;TrustServerCertificate=True;");

        var provider = services.BuildServiceProvider();
        var context = provider.GetService<AlertsDbContext>();

        Assert.NotNull(context);
    }

    [Fact]
    public void AlertsDbOptions_SectionName_IsCorrect()
    {
        Assert.Equal("AlertsDb", AlertsDbOptions.SectionName);
    }

    [Fact]
    public void AlertsDbOptions_ConnectionString_DefaultsToEmpty()
    {
        var options = new AlertsDbOptions();
        Assert.Equal(string.Empty, options.ConnectionString);
    }
}
