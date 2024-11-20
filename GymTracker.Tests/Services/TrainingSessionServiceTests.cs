using FluentAssertions;
using GymTracker.Data;
using GymTracker.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GymTracker.Tests.Services;

public class TrainingSessionServiceTests
{
    private readonly TrainingSessionService _sut;
    private readonly TestApplicationDbContext _dbContext;

    public TrainingSessionServiceTests()
    {
        var factory = new DbContextFactoryWrapper();
        _dbContext = (TestApplicationDbContext)factory.CreateDbContext();
        _sut = new TrainingSessionService(factory);
        _dbContext.Database.EnsureDeleted();

        SeedData();
    }

    private void SeedData()
    {
        var exercise = new DefaultExercise { Id = 1, Name = "Squat" };
        _dbContext.DefaultExercises.Add(exercise);

        var user1Sessions = new[]
        {
            new TrainingSession
            {
                Id = 1,
                UserId = "user1",
                Date = new DateOnly(2024, 1, 1),
                Activities = new List<TrainingActivity>
                {
                    new()
                    {
                        Order = 1,
                        ExerciseId = 1,
                        Sets = new List<ExerciseSet>
                        {
                            new() { Order = 1, Repetitions = 10, Weight = 100 },
                            new() { Order = 2, Repetitions = 10, Weight = 100 }
                        }
                    }
                }
            },
            new TrainingSession
            {
                Id = 2,
                UserId = "user1",
                Date = new DateOnly(2024, 1, 2),
                Activities = new List<TrainingActivity>
                {
                    new()
                    {
                        Order = 1,
                        ExerciseId = 1,
                        Sets = new List<ExerciseSet>
                        {
                            new() { Order = 1, Repetitions = 8, Weight = 120 }
                        }
                    }
                }
            }
        };

        var user2Session = new TrainingSession
        {
            Id = 3,
            UserId = "user2",
            Date = new DateOnly(2024, 1, 1)
        };

        _dbContext.TrainingSessions.AddRange(user1Sessions);
        _dbContext.TrainingSessions.Add(user2Session);

        // Add a default training plan for testing CreateFromPlan
        var plan = new DefaultTrainingPlan
        {
            Id = 1,
            Name = "Test Plan",
            Activities = new List<PlanActivity>
            {
                new()
                {
                    Order = 1,
                    ExerciseId = 1,
                    Sets = new List<ExerciseSet>
                    {
                        new() { Order = 1, Repetitions = 10, Weight = 100 },
                        new() { Order = 2, Repetitions = 10, Weight = 100 }
                    }
                }
            }
        };

        _dbContext.DefaultTrainingPlans.Add(plan);
        _dbContext.SaveChanges();
    }

    [Fact]
    public async Task GetUserSessionsAsync_ReturnsUserSessions()
    {
        // Act
        var result = await _sut.GetUserSessionsAsync("user1");

        // Assert
        result.Should().HaveCount(2);
        result.Should().BeInDescendingOrder(s => s.Date);
        result.All(s => s.UserId == "user1").Should().BeTrue();
    }

    [Fact]
    public async Task GetSessionAsync_ReturnsCorrectSession()
    {
        // Act
        var result = await _sut.GetSessionAsync("user1", 1);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.UserId.Should().Be("user1");
        result.Activities.Should().HaveCount(1);
        result.Activities.First().Sets.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetSessionsInRangeAsync_ReturnsSessionsInRange()
    {
        // Arrange
        var startDate = new DateOnly(2024, 1, 1);
        var endDate = new DateOnly(2024, 1, 1);

        // Act
        var result = await _sut.GetSessionsInRangeAsync("user1", startDate, endDate);

        // Assert
        result.Should().HaveCount(1);
        result.Single().Date.Should().Be(startDate);
    }

    [Fact]
    public async Task CreateFromPlanAsync_CreatesSessionWithPlanStructure()
    {
        // Arrange
        var date = new DateOnly(2024, 1, 3);

        // Act
        var result = await _sut.CreateFromPlanAsync("user1", 1, date, "Test session");

        // Assert
        result.Should().NotBeNull();
        result.Date.Should().Be(date);
        result.Notes.Should().Be("Test session");
        result.Activities.Should().HaveCount(1);
        result.Activities.First().Sets.Should().HaveCount(2);

        // Verify in database
        var savedSession = await _dbContext.TrainingSessions
            .Include(s => s.Activities)
                .ThenInclude(a => a.Sets)
            .FirstOrDefaultAsync(s => s.Id == result.Id);

        savedSession.Should().NotBeNull();
        savedSession!.Activities.Should().HaveCount(1);
        savedSession.Activities.First().Sets.Should().HaveCount(2);
    }

    [Fact]
    public async Task CreateCustomSessionAsync_CreatesCustomSession()
    {
        // Arrange
        var session = new TrainingSession
        {
            Date = new DateOnly(2024, 1, 3),
            Notes = "Custom session",
            Activities = new List<TrainingActivity>
            {
                new()
                {
                    ExerciseId = 1,
                    Sets = new List<ExerciseSet>
                    {
                        new() { Repetitions = 10, Weight = 100 }
                    }
                }
            }
        };

        // Act
        var result = await _sut.CreateCustomSessionAsync("user1", session);

        // Assert
        result.Should().NotBeNull();
        result.UserId.Should().Be("user1");
        result.Activities.Should().HaveCount(1);
        result.Activities.First().Order.Should().Be(1);
        result.Activities.First().Sets.First().Order.Should().Be(1);
    }

    [Fact]
    public async Task CreateCustomSessionAsync_WithNoActivities_ThrowsException()
    {
        // Arrange
        var session = new TrainingSession
        {
            Date = new DateOnly(2024, 1, 3)
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _sut.CreateCustomSessionAsync("user1", session));
    }

    [Fact]
    public async Task UpdateSessionAsync_UpdatesSession()
    {
        // Arrange
        var updatedSession = new TrainingSession
        {
            Date = new DateOnly(2024, 1, 15),
            Notes = "Updated session",
            Activities = new List<TrainingActivity>
            {
                new()
                {
                    ExerciseId = 1,
                    Sets = new List<ExerciseSet>
                    {
                        new() { Repetitions = 12, Weight = 120 }
                    }
                }
            }
        };

        // Act
        var result = await _sut.UpdateSessionAsync("user1", 1, updatedSession);

        // Assert
        result.Should().NotBeNull();
        result!.Date.Should().Be(new DateOnly(2024, 1, 15));
        result.Notes.Should().Be("Updated session");
        result.Activities.Should().HaveCount(1);
        result.Activities.First().Sets.Should().HaveCount(1);
        result.Activities.First().Sets.First().Repetitions.Should().Be(12);
    }

    [Fact]
    public async Task UpdateSessionAsync_WithInvalidId_ReturnsNull()
    {
        // Act
        var result = await _sut.UpdateSessionAsync("user1", 999, new TrainingSession());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteSessionAsync_DeletesSession()
    {
        // Act
        var result = await _sut.DeleteSessionAsync("user1", 1);

        // Assert
        result.Should().BeTrue();
        (await _dbContext.TrainingSessions.FindAsync(1)).Should().BeNull();
    }

    [Fact]
    public async Task DeleteSessionAsync_WithInvalidId_ReturnsFalse()
    {
        // Act
        var result = await _sut.DeleteSessionAsync("user1", 999);

        // Assert
        result.Should().BeFalse();
    }
}
