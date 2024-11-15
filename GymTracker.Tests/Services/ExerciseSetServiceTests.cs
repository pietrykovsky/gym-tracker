using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using GymTracker.Data;
using GymTracker.Services;
using Microsoft.EntityFrameworkCore;

namespace GymTracker.Tests.Services;

public class ExerciseSetServiceTests
{
    private readonly ExerciseSetService _sut;
    private readonly TestApplicationDbContext _dbContext;

    public ExerciseSetServiceTests()
    {
        var factory = new DbContextFactoryWrapper();
        _dbContext = (TestApplicationDbContext)factory.CreateDbContext();
        _sut = new ExerciseSetService(factory);
        _dbContext.Database.EnsureDeleted();

        SeedData();
    }

    private void SeedData()
    {
        var exercise = new DefaultExercise { Id = 1, Name = "Squat" };
        var activity = new PlanActivity { Id = 1, PlanId = 1, ExerciseId = 1 };
        _dbContext.DefaultExercises.Add(exercise);
        _dbContext.TrainingActivities.Add(activity);

        var sets = new[]
        {
            new ExerciseSet
            {
                Id = 1,
                ActivityId = 1,
                Order = 1,
                Repetitions = 10,
                Weight = 100,
                RestAfterDuration = 60
            },
            new ExerciseSet
            {
                Id = 2,
                ActivityId = 1,
                Order = 2,
                Repetitions = 12,
                Weight = 90,
                RestAfterDuration = 90
            }};

        _dbContext.ExerciseSets.AddRange(sets);
        _dbContext.SaveChanges();
    }

    [Fact]
    public async Task GetActivitySetsAsync_ReturnsSetsInOrder()
    {
        // Act
        var result = await _sut.GetActivitySetsAsync(1);

        // Assert
        result.Should().HaveCount(2);
        result.Should().BeInAscendingOrder(s => s.Order);
        result.First().Repetitions.Should().Be(10);
        result.Last().Repetitions.Should().Be(12);
    }

    [Fact]
    public async Task GetSetByIdAsync_ReturnsCorrectSet()
    {
        // Act
        var result = await _sut.GetSetByIdAsync(1, 1);

        // Assert
        result.Should().NotBeNull();
        result!.Repetitions.Should().Be(10);
        result.Weight.Should().Be(100);
        result.RestAfterDuration.Should().Be(60);
    }

    [Fact]
    public async Task CreateSetAsync_CreatesNewSet()
    {
        // Arrange
        var newSet = new ExerciseSet
        {
            ActivityId = 1,
            Repetitions = 15,
            Weight = 80,
            RestAfterDuration = 45
        };

        // Act
        var result = await _sut.CreateSetAsync(newSet);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
        result.Order.Should().Be(3); // Should be placed at the end
        result.Repetitions.Should().Be(15);
        result.Weight.Should().Be(80);
        result.RestAfterDuration.Should().Be(45);
    }

    [Fact]
    public async Task UpdateSetAsync_UpdatesSet()
    {
        // Arrange
        var updatedSet = new ExerciseSet
        {
            Repetitions = 20,
            Weight = 75,
            RestAfterDuration = 30,
            Order = 1
        };

        // Act
        var result = await _sut.UpdateSetAsync(1, 1, updatedSet);

        // Assert
        result.Should().NotBeNull();
        result!.Repetitions.Should().Be(20);
        result.Weight.Should().Be(75);
        result.RestAfterDuration.Should().Be(30);
        result.Order.Should().Be(1);
    }

    [Fact]
    public async Task DeleteSetAsync_DeletesSet()
    {
        // Act
        var result = await _sut.DeleteSetAsync(1, 1);
        var remainingSets = await _dbContext.ExerciseSets
            .Where(s => s.ActivityId == 1)
            .OrderBy(s => s.Order)
            .ToListAsync();

        // Assert
        result.Should().BeTrue();
        remainingSets.Should().HaveCount(1);
        remainingSets[0].Order.Should().Be(1); // Order should be adjusted
    }

