using FluentAssertions;
using GymTracker.Data;
using GymTracker.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GymTracker.Tests.Services;

public class DefaultExerciseServiceTests
{
    private readonly DefaultExerciseService _sut;
    private readonly TestApplicationDbContext _dbContext;

    public DefaultExerciseServiceTests()
    {
        var factory = new DbContextFactoryWrapper();
        _dbContext = (TestApplicationDbContext)factory.CreateDbContext();
        _sut = new DefaultExerciseService(factory);
        _dbContext.Database.EnsureDeleted();

        SeedData();
    }

    private void SeedData()
    {
        var category1 = new ExerciseCategory { Id = 1, Name = "Category1" };
        var category2 = new ExerciseCategory { Id = 2, Name = "Category2" };

        _dbContext.ExerciseCategories.AddRange(category1, category2);

        _dbContext.DefaultExercises.AddRange(
            new DefaultExercise
            {
                Id = 1,
                Name = "Exercise1",
                Difficulty = ExerciseDifficulty.Beginner,
                Categories = new List<ExerciseCategory> { category1 }
            },
            new DefaultExercise
            {
                Id = 2,
                Name = "Exercise2",
                Difficulty = ExerciseDifficulty.Intermediate,
                Categories = new List<ExerciseCategory> { category1, category2 }
            },
            new DefaultExercise
            {
                Id = 3,
                Name = "Exercise3",
                Difficulty = ExerciseDifficulty.Advanced,
                Categories = new List<ExerciseCategory> { category2 }
            }
        );

        _dbContext.SaveChanges();
    }

    [Fact]
    public async Task GetAllExercisesAsync_ReturnsAllExercises()
    {
        // Act
        var result = await _sut.GetAllExercisesAsync();

        // Assert
        result.Should().HaveCount(3);
        result.Should().BeInAscendingOrder(e => e.Name);
        result.All(e => e.Categories != null).Should().BeTrue();
    }

    [Fact]
    public async Task GetAllExercisesByCategoryAsync_ReturnsExercisesInCategory()
    {
        // Act
        var result = await _sut.GetAllExercisesByCategoryAsync(1);

        // Assert
        result.Should().HaveCount(2);
        result.All(e => e.Categories.Any(c => c.Id == 1)).Should().BeTrue();
    }

    [Fact]
    public async Task GetExerciseByIdAsync_ReturnsCorrectExercise()
    {
        // Act
        var result = await _sut.GetExerciseByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Exercise1");
        result.Categories.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetExercisesByDifficultyAsync_ReturnsExercisesWithSpecifiedDifficulty()
    {
        // Act
        var result = await _sut.GetExercisesByDifficultyAsync(ExerciseDifficulty.Intermediate);

        // Assert
        result.Should().HaveCount(1);
        result.Single().Difficulty.Should().Be(ExerciseDifficulty.Intermediate);
    }

    [Fact]
    public async Task GetExercisesByMaxDifficultyAsync_ReturnsExercisesUpToSpecifiedDifficulty()
    {
        // Act
        var result = await _sut.GetExercisesByMaxDifficultyAsync(ExerciseDifficulty.Intermediate);

        // Assert
        result.Should().HaveCount(2);
        result.All(e => e.Difficulty <= ExerciseDifficulty.Intermediate).Should().BeTrue();
        result.Should().BeInAscendingOrder(e => e.Difficulty);
    }

    [Fact]
    public async Task GetExercisesByDifficultyRangeAsync_ReturnsExercisesWithinRange()
    {
        // Act
        var result = await _sut.GetExercisesByDifficultyRangeAsync(
            ExerciseDifficulty.Beginner,
            ExerciseDifficulty.Intermediate);

        // Assert
        result.Should().HaveCount(2);
        result.All(e => e.Difficulty >= ExerciseDifficulty.Beginner &&
                       e.Difficulty <= ExerciseDifficulty.Intermediate)
            .Should().BeTrue();
    }

    [Fact]
    public async Task GetExercisesByDifficultyRangeAsync_WithInvalidRange_ThrowsArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _sut.GetExercisesByDifficultyRangeAsync(
                ExerciseDifficulty.Advanced,
                ExerciseDifficulty.Beginner));
    }

    [Fact]
    public async Task GetAllExercisesAsync_WithNoExercises_ReturnsEmptyList()
    {
        // Arrange
        _dbContext.DefaultExercises.RemoveRange(_dbContext.DefaultExercises);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _sut.GetAllExercisesAsync();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetExerciseByIdAsync_WithInvalidId_ReturnsNull()
    {
        // Act
        var result = await _sut.GetExerciseByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }
}