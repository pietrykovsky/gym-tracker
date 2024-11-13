using FluentAssertions;
using GymTracker.Data;
using GymTracker.Services;
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
        var categories = new[]
        {
            new ExerciseCategory { Id = 1, Name = "Chest" },
            new ExerciseCategory { Id = 2, Name = "Back" }
        };
        _dbContext.ExerciseCategories.AddRange(categories);
        _dbContext.SaveChanges();

        _dbContext.DefaultExercises.AddRange(
            new DefaultExercise { Id = 1, Name = "Bench Press", CategoryId = 1 },
            new DefaultExercise { Id = 2, Name = "Push-Ups", CategoryId = 1 },
            new DefaultExercise { Id = 3, Name = "Pull-Ups", CategoryId = 2 }
        );
        _dbContext.SaveChanges();
    }

    [Fact]
    public async Task GetAllExercisesAsync_ReturnsAllExercises_OrderedByCategory()
    {
        // Act
        var result = await _sut.GetAllExercisesAsync();

        // Assert
        result.Should().HaveCount(3);
        result.Select(e => e.Category).Should().BeInAscendingOrder(c => c.Name);
    }

    [Fact]
    public async Task GetExercisesByCategoryAsync_ReturnsExercisesForCategory()
    {
        // Act
        var result = await _sut.GetAllExercisesByCategoryAsync(1);

        // Assert
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(e => e.CategoryId.Should().Be(1));
        result.Should().BeInAscendingOrder(e => e.Name);
    }

    [Fact]
    public async Task GetExerciseByIdAsync_ReturnsCorrectExercise()
    {
        // Act
        var result = await _sut.GetExerciseByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.Name.Should().Be("Bench Press");
        result.Category.Should().NotBeNull();
    }

    [Fact]
    public async Task GetExerciseByIdAsync_ReturnsNull_WhenNotFound()
    {
        // Act
        var result = await _sut.GetExerciseByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }
}

