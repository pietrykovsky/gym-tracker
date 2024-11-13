using System;
using GymTracker.Data;
using Microsoft.EntityFrameworkCore;

namespace GymTracker.Tests.Services;

internal class DbContextFactoryWrapper : IDbContextFactory<ApplicationDbContext>
{
    private readonly TestApplicationDbContext _context;

    public DbContextFactoryWrapper(string? databaseName = null)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName ?? $"InMemoryTest_{Guid.NewGuid()}")
            .Options;
        _context = new TestApplicationDbContext(options);
    }

    public ApplicationDbContext CreateDbContext()
    {
        return _context;
    }
}
