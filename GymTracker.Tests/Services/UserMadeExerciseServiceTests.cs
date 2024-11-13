using FluentAssertions;
using GymTracker.Data;
using GymTracker.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace GymTracker.Tests.Services;

public class UserMadeExerciseServiceTests
{
    private readonly UserMadeExerciseService _sut;
    private readonly TestApplicationDbContext _dbContext;

    public UserMadeExerciseServiceTests()
    {
        var factory = new DbContextFactoryWrapper();
        _dbContext = (TestApplicationDbContext)factory.CreateDbContext();
        _sut = new UserMadeExerciseService(factory);
        _dbContext.Database.EnsureDeleted();

        SeedData();
    }

    private void SeedData()
    {
        var categories = new[]
        {
            new ExerciseCategory { Id = 1, Name = "Chest" },
            new ExerciseCategory { Id = 2, Name = "Back" }
        };
        _dbContext.ExerciseCategories.AddRange(categories);
        _dbContext.SaveChanges();

        _dbContext.UserMadeExercises.AddRange(
            new UserMadeExercise { Id = 1, Name = "Custom Press", CategoryId = 1, UserId = "user1" },
            new UserMadeExercise { Id = 2, Name = "Custom Fly", CategoryId = 1, UserId = "user1" },
            new UserMadeExercise { Id = 3, Name = "Custom Row", CategoryId = 2, UserId = "user1" },
            new UserMadeExercise { Id = 4, Name = "Other Exercise", CategoryId = 1, UserId = "user2" }
        );
        _dbContext.SaveChanges();
    }

    [Fact]
    public async Task GetUserExercisesAsync_ReturnsExercisesForUser()
    {
        // Act
        var result = await _sut.GetUserExercisesAsync("user1");

        // Assert
        result.Should().HaveCount(3);
        result.Should().AllSatisfy(e => e.UserId.Should().Be("user1"));
        result.Should().BeInAscendingOrder(e => e.Category.Name).And.ThenBeInAscendingOrder(e => e.Name);
    }

    [Fact]
    public async Task GetUserExercisesByCategoryAsync_ReturnsExercisesForUserAndCategory()
    {
        // Act
        var result = await _sut.GetUserExercisesByCategoryAsync("user1", 1);

        // Assert
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(e =>
        {
            e.UserId.Should().Be("user1");
            e.CategoryId.Should().Be(1);
        });
    }

    [Fact]
    public async Task GetUserExerciseByIdAsync_ReturnsCorrectExercise()
    {
        // Act
        var result = await _sut.GetUserExerciseByIdAsync("user1", 1);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.UserId.Should().Be("user1");
        result.Category.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateUserExerciseAsync_AddsExercise()
    {
        // Arrange
        var newExercise = new UserMadeExercise
        {
            Name = "New Exercise",
            CategoryId = 1
        };

        // Act
        var result = await _sut.CreateUserExerciseAsync("user1", newExercise);

        // Assert
        var exercisesCount = _dbContext.UserMadeExercises.Count();
        result.Should().NotBeNull();
        result.UserId.Should().Be("user1");
        result.Name.Should().Be("New Exercise");
        exercisesCount.Should().Be(5);
    }

    [Fact]
    public async Task UpdateUserExerciseAsync_UpdatesExercise()
    {
        // Arrange
        var updatedExercise = new UserMadeExercise
        {
            Name = "Updated Exercise",
            CategoryId = 2
        };

        // Act
        var result = await _sut.UpdateUserExerciseAsync("user1", 1, updatedExercise);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Updated Exercise");
        result.CategoryId.Should().Be(2);
    }

    [Fact]
    public async Task UpdateUserExerciseAsync_ReturnsNull_WhenExerciseNotFound()
    {
        // Arrange
        var updatedExercise = new UserMadeExercise
        {
            Name = "Updated Exercise",
            CategoryId = 2
        };

        // Act
        var result = await _sut.UpdateUserExerciseAsync("user1", 999, updatedExercise);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteUserExerciseAsync_RemovesExercise()
    {
        // Act
        var success = await _sut.DeleteUserExerciseAsync("user1", 1);

        // Assert
        var exercisesCount = _dbContext.UserMadeExercises.Count();
        success.Should().BeTrue();
        exercisesCount.Should().Be(3);
    }

    [Fact]
    public async Task DeleteUserExerciseAsync_ReturnsFalse_WhenExerciseNotFound()
    {
        // Act
        var success = await _sut.DeleteUserExerciseAsync("user1", 999);

        // Assert
        var exercisesCount = _dbContext.UserMadeExercises.Count();
        success.Should().BeFalse();
        exercisesCount.Should().Be(4);
    }
}
