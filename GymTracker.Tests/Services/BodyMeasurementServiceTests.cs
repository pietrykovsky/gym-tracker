using FluentAssertions;
using GymTracker.Data;
using GymTracker.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace GymTracker.Tests.Services;

public class BodyMeasurementServiceTests
{
    private readonly BodyMeasurementService _sut;
    private readonly TestApplicationDbContext _dbContext;

    public BodyMeasurementServiceTests()
    {
        var factory = new DbContextFactoryWrapper();

        _dbContext = (TestApplicationDbContext)factory.CreateDbContext();
        _sut = new BodyMeasurementService(factory);
        _dbContext.Database.EnsureDeleted();

        SeedData();
    }

    private void SeedData()
    {
        _dbContext.BodyMeasurements.AddRange(
            new BodyMeasurement { Id = 1, UserId = "user1", Date = new DateOnly(2023, 1, 1), Weight = 70 },
            new BodyMeasurement { Id = 2, UserId = "user1", Date = new DateOnly(2023, 2, 1), Weight = 72 },
            new BodyMeasurement { Id = 3, UserId = "user2", Date = new DateOnly(2023, 3, 1), Weight = 75 }
        );
        _dbContext.SaveChanges();
    }

    [Fact]
    public async Task GetUserMeasurementsAsync_ReturnsMeasurementsForUser()
    {
        // Act
        var result = await _sut.GetUserMeasurementsAsync("user1");

        // Assert
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(m => m.UserId.Should().Be("user1"));
    }

    [Fact]
    public async Task GetUserMeasurements_ReturnsAllUserMeasurements_OrderedByDateDescending()
    {
        // Arrange
        _dbContext.BodyMeasurements.AddRange(
            new BodyMeasurement { Id = 4, UserId = "user1", Date = new DateOnly(2023, 1, 21), Weight = 67 },
            new BodyMeasurement { Id = 5, UserId = "user1", Date = new DateOnly(2021, 2, 15), Weight = 72 }
        );
        _dbContext.SaveChanges();

        // Act
        var result = await _sut.GetUserMeasurementsAsync("user1");

        // Assert
        result.Should().NotBeNull();
        result.Should().BeInDescendingOrder(m => m.Date);
        result.Should().AllSatisfy(m => m.UserId.Should().Be("user1"));
        result.Should().HaveCount(4);
    }

    [Fact]
    public async Task GetMeasurementsInRange_ReturnsOnlyMeasurementsWithinRange()
    {
        // Arrange
        var startDate = new DateOnly(2024, 1, 1);
        var endDate = new DateOnly(2024, 1, 31);
        _dbContext.BodyMeasurements.AddRange(
            new BodyMeasurement { Id = 10, UserId = "user3", Date = new DateOnly(2024, 1, 15), Weight = 70.0f },
            new BodyMeasurement { Id = 11, UserId = "user3", Date = new DateOnly(2024, 2, 1), Weight = 71.0f }
        );
        _dbContext.SaveChanges();

        // Act
        var result = await _sut.GetMeasurementsInRangeAsync("user3", startDate, endDate);

        // Assert
        result.Should().NotBeNull()
            .And.HaveCount(1)
            .And.AllSatisfy(m =>
            {
                m.Date.Should().BeOnOrAfter(startDate);
                m.Date.Should().BeOnOrBefore(endDate);
                m.UserId.Should().Be("user3");
            });
    }

    [Fact]
    public async Task GetMeasurementAsync_ReturnsCorrectMeasurement()
    {
        // Act
        var result = await _sut.GetMeasurementAsync("user1", 1);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result!.UserId.Should().Be("user1");
    }

    [Fact]
    public async Task CreateMeasurementAsync_AddsMeasurement()
    {
        // Arrange
        var newMeasurement = new BodyMeasurement
        {
            Date = new DateOnly(2023, 4, 1),
            Weight = 68
        };

        // Act
        var result = await _sut.CreateMeasurementAsync("user1", newMeasurement);

        // Assert
        var measurementsCount = _dbContext.BodyMeasurements.Count();
        result.Should().NotBeNull();
        result.UserId.Should().Be("user1");
        result.Weight.Should().Be(68);
        measurementsCount.Should().Be(4);
    }

    [Fact]
    public async Task UpdateMeasurementAsync_UpdatesMeasurement()
    {
        // Arrange
        var updatedMeasurement = new BodyMeasurement
        {
            Date = new DateOnly(2023, 1, 15),
            Weight = 71
        };

        // Act
        var result = await _sut.UpdateMeasurementAsync("user1", 1, updatedMeasurement);

        // Assert
        result.Should().NotBeNull();
        result!.Date.Should().Be(new DateOnly(2023, 1, 15));
        result!.Weight.Should().Be(71);
    }

    [Fact]
    public async Task DeleteMeasurementAsync_RemovesMeasurement()
    {
        // Act
        var success = await _sut.DeleteMeasurementAsync("user1", 1);

        // Assert
        var measurementsCount = _dbContext.BodyMeasurements.Count();
        success.Should().BeTrue();
        measurementsCount.Should().Be(2);
    }

    [Fact]
    public async Task UpdateMeasurement_ReturnsNull_WhenMeasurementNotFound()
    {
        // Arrange
        var updatedMeasurement = new BodyMeasurement
        {
            Id = 999,
            Weight = 80.0f
        };

        // Act
        var result = await _sut.UpdateMeasurementAsync("fake user", 999, updatedMeasurement);

        // Assert
        result.Should().BeNull();
    }
}
