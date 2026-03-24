using KF.Data.Generated;
using KF.Data.Generated.Alerts;
using Microsoft.EntityFrameworkCore;

namespace KF.Data.Tests;

public class AlertsDbContextTests : IDisposable
{
    private readonly AlertsDbContext _context;

    public AlertsDbContextTests()
    {
        var options = new DbContextOptionsBuilder<AlertsDbContext>()
            .UseSqlite("DataSource=:memory:")
            .Options;

        _context = new AlertsDbContext(options);
        _context.Database.OpenConnection();
        _context.Database.EnsureCreated();
    }

    public void Dispose()
    {
        _context.Database.CloseConnection();
        _context.Dispose();
    }

    // -----------------------------------------------------------------------
    // Context construction
    // -----------------------------------------------------------------------

    [Fact]
    public void Context_CanBeConstructed()
    {
        Assert.NotNull(_context);
    }

    // -----------------------------------------------------------------------
    // DbSet existence
    // -----------------------------------------------------------------------

    [Fact]
    public void DbSet_Channel_Exists()
    {
        Assert.NotNull(_context.Channel);
    }

    [Fact]
    public void DbSet_Priority_Exists()
    {
        Assert.NotNull(_context.Priority);
    }

    [Fact]
    public void DbSet_OutboxStatus_Exists()
    {
        Assert.NotNull(_context.OutboxStatus);
    }

    [Fact]
    public void DbSet_SendOutcome_Exists()
    {
        Assert.NotNull(_context.SendOutcome);
    }

    [Fact]
    public void DbSet_NotificationOutbox_Exists()
    {
        Assert.NotNull(_context.NotificationOutbox);
    }

    [Fact]
    public void DbSet_EmailPayload_Exists()
    {
        Assert.NotNull(_context.EmailPayload);
    }

    [Fact]
    public void DbSet_SmsPayload_Exists()
    {
        Assert.NotNull(_context.SmsPayload);
    }

    // -----------------------------------------------------------------------
    // Lookup table CRUD
    // -----------------------------------------------------------------------

    [Fact]
    public void Channel_CanInsertAndQuery()
    {
        _context.Channel.Add(new Channel { Name = "Email" });
        _context.SaveChanges();

        var result = _context.Channel.Single(c => c.Name == "Email");
        Assert.Equal("Email", result.Name);
        Assert.True(result.ChannelId > 0);
    }

    [Fact]
    public void Priority_CanInsertAndQuery()
    {
        _context.Priority.Add(new Priority { Name = "High" });
        _context.SaveChanges();

        var result = _context.Priority.Single(p => p.Name == "High");
        Assert.Equal("High", result.Name);
    }

    [Fact]
    public void OutboxStatus_CanInsertAndQuery()
    {
        _context.OutboxStatus.Add(new OutboxStatus { Name = "Pending" });
        _context.SaveChanges();

        var result = _context.OutboxStatus.Single(s => s.Name == "Pending");
        Assert.Equal("Pending", result.Name);
    }

    [Fact]
    public void SendOutcome_CanInsertAndQuery()
    {
        _context.SendOutcome.Add(new SendOutcome { Name = "Success" });
        _context.SaveChanges();

        var result = _context.SendOutcome.Single(s => s.Name == "Success");
        Assert.Equal("Success", result.Name);
    }

    // -----------------------------------------------------------------------
    // FK navigation — NotificationOutbox → lookup tables
    // -----------------------------------------------------------------------

    [Fact]
    public void NotificationOutbox_NavigationToChannel_Works()
    {
        SeedLookups();

        var outbox = CreateOutbox();
        _context.NotificationOutbox.Add(outbox);
        _context.SaveChanges();

        var loaded = _context.NotificationOutbox
            .Include(n => n.Channel)
            .Single();
        Assert.NotNull(loaded.Channel);
        Assert.Equal("Email", loaded.Channel.Name);
    }

    [Fact]
    public void NotificationOutbox_NavigationToPriority_Works()
    {
        SeedLookups();

        var outbox = CreateOutbox();
        _context.NotificationOutbox.Add(outbox);
        _context.SaveChanges();

        var loaded = _context.NotificationOutbox
            .Include(n => n.Priority)
            .Single();
        Assert.NotNull(loaded.Priority);
        Assert.Equal("Normal", loaded.Priority.Name);
    }

    [Fact]
    public void NotificationOutbox_NavigationToOutboxStatus_Works()
    {
        SeedLookups();

        var outbox = CreateOutbox();
        _context.NotificationOutbox.Add(outbox);
        _context.SaveChanges();

        var loaded = _context.NotificationOutbox
            .Include(n => n.OutboxStatus)
            .Single();
        Assert.NotNull(loaded.OutboxStatus);
        Assert.Equal("Pending", loaded.OutboxStatus.Name);
    }

