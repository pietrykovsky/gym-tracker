using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using GymTracker.Data;
using GymTracker.Services;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System;
using System.Threading;
using System.Linq;
using System.Collections.Generic;

namespace GymTracker.Tests.Services;

public class DatabaseSeederServiceTests
{
    private readonly DatabaseSeederService _sut;
    private readonly TestApplicationDbContext _dbContext;
    private readonly Mock<ILogger<DatabaseSeederService>> _loggerMock;

    public DatabaseSeederServiceTests()
    {
        var factory = new DbContextFactoryWrapper();
        _dbContext = (TestApplicationDbContext)factory.CreateDbContext();
        _loggerMock = new Mock<ILogger<DatabaseSeederService>>();
        _sut = new DatabaseSeederService(factory, _loggerMock.Object);
        _dbContext.Database.EnsureDeleted();
    }

    [Fact]
    public async Task SeedAsync_WhenDatabaseEmpty_SeedsAllData()
    {
        // Act
        await _sut.SeedAsync();

        // Assert
        var exerciseCategories = await _dbContext.ExerciseCategories.ToListAsync();
        var exercises = await _dbContext.DefaultExercises
            .Include(e => e.Categories)
            .Include(e => e.PrimaryCategory)
            .ToListAsync();
        var trainingPlanCategories = await _dbContext.TrainingPlanCategories.ToListAsync();
        var trainingPlans = await _dbContext.DefaultTrainingPlans
            .Include(p => p.Categories)
            .ToListAsync();
        var activities = await _dbContext.PlanActivities
            .Include(a => a.Sets)
            .ToListAsync();

        // Verify exercise data
        exerciseCategories.Should().NotBeEmpty();
        exerciseCategories.Should().HaveCount(13);
        exercises.Should().NotBeEmpty();
        exercises.Should().HaveCount(151);
        exercises.All(e => e.PrimaryCategory != null).Should().BeTrue();
        exercises.All(e => exerciseCategories.Select(c => c.Id).Contains(e.PrimaryCategory.Id)).Should().BeTrue();
        exercises.All(e => e.Categories.All(c => c.Id != e.PrimaryCategory.Id)).Should().BeTrue();

        // Verify training plan data
        trainingPlanCategories.Should().NotBeEmpty();
        trainingPlanCategories.Should().HaveCount(9); // Full Body, Split Routine, Strength, etc.
        trainingPlans.Should().NotBeEmpty();
        trainingPlans.Should().HaveCountGreaterThan(15); // We have more than 15 different plans

        // Verify training plan relationships and structure
        trainingPlans.All(p => p.Categories.Any()).Should().BeTrue();
        activities.Should().NotBeEmpty();
        activities.All(a => a.Sets.Any()).Should().BeTrue();

        // Verify specific plans exist
        trainingPlans.Should().Contain(p => p.Name == "Full Body Strength");
        trainingPlans.Should().Contain(p => p.Name == "Push Day");
        trainingPlans.Should().Contain(p => p.Name == "HIIT Circuit A");
        trainingPlans.Should().Contain(p => p.Name == "Mobility Flow");

        // Verify plan categories are correct
        var strengthPlan = trainingPlans.First(p => p.Name == "Full Body Strength");
        strengthPlan.Categories.Should().Contain(c => c.Name == "Full Body");
        strengthPlan.Categories.Should().Contain(c => c.Name == "Strength");

        // Verify activities and sets are properly structured
        var strengthPlanActivities = activities.Where(a => a.PlanId == strengthPlan.Id).OrderBy(a => a.Order).ToList();
        strengthPlanActivities.Should().HaveCount(6); // 6 exercises in Full Body Strength
        strengthPlanActivities.All(a => a.Sets.Any()).Should().BeTrue();
        strengthPlanActivities.First().Order.Should().Be(1);
        strengthPlanActivities.Last().Order.Should().Be(6);
    }

    [Fact]
    public async Task SeedAsync_WhenPartialDataExists_OnlyAddsNewData()
    {
        // Arrange
        var category = new TrainingPlanCategory { Name = "Test Category", Description = "Test" };
        var plan = new DefaultTrainingPlan
        {
            Name = "Test Plan",
            Description = "Test",
            Categories = new List<TrainingPlanCategory> { category }
        };

        await _dbContext.TrainingPlanCategories.AddAsync(category);
        await _dbContext.DefaultTrainingPlans.AddAsync(plan);
        await _dbContext.SaveChangesAsync();

        var initialPlanCount = await _dbContext.DefaultTrainingPlans.CountAsync();
        var initialCategoryCount = await _dbContext.TrainingPlanCategories.CountAsync();

        // Act
        await _sut.SeedAsync();

        // Assert
        var categories = await _dbContext.TrainingPlanCategories.ToListAsync();
        var plans = await _dbContext.DefaultTrainingPlans.ToListAsync();

        categories.Count.Should().BeGreaterThan(initialCategoryCount);
        plans.Count.Should().BeGreaterThan(initialPlanCount);

        // Original data should remain unchanged
        var originalCategory = categories.FirstOrDefault(c => c.Name == "Test Category");
        originalCategory.Should().NotBeNull();
        originalCategory!.Description.Should().Be("Test");

        var originalPlan = plans.FirstOrDefault(p => p.Name == "Test Plan");
        originalPlan.Should().NotBeNull();
        originalPlan!.Description.Should().Be("Test");
    }

