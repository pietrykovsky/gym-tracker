using FluentAssertions;
using GymTracker.Data;
using GymTracker.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GymTracker.Tests.Services;

public class TrainingActivityServiceTests
{
    private readonly TrainingActivityService _sut;
    private readonly TestApplicationDbContext _dbContext;

    public TrainingActivityServiceTests()
    {
        var factory = new DbContextFactoryWrapper();
        _dbContext = (TestApplicationDbContext)factory.CreateDbContext();
        _sut = new TrainingActivityService(factory);
        _dbContext.Database.EnsureDeleted();

        SeedData();
    }

    private void SeedData()
    {
        var exercise = new DefaultExercise { Id = 1, Name = "Squat" };
        _dbContext.DefaultExercises.Add(exercise);

        var session = new TrainingSession
        {
            Id = 1,
            UserId = "user1",
            Date = new DateOnly(2024, 1, 1),
            Activities = new List<TrainingActivity>
            {
                new()
                {
                    Id = 1,
                    Order = 1,
                    ExerciseId = 1,
                    Sets = new List<ExerciseSet>
                    {
                        new() { Order = 1, Repetitions = 10, Weight = 100 },
                        new() { Order = 2, Repetitions = 10, Weight = 100 }
                    }
                },
                new()
                {
                    Id = 2,
                    Order = 2,
                    ExerciseId = 1,
                    Sets = new List<ExerciseSet>
                    {
                        new() { Order = 1, Repetitions = 8, Weight = 120 }
                    }
                }
            }
        };

        _dbContext.TrainingSessions.Add(session);
        _dbContext.SaveChanges();
    }

    [Fact]
    public async Task GetSessionActivitiesAsync_ReturnsActivities()
    {
        // Act
        var result = await _sut.GetSessionActivitiesAsync("user1", 1);

        // Assert
        result.Should().HaveCount(2);
        result.Should().BeInAscendingOrder(a => a.Order);
    }

    [Fact]
    public async Task GetActivityByIdAsync_ReturnsCorrectActivity()
    {
        // Act
        var result = await _sut.GetActivityByIdAsync("user1", 1, 1);

        // Assert
        result.Should().NotBeNull();
        result!.Order.Should().Be(1);
        result.Sets.Should().HaveCount(2);
    }

    [Fact]
    public async Task CreateActivityAsync_CreatesActivity()
    {
        // Arrange
        var activity = new TrainingActivity
        {
            ExerciseId = 1,
            Sets = new List<ExerciseSet>
            {
                new() { Order = 1, Repetitions = 12, Weight = 90 }
            }
        };

        // Act
        var result = await _sut.CreateActivityAsync("user1", 1, activity);

        // Assert
        result.Should().NotBeNull();
        result.Order.Should().Be(3); // Should be placed at the end
        result.Sets.Should().HaveCount(1);
    }

