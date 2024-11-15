using FluentAssertions;
using GymTracker.Data;
using GymTracker.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
        // First clear any existing data
        _dbContext.ExerciseSets.RemoveRange(_dbContext.ExerciseSets);
        _dbContext.DefaultExercises.RemoveRange(_dbContext.DefaultExercises);
        _dbContext.UserMadeExercises.RemoveRange(_dbContext.UserMadeExercises);
        _dbContext.ExerciseCategories.RemoveRange(_dbContext.ExerciseCategories);
        _dbContext.SaveChanges();

        // Create categories first
        var category1 = new ExerciseCategory { Name = "Category1", Description = "Description1" };
        var category2 = new ExerciseCategory { Name = "Category2", Description = "Description2" };
        var category3 = new ExerciseCategory { Name = "Category3", Description = "Description3" };

        _dbContext.ExerciseCategories.AddRange(category1, category2, category3);
        _dbContext.SaveChanges();

        // Create exercises and set up relationships
        var defaultExercise = new DefaultExercise
        {
            Name = "DefaultExercise",
            Categories = new List<ExerciseCategory> { category1 }
        };

        var userExercise = new UserMadeExercise
        {
            Name = "UserExercise",
            UserId = "user1",
            Categories = new List<ExerciseCategory> { category1 }
        };

        _dbContext.DefaultExercises.Add(defaultExercise);
        _dbContext.UserMadeExercises.Add(userExercise);
        _dbContext.SaveChanges();
    }

    [Fact]
    public async Task GetAllCategoriesAsync_ReturnsAllCategories()
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
        result!.Name.Should().Be("Category1");
        result.Description.Should().Be("Description1");
    }

    [Fact]
    public async Task GetCategoryByIdAsync_WithInvalidId_ReturnsNull()
    {
        // Act
        var result = await _sut.GetCategoryByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetCategoryWithExercisesAsync_ReturnsCategoryWithRelatedExercises()
    {
        // Act
        var result = await _sut.GetCategoryWithExercisesAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.DefaultExercises.Should().HaveCount(1);
        result.UserMadeExercises.Should().HaveCount(1);
        result.DefaultExercises.Single().Name.Should().Be("DefaultExercise");
        result.UserMadeExercises.Single().Name.Should().Be("UserExercise");
    }

    [Fact]
    public async Task GetCategoryWithExercisesAsync_WithInvalidId_ReturnsNull()
    {
        // Act
        var result = await _sut.GetCategoryWithExercisesAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetCategoryWithExercisesAsync_WithNoExercises_ReturnsCategoryWithEmptyCollections()
    {
        // Act
        var result = await _sut.GetCategoryWithExercisesAsync(2);

        // Assert
        result.Should().NotBeNull();
        result!.DefaultExercises.Should().BeEmpty();
        result.UserMadeExercises.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllCategoriesAsync_WithNoCategories_ReturnsEmptyList()
    {
        // Arrange
        _dbContext.ExerciseCategories.RemoveRange(_dbContext.ExerciseCategories);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _sut.GetAllCategoriesAsync();

        // Assert
        result.Should().BeEmpty();
    }
}