    [Fact]
    public async Task SeedAsync_CreatesProperExerciseRelationships()
    {
        // Act
        await _sut.SeedAsync();

        // Assert
        var pushDay = await _dbContext.DefaultTrainingPlans
            .Include(p => p.Categories)
            .FirstOrDefaultAsync(p => p.Name == "Push Day");

        pushDay.Should().NotBeNull();
        pushDay!.Categories.Should().Contain(c => c.Name == "Split Routine");

        var activities = await _dbContext.PlanActivities
            .Include(a => a.Sets)
            .Include(a => a.Exercise)
            .Where(a => a.PlanId == pushDay.Id)
            .OrderBy(a => a.Order)
            .ToListAsync();

        activities.Should().HaveCount(5);
        activities.First().Exercise.Name.Should().Be("Flat Barbell Bench Press");
        activities.First().Sets.Should().HaveCount(4);
        activities.All(a => a.Sets.All(s => s.Order > 0)).Should().BeTrue();
    }

    [Fact]
    public async Task SeedAsync_CreatesPlanActivitiesWithCorrectOrdering()
    {
        // Act
        await _sut.SeedAsync();

        // Assert
        var fullBodyPlan = await _dbContext.DefaultTrainingPlans
            .FirstOrDefaultAsync(p => p.Name == "Full Body Strength");

        fullBodyPlan.Should().NotBeNull();

        var activities = await _dbContext.PlanActivities
            .Where(a => a.PlanId == fullBodyPlan!.Id)
            .OrderBy(a => a.Order)
            .ToListAsync();

        activities.Should().HaveCount(6);
        activities.Select(a => a.Order).Should().BeEquivalentTo([1, 2, 3, 4, 5, 6]);
    }

    [Fact]
    public async Task SeedAsync_CreatesSetsWithCorrectParameters()
    {
        // Act
        await _sut.SeedAsync();

        // Assert
        var strengthPlan = await _dbContext.DefaultTrainingPlans
            .FirstOrDefaultAsync(p => p.Name == "Pure Strength Upper");

        strengthPlan.Should().NotBeNull();

        var benchPressActivity = await _dbContext.PlanActivities
            .Include(a => a.Sets)
            .Include(a => a.Exercise)
            .FirstOrDefaultAsync(a => a.PlanId == strengthPlan!.Id && a.Exercise.Name == "Flat Barbell Bench Press");

        benchPressActivity.Should().NotBeNull();
        benchPressActivity!.Sets.Should().HaveCount(4);
        benchPressActivity.Sets.All(s => s.Repetitions == 6).Should().BeTrue();
        benchPressActivity.Sets.Select(s => s.Order).Should().BeEquivalentTo([1, 2, 3, 4]);
    }

    [Fact]
    public async Task SeedAsync_WhenExceptionOccurs_LogsErrorAndRethrows()
    {
        // Arrange
        var failingFactory = new Mock<IDbContextFactory<ApplicationDbContext>>();
        failingFactory.Setup(f => f.CreateDbContextAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Test exception"));

        var seeder = new DatabaseSeederService(failingFactory.Object, _loggerMock.Object);

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => seeder.SeedAsync());

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
    }

    [Fact]
    public async Task SeedAsync_CreatesPlansWithMultipleCategories()
    {
        // Act
        await _sut.SeedAsync();

        // Assert
        var hiitCircuit = await _dbContext.DefaultTrainingPlans
            .Include(p => p.Categories)
            .FirstOrDefaultAsync(p => p.Name == "HIIT Circuit A");

        hiitCircuit.Should().NotBeNull();
        hiitCircuit!.Categories.Should().Contain(c => c.Name == "Weight Loss");
        hiitCircuit.Categories.Should().Contain(c => c.Name == "HIIT");
        hiitCircuit.Categories.Count.Should().Be(2);
    }

    [Fact]
    public async Task SeedAsync_CreatesTimedExerciseSets()
    {
        // Act
        await _sut.SeedAsync();

        // Assert
        var plankActivity = await _dbContext.PlanActivities
            .Include(a => a.Sets)
            .Include(a => a.Exercise)
            .FirstOrDefaultAsync(a => a.Exercise.Name == "Plank");

        plankActivity.Should().NotBeNull();
        plankActivity!.Sets.Should().HaveCount(3);
        plankActivity.Sets.All(s => s.RestAfterDuration == 60).Should().BeTrue();
        plankActivity.Sets.All(s => s.Repetitions == 0).Should().BeTrue(); // Time-based exercise
    }
}