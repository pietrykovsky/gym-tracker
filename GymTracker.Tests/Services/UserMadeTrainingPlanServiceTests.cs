using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using GymTracker.Data;
using GymTracker.Services;
namespace GymTracker.Tests.Services;

public class UserMadeTrainingPlanServiceTests
{
    private readonly UserMadeTrainingPlanService _sut;
    private readonly TestApplicationDbContext _dbContext;

    public UserMadeTrainingPlanServiceTests()
    {
        var factory = new DbContextFactoryWrapper();
        _dbContext = (TestApplicationDbContext)factory.CreateDbContext();
        _sut = new UserMadeTrainingPlanService(factory);
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
            new UserMadeTrainingPlan
            {
                Id = 1,
                Name = "My Strength Plan",
                UserId = "user1",
                Categories = new List<TrainingPlanCategory> { category1 }
            },
            new UserMadeTrainingPlan
            {
                Id = 2,
                Name = "My Cardio Plan",
                UserId = "user1",
                Categories = new List<TrainingPlanCategory> { category2 }
            },
            new UserMadeTrainingPlan
            {
                Id = 3,
                Name = "Other User Plan",
                UserId = "user2",
                Categories = new List<TrainingPlanCategory> { category1 }
            }
        };

        _dbContext.UserMadeTrainingPlans.AddRange(plans);
        _dbContext.SaveChanges();
    }

    [Fact]
    public async Task GetUserPlansAsync_ReturnsUserPlans()
    {
        // Act
        var result = await _sut.GetUserPlansAsync("user1");

        // Assert
        result.Should().HaveCount(2);
        result.All(p => p.UserId == "user1").Should().BeTrue();
        result.Should().BeInAscendingOrder(p => p.Name);
    }

    [Fact]
    public async Task GetUserPlansByCategoryAsync_ReturnsPlansInCategory()
    {
        // Act
        var result = await _sut.GetUserPlansByCategoryAsync("user1", 1);

        // Assert
        result.Should().HaveCount(1);
        result.All(p => p.Categories.Any(c => c.Id == 1)).Should().BeTrue();
        result.All(p => p.UserId == "user1").Should().BeTrue();
    }

    [Fact]
    public async Task CreateUserPlanAsync_CreatesNewPlan()
    {
        // Arrange
        var newPlan = new UserMadeTrainingPlan
        {
            Name = "New Plan",
            Description = "Test plan"
        };

        // Act
        var result = await _sut.CreateUserPlanAsync("user1", newPlan, new[] { 1 });

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
        result.UserId.Should().Be("user1");
        result.Categories.Should().HaveCount(1);
    }

    [Fact]
    public async Task UpdateUserPlanAsync_UpdatesPlan()
    {
        // Arrange
        var updatedPlan = new UserMadeTrainingPlan
        {
            Name = "Updated Plan",
            Description = "Updated description"
        };

        // Act
        var result = await _sut.UpdateUserPlanAsync("user1", 1, updatedPlan, new[] { 2 });

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Updated Plan");
        result.Description.Should().Be("Updated description");
        result.Categories.Should().HaveCount(1);
        result.Categories.First().Id.Should().Be(2);
    }

    [Fact]
    public async Task DeleteUserPlanAsync_DeletesPlan()
    {
        // Act
        var result = await _sut.DeleteUserPlanAsync("user1", 1);

        // Assert
        result.Should().BeTrue();
        (await _dbContext.UserMadeTrainingPlans.FindAsync(1)).Should().BeNull();
    }
}