using FluentAssertions;
using System.Threading.Tasks;
using GymTracker.Data;
using GymTracker.Services;

namespace GymTracker.Tests.Services;

public class ExerciseCategoryServiceTests
{
    private readonly ExerciseCategoryService _sut;
    private readonly TestApplicationDbContext _dbContext;

    public ExerciseCategoryServiceTests()
    {
        var factory = new DbContextFactoryWrapper();
        _dbContext = (TestApplicationDbContext)factory.CreateDbContext();
        _sut = new ExerciseCategoryService(factory);
        _dbContext.Database.EnsureDeleted();

        SeedData();
    }

    private void SeedData()
    {
        _dbContext.ExerciseCategories.AddRange(
            new ExerciseCategory { Id = 1, Name = "Chest", Description = "Chest exercises" },
            new ExerciseCategory { Id = 2, Name = "Back", Description = "Back exercises" },
            new ExerciseCategory { Id = 3, Name = "Legs", Description = "Leg exercises" }
        );
        _dbContext.SaveChanges();
    }

    [Fact]
    public async Task GetAllCategoriesAsync_ReturnsAllCategories_OrderedByName()
    {
        // Act
        var result = await _sut.GetAllCategoriesAsync();

        // Assert
        result.Should().HaveCount(3);
        result.Should().BeInAscendingOrder(c => c.Name);
    }

    [Fact]
    public async Task GetCategoryByIdAsync_ReturnsCorrectCategory()
    {
        // Act
        var result = await _sut.GetCategoryByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.Name.Should().Be("Chest");
    }

    [Fact]
    public async Task GetCategoryByIdAsync_ReturnsNull_WhenNotFound()
    {
        // Act
        var result = await _sut.GetCategoryByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetCategoryWithExercisesAsync_ReturnsCategory_WithExercises()
    {
        // Arrange
        _dbContext.DefaultExercises.Add(
            new DefaultExercise { Id = 1, Name = "Bench Press", CategoryId = 1 }
        );
        _dbContext.UserMadeExercises.Add(
            new UserMadeExercise { Id = 2, Name = "Custom Press", CategoryId = 1, UserId = "user1" }
        );
        _dbContext.SaveChanges();

        // Act
        var result = await _sut.GetCategoryWithExercisesAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.DefaultExercises.Should().HaveCount(1);
        result.UserMadeExercises.Should().HaveCount(1);
    }
}
