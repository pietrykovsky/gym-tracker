using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using GymTracker.Data;
using GymTracker.Services;

namespace GymTracker.Tests.Services;

public class DefaultTrainingPlanServiceTests
{
    private readonly DefaultTrainingPlanService _sut;
    private readonly TestApplicationDbContext _dbContext;

    public DefaultTrainingPlanServiceTests()
    {
        var factory = new DbContextFactoryWrapper();
        _dbContext = (TestApplicationDbContext)factory.CreateDbContext();
        _sut = new DefaultTrainingPlanService(factory);
        _dbContext.Database.EnsureDeleted();

        SeedData();
    }

    private void SeedData()
    {
        var category1 = new TrainingPlanCategory { Id = 1, Name = "Strength" };
        var category2 = new TrainingPlanCategory { Id = 2, Name = "Cardio" };
        _dbContext.TrainingPlanCategories.AddRange(category1, category2);

        var plans = new[]
        {
            new DefaultTrainingPlan
            {
                Id = 1,
                Name = "Beginner Strength",
                Description = "Basic strength plan",
                Categories = new List<TrainingPlanCategory> { category1 }
            },
            new DefaultTrainingPlan
            {
                Id = 2,
                Name = "Advanced Cardio",
                Description = "Intensive cardio plan",
                Categories = new List<TrainingPlanCategory> { category2 }
            },
            new DefaultTrainingPlan
            {
                Id = 3,
                Name = "Mixed Training",
                Description = "Combined plan",
                Categories = new List<TrainingPlanCategory> { category1, category2 }
            }
        };

        _dbContext.DefaultTrainingPlans.AddRange(plans);
        _dbContext.SaveChanges();
    }

    [Fact]
    public async Task GetAllPlansAsync_ReturnsAllPlans()
    {
        // Act
        var result = await _sut.GetAllPlansAsync();

        // Assert
        result.Should().HaveCount(3);
        result.Should().BeInAscendingOrder(p => p.Name);
        result.All(p => p.Categories != null).Should().BeTrue();
    }

    [Fact]
    public async Task GetPlansByCategoryAsync_ReturnsPlansInCategory()
    {
        // Act
        var result = await _sut.GetPlansByCategoryAsync(1);

        // Assert
        result.Should().HaveCount(2);
        result.All(p => p.Categories.Any(c => c.Id == 1)).Should().BeTrue();
    }

    [Fact]
    public async Task GetPlanByIdAsync_ReturnsCorrectPlan()
    {
        // Act
        var result = await _sut.GetPlanByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Beginner Strength");
        result.Categories.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetPlanByIdAsync_WithInvalidId_ReturnsNull()
    {
        // Act
        var result = await _sut.GetPlanByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }
}
