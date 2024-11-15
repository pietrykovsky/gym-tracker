using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using GymTracker.Data;
using GymTracker.Services;
using Microsoft.EntityFrameworkCore;

namespace GymTracker.Tests.Services;

public class PlanActivityServiceTests
{
    private readonly PlanActivityService _sut;
    private readonly TestApplicationDbContext _dbContext;

    public PlanActivityServiceTests()
    {
        var factory = new DbContextFactoryWrapper();
        _dbContext = (TestApplicationDbContext)factory.CreateDbContext();
        _sut = new PlanActivityService(factory);
        _dbContext.Database.EnsureDeleted();

        SeedData();
    }

    private void SeedData()
    {
        var exercise = new DefaultExercise { Id = 1, Name = "Squat" };
        _dbContext.DefaultExercises.Add(exercise);

        var activities = new[]
        {
            new PlanActivity
            {
                Id = 1,
                PlanId = 1,
                ExerciseId = 1,
                Order = 1,
                Sets = new List<ExerciseSet>
                {
                    new() { Id = 1, Order = 1, Repetitions = 10 },
                    new() { Id = 2, Order = 2, Repetitions = 12 }
                }
            },
            new PlanActivity
            {
                Id = 2,
                PlanId = 1,
                ExerciseId = 1,
                Order = 2
            }
        };

        _dbContext.TrainingActivities.AddRange(activities);
        _dbContext.SaveChanges();
    }

    [Fact]
    public async Task GetPlanActivitiesAsync_ReturnsActivities()
    {
        // Act
        var result = await _sut.GetPlanActivitiesAsync(1);

        // Assert
        result.Should().HaveCount(2);
        result.Should().BeInAscendingOrder(a => a.Order);
    }

    [Fact]
    public async Task CreateActivityAsync_CreatesActivity()
    {
        // Arrange
        var newActivity = new PlanActivity
        {
            PlanId = 1,
            ExerciseId = 1
        };

        // Act
        var result = await _sut.CreateActivityAsync(newActivity);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
        result.Order.Should().Be(3); // Should be placed at the end
    }

    [Fact]
    public async Task UpdateActivityAsync_UpdatesActivity()
    {
        // Arrange
        var updatedActivity = new PlanActivity
        {
            ExerciseId = 1,
            Order = 2,
            Sets = new List<ExerciseSet> { new() { Order = 1, Repetitions = 15 } }
        };

        // Act
        var result = await _sut.UpdateActivityAsync(1, 1, updatedActivity);

        // Assert
        result.Should().NotBeNull();
        result!.Sets.Should().HaveCount(1);
        result.Sets.First().Repetitions.Should().Be(15);
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
        var result = await _sut.UpdateActivitiesOrderAsync(1, newOrders);
        var activities = await _dbContext.TrainingActivities
            .Where(a => a.PlanId == 1)
            .OrderBy(a => a.Order)
            .ToListAsync();

        // Assert
        result.Should().BeTrue();
        activities[0].Id.Should().Be(2);
        activities[1].Id.Should().Be(1);
    }
}