    [Fact]
    public async Task UpdateSetsOrderAsync_UpdatesOrder()
    {
        // Arrange
        var newOrders = new[]
        {
            (SetId: 1, NewOrder: 2),
            (SetId: 2, NewOrder: 1)
        };

        // Act
        var result = await _sut.UpdateSetsOrderAsync(1, newOrders);
        var sets = await _dbContext.ExerciseSets
            .Where(s => s.ActivityId == 1)
            .OrderBy(s => s.Order)
            .ToListAsync();

        // Assert
        result.Should().BeTrue();
        sets[0].Id.Should().Be(2);
        sets[1].Id.Should().Be(1);
    }

    [Fact]
    public async Task GetActivitySetsAsync_WithNoSets_ReturnsEmptyList()
    {
        // Act
        var result = await _sut.GetActivitySetsAsync(999);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetSetByIdAsync_WithInvalidId_ReturnsNull()
    {
        // Act
        var result = await _sut.GetSetByIdAsync(1, 999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateSetAsync_WithInvalidId_ReturnsNull()
    {
        // Arrange
        var updatedSet = new ExerciseSet
        {
            Repetitions = 20,
            Weight = 75
        };

        // Act
        var result = await _sut.UpdateSetAsync(1, 999, updatedSet);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteSetAsync_WithInvalidId_ReturnsFalse()
    {
        // Act
        var result = await _sut.DeleteSetAsync(1, 999);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateSetsOrderAsync_WithInvalidIds_StillSucceeds()
    {
        // Arrange
        var newOrders = new[]
        {
            (SetId: 999, NewOrder: 1),
            (SetId: 998, NewOrder: 2)
        };

        // Act
        var result = await _sut.UpdateSetsOrderAsync(1, newOrders);

        // Assert
        result.Should().BeTrue(); // The operation itself succeeds
        var sets = await _dbContext.ExerciseSets
            .Where(s => s.ActivityId == 1)
            .OrderBy(s => s.Order)
            .ToListAsync();
        sets.Should().HaveCount(2); // Original sets should remain unchanged
    }

    [Fact]
    public async Task CreateSetAsync_AutomaticallyAssignsCorrectOrder()
    {
        // Arrange
        await _dbContext.ExerciseSets.AddRangeAsync(
            new ExerciseSet { ActivityId = 2, Order = 1, Repetitions = 10 },
            new ExerciseSet { ActivityId = 2, Order = 2, Repetitions = 12 }
        );
        await _dbContext.SaveChangesAsync();

        var newSet = new ExerciseSet
        {
            ActivityId = 2,
            Repetitions = 15
        };

        // Act
        var result = await _sut.CreateSetAsync(newSet);

        // Assert
        result.Should().NotBeNull();
        result.Order.Should().Be(3);
    }

    [Fact]
    public async Task DeleteSetAsync_ReordersRemainingSets()
    {
        // Arrange
        var sets = new[]
        {
            new ExerciseSet { ActivityId = 3, Order = 1, Repetitions = 10 },
            new ExerciseSet { ActivityId = 3, Order = 2, Repetitions = 12 },
            new ExerciseSet { ActivityId = 3, Order = 3, Repetitions = 15 }
        };
        await _dbContext.ExerciseSets.AddRangeAsync(sets);
        await _dbContext.SaveChangesAsync();

        // Act
        await _sut.DeleteSetAsync(3, sets[0].Id);
        var remainingSets = await _dbContext.ExerciseSets
            .Where(s => s.ActivityId == 3)
            .OrderBy(s => s.Order)
            .ToListAsync();

        // Assert
        remainingSets.Should().HaveCount(2);
        remainingSets[0].Order.Should().Be(1);
        remainingSets[1].Order.Should().Be(2);
        remainingSets[0].Repetitions.Should().Be(12);
        remainingSets[1].Repetitions.Should().Be(15);
    }
}
