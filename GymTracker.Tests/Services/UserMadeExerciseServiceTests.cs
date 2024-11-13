using FluentAssertions;
using GymTracker.Data;
using GymTracker.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
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
        var category1 = new ExerciseCategory { Id = 1, Name = "Category1" };
        var category2 = new ExerciseCategory { Id = 2, Name = "Category2" };

        _dbContext.ExerciseCategories.AddRange(category1, category2);

        _dbContext.UserMadeExercises.AddRange(
            new UserMadeExercise
            {
                Id = 1,
                Name = "Exercise1",
                UserId = "user1",
                Difficulty = ExerciseDifficulty.Beginner,
                Categories = new List<ExerciseCategory> { category1 }
            },
            new UserMadeExercise
            {
                Id = 2,
                Name = "Exercise2",
                UserId = "user1",
                Difficulty = ExerciseDifficulty.Intermediate,
                Categories = new List<ExerciseCategory> { category1, category2 }
            },
            new UserMadeExercise
            {
                Id = 3,
                Name = "Exercise3",
                UserId = "user2",
                Difficulty = ExerciseDifficulty.Advanced,
                Categories = new List<ExerciseCategory> { category2 }
            }
        );

        _dbContext.SaveChanges();
    }

    [Fact]
    public async Task GetUserExercisesAsync_ReturnsUserExercises()
    {
        // Act
        var result = await _sut.GetUserExercisesAsync("user1");

        // Assert
        result.Should().HaveCount(2);
        result.All(e => e.UserId == "user1").Should().BeTrue();
        result.Should().BeInAscendingOrder(e => e.Name);
    }

    [Fact]
    public async Task GetUserExercisesByCategoryAsync_ReturnsExercisesInCategory()
    {
        // Act
        var result = await _sut.GetUserExercisesByCategoryAsync("user1", 1);

        // Assert
        result.Should().HaveCount(2);
        result.All(e => e.Categories.Any(c => c.Id == 1)).Should().BeTrue();
        result.All(e => e.UserId == "user1").Should().BeTrue();
    }

    [Fact]
    public async Task GetUserExercisesByDifficultyAsync_ReturnsExercisesWithSpecifiedDifficulty()
    {
        // Act
        var result = await _sut.GetUserExercisesByDifficultyAsync("user1", ExerciseDifficulty.Intermediate);

        // Assert
        result.Should().HaveCount(1);
        result.Single().Difficulty.Should().Be(ExerciseDifficulty.Intermediate);
        result.Single().UserId.Should().Be("user1");
    }

    [Fact]
    public async Task GetUserExerciseByIdAsync_ReturnsCorrectExercise()
    {
        // Act
        var result = await _sut.GetUserExerciseByIdAsync("user1", 1);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Exercise1");
        result.UserId.Should().Be("user1");
        result.Categories.Should().HaveCount(1);
    }

    [Fact]
    public async Task CreateUserExerciseAsync_CreatesNewExercise()
    {
        // Arrange
        var newExercise = new UserMadeExercise
        {
            Name = "NewExercise",
            Difficulty = ExerciseDifficulty.Beginner,
            Description = "Test exercise"
        };

        // Act
        var result = await _sut.CreateUserExerciseAsync("user1", newExercise, new[] { 1 });

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
        result.UserId.Should().Be("user1");
        result.Categories.Should().HaveCount(1);
        result.Categories.First().Id.Should().Be(1);
    }

    [Fact]
    public async Task CreateUserExerciseAsync_WithInvalidCategory_ThrowsArgumentException()
    {
        // Arrange
        var newExercise = new UserMadeExercise
        {
            Name = "NewExercise",
            Difficulty = ExerciseDifficulty.Beginner
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _sut.CreateUserExerciseAsync("user1", newExercise, new[] { 999 }));
    }

    [Fact]
    public async Task UpdateUserExerciseAsync_UpdatesExercise()
    {
        // Arrange
        var updatedExercise = new UserMadeExercise
        {
            Name = "UpdatedExercise",
            Description = "Updated description",
            Difficulty = ExerciseDifficulty.Advanced
        };

        // Act
        var result = await _sut.UpdateUserExerciseAsync("user1", 1, updatedExercise, new[] { 2 });

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("UpdatedExercise");
        result.Description.Should().Be("Updated description");
        result.Difficulty.Should().Be(ExerciseDifficulty.Advanced);
        result.Categories.Should().HaveCount(1);
        result.Categories.First().Id.Should().Be(2);
    }

    [Fact]
    public async Task UpdateUserExerciseAsync_WithInvalidUserId_ReturnsNull()
    {
        // Arrange
        var updatedExercise = new UserMadeExercise
        {
            Name = "UpdatedExercise",
            Difficulty = ExerciseDifficulty.Advanced
        };

        // Act
        var result = await _sut.UpdateUserExerciseAsync("invalidUser", 1, updatedExercise, new[] { 1 });

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateUserExerciseAsync_WithInvalidExerciseId_ReturnsNull()
    {
        // Arrange
        var updatedExercise = new UserMadeExercise
        {
            Name = "UpdatedExercise",
            Difficulty = ExerciseDifficulty.Advanced
        };

        // Act
        var result = await _sut.UpdateUserExerciseAsync("user1", 999, updatedExercise, new[] { 1 });

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteUserExerciseAsync_DeletesExercise()
    {
        // Act
        var result = await _sut.DeleteUserExerciseAsync("user1", 1);
        var exerciseExists = await _dbContext.UserMadeExercises.AnyAsync(e => e.Id == 1);

        // Assert
        result.Should().BeTrue();
        exerciseExists.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteUserExerciseAsync_WithInvalidId_ReturnsFalse()
    {
        // Act
        var result = await _sut.DeleteUserExerciseAsync("user1", 999);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteUserExerciseAsync_WithInvalidUserId_ReturnsFalse()
    {
        // Act
        var result = await _sut.DeleteUserExerciseAsync("invalidUser", 1);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetUserExercisesAsync_WithNoExercises_ReturnsEmptyList()
    {
        // Act
        var result = await _sut.GetUserExercisesAsync("user3");

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetUserExercisesByCategoryAsync_WithNoMatchingExercises_ReturnsEmptyList()
    {
        // Arrange
        var newCategory = new ExerciseCategory { Id = 3, Name = "EmptyCategory" };
        await _dbContext.ExerciseCategories.AddAsync(newCategory);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _sut.GetUserExercisesByCategoryAsync("user1", 3);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task CreateUserExerciseAsync_WithEmptyCategories_ThrowsArgumentException()
    {
        // Arrange
        var newExercise = new UserMadeExercise
        {
            Name = "NewExercise",
            Difficulty = ExerciseDifficulty.Beginner
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _sut.CreateUserExerciseAsync("user1", newExercise, Array.Empty<int>()));
    }
}