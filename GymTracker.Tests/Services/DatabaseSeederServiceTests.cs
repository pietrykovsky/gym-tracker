using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using GymTracker.Data;
using GymTracker.Services;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System;
using System.Threading;
using System.Linq;

namespace GymTracker.Tests.Services;

public class DatabaseSeederServiceTests
{
    private readonly DatabaseSeederService _sut;
    private readonly TestApplicationDbContext _dbContext;
    private readonly Mock<ILogger<DatabaseSeederService>> _loggerMock;

    public DatabaseSeederServiceTests()
    {
        var factory = new DbContextFactoryWrapper();
        _dbContext = (TestApplicationDbContext)factory.CreateDbContext();
        _loggerMock = new Mock<ILogger<DatabaseSeederService>>();
        _sut = new DatabaseSeederService(factory, _loggerMock.Object);
        _dbContext.Database.EnsureDeleted();
    }

    [Fact]
    public async Task SeedAsync_WhenDatabaseEmpty_SeedsAllData()
    {
        // Act
        await _sut.SeedAsync();

        // Assert
        var categories = await _dbContext.ExerciseCategories.ToListAsync();
        var exercises = await _dbContext.DefaultExercises.ToListAsync();

        categories.Should().NotBeEmpty();
        categories.Should().HaveCount(13);
        exercises.Should().NotBeEmpty();
        exercises.Should().HaveCount(151);

        // Verify relationships
        exercises.All(e => e.Categories.Count() != 0).Should().BeTrue();
        exercises.All(e => categories.Select(c => c.Id).Contains(e.Categories.First().Id)).Should().BeTrue();
    }

    [Fact]
    public async Task SeedAsync_WhenPartialDataExists_OnlyAddsNewData()
    {
        // Arrange
        var category = new ExerciseCategory { Name = "Chest", Description = "Test" };
        var exercise = new DefaultExercise
        {
            Name = "Bench Press",
            Description = "Test",
            Categories = [category]
        };

        await _dbContext.ExerciseCategories.AddAsync(category);
        await _dbContext.DefaultExercises.AddAsync(exercise);
        await _dbContext.SaveChangesAsync();

        // Act
        await _sut.SeedAsync();

        // Assert
        var categories = await _dbContext.ExerciseCategories.ToListAsync();
        var exercises = await _dbContext.DefaultExercises.ToListAsync();

        categories.Should().HaveCount(13); // All categories should exist
        exercises.Count.Should().BeGreaterThan(1); // More exercises should be added

        // Original data should remain unchanged
        var originalCategory = categories.FirstOrDefault(c => c.Name == "Chest");
        originalCategory.Should().NotBeNull();
        originalCategory!.Description.Should().Be("Test");

        var originalExercise = exercises.FirstOrDefault(e => e.Name == "Bench Press");
        originalExercise.Should().NotBeNull();
        originalExercise!.Description.Should().Be("Test");
    }

    [Fact]
    public async Task SeedAsync_WhenNewDataAdded_UpdatesSuccessfully()
    {
        // Arrange - First seeding
        await _sut.SeedAsync();
        var initialExerciseCount = await _dbContext.DefaultExercises.CountAsync();

        // Act - Simulate adding new exercise to seed data and reseed
        var newCategory = new ExerciseCategory { Name = "Test Category", Description = "Test" };
        await _dbContext.ExerciseCategories.AddAsync(newCategory);
        await _dbContext.SaveChangesAsync();

        await _sut.SeedAsync();

        // Assert
        var categories = await _dbContext.ExerciseCategories.ToListAsync();
        var exercises = await _dbContext.DefaultExercises.ToListAsync();

        categories.Count.Should().BeGreaterThan(10); // Original + new category
        exercises.Count.Should().Be(initialExerciseCount); // No duplicate exercises
    }

    [Fact]
    public async Task SeedAsync_WhenExceptionOccurs_LogsErrorAndRethrows()
    {
        // Arrange
        var failingFactory = new Mock<IDbContextFactory<ApplicationDbContext>>();
        failingFactory.Setup(f => f.CreateDbContextAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Test exception"));

        var seeder = new DatabaseSeederService(failingFactory.Object, _loggerMock.Object);

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => seeder.SeedAsync());

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
    }
}