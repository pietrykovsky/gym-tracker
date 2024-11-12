using System;
using System.Threading.Tasks;
using GymTracker.Data;
using Microsoft.EntityFrameworkCore;

namespace GymTracker.Tests.Services;

internal class TestApplicationDbContext : ApplicationDbContext, IDisposable
{
    public TestApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public new void Dispose()
    {
    }
}
