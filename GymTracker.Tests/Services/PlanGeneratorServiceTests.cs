using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using GymTracker.Data;
using GymTracker.Services;
using Moq;

namespace GymTracker.Tests.Services;

public class PlanGeneratorServiceTests
{
    private readonly TestableTrainingPlanGenerator _sut;
    private readonly Mock<IDefaultExerciseService> _defaultExerciseService;
    private readonly Mock<IUserMadeExerciseService> _userMadeExerciseService;
    private readonly Mock<ITrainingPlanCategoryService> _categoryService;
    private const string UserId = "user1";

    public PlanGeneratorServiceTests()
    {
        _defaultExerciseService = new Mock<IDefaultExerciseService>();
        _userMadeExerciseService = new Mock<IUserMadeExerciseService>();
        _categoryService = new Mock<ITrainingPlanCategoryService>();

        _sut = new TestableTrainingPlanGenerator(
            _defaultExerciseService.Object,
            _userMadeExerciseService.Object,
            _categoryService.Object);
    }

    [Theory]
    [InlineData(ExperienceLevel.Untrained, 1, WorkoutType.FullBody)]
    [InlineData(ExperienceLevel.Untrained, 3, WorkoutType.FullBody)]
    [InlineData(ExperienceLevel.Trained, 2, WorkoutType.UpperLower)]
    [InlineData(ExperienceLevel.Advanced, 4, WorkoutType.PushPull)]
    public void GetWorkoutType_ShouldReturnCorrectType(
        ExperienceLevel experience, int trainingDays, WorkoutType expected)
    {
        // Act
        var result = _sut.GetWorkoutType(experience, trainingDays);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public async Task GenerateTrainingPlanAsync_FullBody_ShouldGenerateValidPlan()
    {
        // Arrange
        SetupMockData();
        var equipment = new[] { Equipment.Barbell, Equipment.Dumbbell };

        // Act
        var result = await _sut.GenerateTrainingPlanAsync(
            UserId,
            TrainingGoal.Strength,
            ExperienceLevel.Untrained,
            3,
            equipment);

        // Assert
        result.Should().NotBeNull();
        result.UserId.Should().Be(UserId);
        result.Activities.Should().NotBeEmpty();
        // Changed to use simple descending order check
        result.Activities.Select(a => TestableTrainingPlanGenerator.GetComplexityScore(a.Exercise!))
            .Should().BeInDescendingOrder();
        result.Categories.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GenerateTrainingPlanAsync_UpperLower_ShouldGenerateValidPlan()
    {
        // Arrange
        SetupMockData();
        var equipment = new[] { Equipment.Barbell, Equipment.Dumbbell };

        // Act
        var result = await _sut.GenerateTrainingPlanAsync(
            UserId,
            TrainingGoal.Strength,
            ExperienceLevel.Trained,
            2,
            equipment,
            null,
            UpperLowerWorkoutDay.Upper);

        // Assert
        result.Should().NotBeNull();
        result.UserId.Should().Be(UserId);
        result.Activities.Should().NotBeEmpty();

        // Check each activity's primary category
        foreach (var activity in result.Activities)
        {
            activity.Exercise.Should().NotBeNull("because each activity should have an exercise");
            activity.Exercise!.PrimaryCategory.Name.Should().BeOneOf(
                "Chest", "Back", "Shoulders",
                "because upper body workout should only contain chest, back, or shoulder exercises");
        }
    }

    [Fact]
    public async Task GenerateTrainingPlanAsync_PushPull_ShouldGenerateValidPlan()
    {
        // Arrange
        SetupMockData();
        var equipment = new[] { Equipment.Barbell, Equipment.Dumbbell };

        // Act
        var result = await _sut.GenerateTrainingPlanAsync(
            UserId,
            TrainingGoal.Strength,
            ExperienceLevel.Advanced,
            4,
            equipment,
            PushPullWorkoutDay.Push);

        // Assert
        result.Should().NotBeNull();
        result.UserId.Should().Be(UserId);
        result.Activities.Should().NotBeEmpty();
        result.Activities.All(a => a.Exercise?.PrimaryCategory.Name is "Chest" or "Triceps")
            .Should().BeTrue();
    }

    [Theory]
    [InlineData(
        TrainingGoal.Strength,
        ExperienceLevel.Advanced,
        2,                  // 2 days per week
        5,                  // baseSets(4) + 1 for low frequency
        3,                  // baseReps stays constant
        300)]               // baseRest stays same for low frequency
    [InlineData(
        TrainingGoal.Hypertrophy,
        ExperienceLevel.Trained,
        3,                  // 3 days per week
        3,                  // baseSets unchanged for optimal frequency
        8,                  // baseReps stays constant
        75)]                // baseRest(90) - 15 for moderate frequency
    [InlineData(
        TrainingGoal.Endurance,
        ExperienceLevel.Untrained,
        1,                  // 1 day per week
        4,                  // baseSets(2) + 2 for very low frequency
        15,                 // baseReps stays constant
        45)]                // baseRest unchanged for low frequency
    public void GetTrainingParameters_ShouldReturnValidParameters(
        TrainingGoal goal,
        ExperienceLevel experience,
        int trainingDays,
        int expectedSets,
        int expectedReps,
        int expectedRest)
    {
        // Act
        var (sets, reps, rest) = TestableTrainingPlanGenerator.GetParameters(goal, experience, trainingDays);

        // Assert
        sets.Should().Be(expectedSets,
            "because set count is adjusted based on training frequency");
        reps.Should().Be(expectedReps,
            "because rep count remains constant regardless of frequency");
        rest.Should().Be(expectedRest,
            "because rest period is adjusted based on training frequency");
    }

    [Fact]
    public void GetExerciseComplexityScore_ShouldReturnCorrectScore()
    {
        // Arrange
        var compoundExercise = new DefaultExercise
        {
            Name = "Squat",
            Categories = new List<ExerciseCategory>
            {
                new() { Name = "Legs" },
                new() { Name = "Core" }
            }
        };

        var isolationExercise = new DefaultExercise
        {
            Name = "Bicep Curl",
            Categories = new List<ExerciseCategory>
            {
                new() { Name = "Biceps" }
            }
        };

        // Act
        var compoundScore = TestableTrainingPlanGenerator.GetComplexityScore(compoundExercise);
        var isolationScore = TestableTrainingPlanGenerator.GetComplexityScore(isolationExercise);

        // Assert
        compoundScore.Should().BeGreaterThan(isolationScore);
    }

    [Fact]
    public void UpdatePlanWithWeights_ShouldUpdateWeightsCorrectly()
    {
        // Arrange
        var plan = new UserMadeTrainingPlan
        {
            Activities = new List<PlanActivity>
            {
                new()
                {
                    ExerciseId = 1,
                    Exercise = new DefaultExercise
                    {
                        RequiredEquipment = Equipment.Barbell,
                        Name = "Squat"
                    },
                    Sets = new List<ExerciseSet>
                    {
                        new() { Order = 1 },
                        new() { Order = 2 }
                    }
                }
            }
        };

        var repMaxes = new Dictionary<int, float> { { 1, 100f } };

        // Act
        _sut.UpdatePlanWithWeights(plan, repMaxes, TrainingGoal.Strength, ExperienceLevel.Advanced);

        // Assert
        plan.Activities.First().Sets.Should().OnlyContain(s => s.Weight.HasValue);
        plan.Activities.First().Sets.Should().BeInAscendingOrder(s => s.Weight);
    }

    private void SetupMockData()
    {
        var categories = new List<ExerciseCategory>
        {
            new() { Id = 1, Name = "Chest" },
            new() { Id = 2, Name = "Back" },
            new() { Id = 3, Name = "Shoulders" },
            new() { Id = 4, Name = "Legs" },
            new() { Id = 5, Name = "Triceps" },
            new() { Id = 6, Name = "Core" },
            new() { Id = 7, Name = "Biceps" }
        };

        var exercises = new List<DefaultExercise>
        {
            new()
            {
                Id = 1,
                Name = "Bench Press",
                PrimaryCategory = categories[0], // Chest
                Categories = new List<ExerciseCategory> { categories[4], categories[2] }, // Triceps, Shoulders
                RequiredEquipment = Equipment.Barbell
            },
            new()
            {
                Id = 2,
                Name = "Barbell Row",
                PrimaryCategory = categories[1], // Back
                Categories = new List<ExerciseCategory> { categories[6], categories[5] }, // Biceps, Core
                RequiredEquipment = Equipment.Barbell
            },
            new()
            {
                Id = 3,
                Name = "Military Press",
                PrimaryCategory = categories[2], // Shoulders
                Categories = new List<ExerciseCategory> { categories[4] }, // Triceps
                RequiredEquipment = Equipment.Barbell
            }
        };

        var planCategories = new List<TrainingPlanCategory>
        {
            new() { Id = 1, Name = "Strength" },
            new() { Id = 2, Name = "Full Body" },
            new() { Id = 3, Name = "Upper/Lower" }
        };

        _defaultExerciseService.Setup(x => x.GetAllExercisesAsync())
            .ReturnsAsync(exercises);

        _userMadeExerciseService.Setup(x => x.GetUserExercisesAsync(It.IsAny<string>()))
            .ReturnsAsync(new List<UserMadeExercise>());

        _categoryService.Setup(x => x.GetAllCategoriesAsync())
            .ReturnsAsync(planCategories);
    }
}