    [Fact]
    public void NotificationOutbox_NullableSendOutcome_IsNull()
    {
        SeedLookups();

        var outbox = CreateOutbox();
        _context.NotificationOutbox.Add(outbox);
        _context.SaveChanges();

        var loaded = _context.NotificationOutbox
            .Include(n => n.SendOutcome)
            .Single();
        Assert.Null(loaded.SendOutcome);
    }

    [Fact]
    public void NotificationOutbox_WithSendOutcome_Navigates()
    {
        SeedLookups();
        var outcome = _context.SendOutcome.Add(new SendOutcome { Name = "Success" });
        _context.SaveChanges();

        var outbox = CreateOutbox();
        outbox.SendOutcomeId = outcome.Entity.SendOutcomeId;
        _context.NotificationOutbox.Add(outbox);
        _context.SaveChanges();

        var loaded = _context.NotificationOutbox
            .Include(n => n.SendOutcome)
            .Single();
        Assert.NotNull(loaded.SendOutcome);
        Assert.Equal("Success", loaded.SendOutcome!.Name);
    }

    // -----------------------------------------------------------------------
    // FK navigation — EmailPayload / SmsPayload → NotificationOutbox
    // -----------------------------------------------------------------------

    [Fact]
    public void EmailPayload_NavigationToOutbox_Works()
    {
        SeedLookups();
        var outbox = CreateOutbox();
        _context.NotificationOutbox.Add(outbox);
        _context.SaveChanges();

        _context.EmailPayload.Add(new EmailPayload
        {
            NotificationOutboxId = outbox.NotificationOutboxId,
            FromAddress = "noreply@test.com",
            IsHtml = true
        });
        _context.SaveChanges();

        var loaded = _context.EmailPayload
            .Include(e => e.NotificationOutbox)
            .Single();
        Assert.NotNull(loaded.NotificationOutbox);
        Assert.Equal(outbox.NotificationOutboxId, loaded.NotificationOutbox.NotificationOutboxId);
    }

    [Fact]
    public void SmsPayload_NavigationToOutbox_Works()
    {
        SeedLookups();
        var outbox = CreateOutbox();
        _context.NotificationOutbox.Add(outbox);
        _context.SaveChanges();

        _context.SmsPayload.Add(new SmsPayload
        {
            NotificationOutboxId = outbox.NotificationOutboxId,
            FromNumber = "+44123456789"
        });
        _context.SaveChanges();

        var loaded = _context.SmsPayload
            .Include(s => s.NotificationOutbox)
            .Single();
        Assert.NotNull(loaded.NotificationOutbox);
    }

    // -----------------------------------------------------------------------
    // Reverse navigation — Outbox → collections
    // -----------------------------------------------------------------------

    [Fact]
    public void NotificationOutbox_EmailPayloadCollection_Populates()
    {
        SeedLookups();
        var outbox = CreateOutbox();
        _context.NotificationOutbox.Add(outbox);
        _context.SaveChanges();

        _context.EmailPayload.Add(new EmailPayload
        {
            NotificationOutboxId = outbox.NotificationOutboxId,
            FromAddress = "a@test.com",
            IsHtml = true
        });
        _context.EmailPayload.Add(new EmailPayload
        {
            NotificationOutboxId = outbox.NotificationOutboxId,
            FromAddress = "b@test.com",
            IsHtml = false
        });
        _context.SaveChanges();

        var loaded = _context.NotificationOutbox
            .Include(n => n.EmailPayload)
            .Single();
        Assert.Equal(2, loaded.EmailPayload.Count);
    }

    [Fact]
    public void Channel_ReverseNavigation_PopulatesOutboxCollection()
    {
        SeedLookups();
        var outbox = CreateOutbox();
        _context.NotificationOutbox.Add(outbox);
        _context.SaveChanges();

        var channel = _context.Channel
            .Include(c => c.NotificationOutbox)
            .Single(c => c.Name == "Email");
        Assert.Single(channel.NotificationOutbox);
    }

    // -----------------------------------------------------------------------
    // Helpers
    // -----------------------------------------------------------------------

    private void SeedLookups()
    {
        _context.Channel.Add(new Channel { Name = "Email" });
        _context.Priority.Add(new Priority { Name = "Normal" });
        _context.OutboxStatus.Add(new OutboxStatus { Name = "Pending" });
        _context.SaveChanges();
    }

    private NotificationOutbox CreateOutbox()
    {
        var channel = _context.Channel.Single(c => c.Name == "Email");
        var priority = _context.Priority.Single(p => p.Name == "Normal");
        var status = _context.OutboxStatus.Single(s => s.Name == "Pending");

        return new NotificationOutbox
        {
            CorrelationId = Guid.NewGuid(),
            ChannelId = channel.ChannelId,
            PriorityId = priority.PriorityId,
            OutboxStatusId = status.OutboxStatusId,
            Recipient = "user@example.com",
            Body = "Test notification body",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
    }
}