    [Fact]
    public async Task CreateActivityAsync_WithInvalidSession_ThrowsException()
    {
        // Arrange
        var activity = new TrainingActivity { ExerciseId = 1 };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _sut.CreateActivityAsync("user1", 999, activity));
    }

    [Fact]
    public async Task UpdateActivityAsync_UpdatesActivity()
    {
        // Arrange
        var updatedActivity = new TrainingActivity
        {
            ExerciseId = 1,
            Sets = new List<ExerciseSet>
            {
                new() { Order = 1, Repetitions = 15, Weight = 80 }
            }
        };

        // Act
        var result = await _sut.UpdateActivityAsync("user1", 1, 1, updatedActivity);

        // Assert
        result.Should().NotBeNull();
        result!.Sets.Should().HaveCount(1);
        result.Sets.First().Repetitions.Should().Be(15);
    }

    [Fact]
    public async Task UpdateActivityAsync_WithInvalidId_ReturnsNull()
    {
        // Act
        var result = await _sut.UpdateActivityAsync("user1", 1, 999, new TrainingActivity());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteActivityAsync_DeletesActivity()
    {
        // Act
        var result = await _sut.DeleteActivityAsync("user1", 1, 1);
        var remainingActivities = await _dbContext.TrainingActivities
            .Where(a => a.TrainingSessionId == 1)
            .OrderBy(a => a.Order)
            .ToListAsync();

        // Assert
        result.Should().BeTrue();
        remainingActivities.Should().HaveCount(1);
        remainingActivities[0].Order.Should().Be(1);
    }

    [Fact]
    public async Task DeleteActivityAsync_WithInvalidId_ReturnsFalse()
    {
        // Act
        var result = await _sut.DeleteActivityAsync("user1", 1, 999);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateActivitiesOrderAsync_UpdatesOrder()
    {
        // Arrange
        var newOrders = new[]
        {
            (ActivityId: 1, NewOrder: 2),
            (ActivityId: 2, NewOrder: 1)
        };

        // Act
        var result = await _sut.UpdateActivitiesOrderAsync("user1", 1, newOrders);
        var activities = await _dbContext.TrainingActivities
            .Where(a => a.TrainingSessionId == 1)
            .OrderBy(a => a.Order)
            .ToListAsync();

        // Assert
        result.Should().BeTrue();
        activities[0].Id.Should().Be(2);
        activities[1].Id.Should().Be(1);
    }

    [Fact]
    public async Task UpdateActivitiesOrderAsync_WithInvalidSession_ReturnsFalse()
    {
        // Arrange
        var newOrders = new[] { (ActivityId: 1, NewOrder: 1) };

        // Act
        var result = await _sut.UpdateActivitiesOrderAsync("user1", 999, newOrders);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateActivitiesOrderAsync_WithInvalidIds_PreservesExistingActivities()
    {
        // Arrange
        var newOrders = new[]
        {
            (ActivityId: 999, NewOrder: 1),
            (ActivityId: 998, NewOrder: 2)
        };

        // Act
        var result = await _sut.UpdateActivitiesOrderAsync("user1", 1, newOrders);

        // Assert
        result.Should().BeTrue(); // Operation succeeds but no changes made
        var activities = await _dbContext.TrainingActivities
            .Where(a => a.TrainingSessionId == 1)
            .OrderBy(a => a.Order)
            .ToListAsync();
        activities.Should().HaveCount(2);
        activities[0].Id.Should().Be(1);
        activities[1].Id.Should().Be(2);
    }

    [Fact]
    public async Task GetActivityByIdAsync_WithWrongUser_ReturnsNull()
    {
        // Act
        var result = await _sut.GetActivityByIdAsync("wrong-user", 1, 1);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateActivityAsync_SetsProperInitialOrder()
    {
        // Arrange
        var activity1 = new TrainingActivity
        {
            ExerciseId = 1,
            Sets = new List<ExerciseSet> { new() { Repetitions = 10 } }
        };
        var activity2 = new TrainingActivity
        {
            ExerciseId = 1,
            Sets = new List<ExerciseSet> { new() { Repetitions = 10 } }
        };

        // Act
        var result1 = await _sut.CreateActivityAsync("user1", 1, activity1);
        var result2 = await _sut.CreateActivityAsync("user1", 1, activity2);

        // Assert
        result1.Order.Should().Be(3); // After existing 2 activities
        result2.Order.Should().Be(4);
    }

    [Fact]
    public async Task GetSessionActivitiesAsync_IncludesExerciseAndSets()
    {
        // Act
        var result = await _sut.GetSessionActivitiesAsync("user1", 1);
        var firstActivity = result.First();

        // Assert
        firstActivity.Exercise.Should().NotBeNull();
        firstActivity.Exercise.Name.Should().Be("Squat");
        firstActivity.Sets.Should().NotBeNull();
        firstActivity.Sets.Should().HaveCount(2);
    }

    [Fact]
    public async Task DeleteActivityAsync_CascadeDeletesSets()
    {
        // Arrange
        var activityId = 1;
        var setIds = await _dbContext.ExerciseSets
            .Where(s => s.ActivityId == activityId)
            .Select(s => s.Id)
            .ToListAsync();

        // Act
        await _sut.DeleteActivityAsync("user1", 1, activityId);

        // Assert
        var remainingSets = await _dbContext.ExerciseSets
            .Where(s => setIds.Contains(s.Id))
            .ToListAsync();
        remainingSets.Should().BeEmpty();
    }

    [Fact]
    public async Task UpdateActivityAsync_PreservesOrder()
    {
        // Arrange
        var activity = await _dbContext.TrainingActivities
            .Include(a => a.Sets)
            .FirstAsync(a => a.Id == 1);
        var originalOrder = activity.Order;

        var updatedActivity = new TrainingActivity
        {
            ExerciseId = 1,
            Sets = new List<ExerciseSet>
            {
                new() { Repetitions = 15 }
            }
        };

        // Act
        var result = await _sut.UpdateActivityAsync("user1", 1, 1, updatedActivity);

        // Assert
        result.Should().NotBeNull();
        result!.Order.Should().Be(originalOrder);
        result.Sets.Should().NotBeEmpty();
        result.Sets.First().Order.Should().Be(1);
    }

    [Fact]
    public async Task CreateActivityAsync_ValidatesSetsOrder()
    {
        // Arrange
        var activity = new TrainingActivity
        {
            ExerciseId = 1,
            Sets = new List<ExerciseSet>
            {
                new() { Order = 2, Repetitions = 10 },
                new() { Order = 1, Repetitions = 10 }
            }
        };

        // Act
        var result = await _sut.CreateActivityAsync("user1", 1, activity);

        // Assert
        result.Should().NotBeNull();
        result.Sets.Should().NotBeEmpty();
        var setOrders = result.Sets.Select(s => s.Order).ToList();
        setOrders.Should().BeInAscendingOrder();
        setOrders.Should().BeEquivalentTo(new[] { 1, 2 });
    }
}