using FluentAssertions;
using GymTracker.Data;
using GymTracker.Services;
using System.Linq;
using System.Threading.Tasks;
using BlazorBootstrap;
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace GymTracker.Tests.Services;

public class AnalyticsServiceTests
{
    private readonly AnalyticsService _sut;
    private readonly TestApplicationDbContext _dbContext;

    private readonly DateOnly _startDate = new(2024, 1, 1);
    private readonly DateOnly _endDate = new(2024, 1, 31);
    private const string UserId = "user1";

    public AnalyticsServiceTests()
    {
        var factory = new DbContextFactoryWrapper();
        _dbContext = (TestApplicationDbContext)factory.CreateDbContext();
        _sut = new AnalyticsService(factory);
        _dbContext.Database.EnsureDeleted();

        SeedTestData();
    }

    private void SeedTestData()
    {
        var exercise = new DefaultExercise { Id = 1, Name = "Bench Press" };
        _dbContext.DefaultExercises.Add(exercise);

        var sessions = new[]
        {
            new TrainingSession
            {
                Id = 1,
                UserId = UserId,
                Date = new DateOnly(2024, 1, 1),
                Activities = new List<TrainingActivity>
                {
                    new()
                    {
                        ExerciseId = 1,
                        Exercise = exercise,
                        Sets = new List<ExerciseSet>
                        {
                            new() { Order = 1, Repetitions = 10, Weight = 100 },
                            new() { Order = 2, Repetitions = 8, Weight = 120 }
                        }
                    }
                }
            },
            new TrainingSession
            {
                Id = 2,
                UserId = UserId,
                Date = new DateOnly(2024, 1, 15),
                Activities = new List<TrainingActivity>
                {
                    new()
                    {
                        ExerciseId = 1,
                        Exercise = exercise,
                        Sets = new List<ExerciseSet>
                        {
                            new() { Order = 1, Repetitions = 12, Weight = 110 },
                            new() { Order = 2, Repetitions = 10, Weight = 130 }
                        }
                    }
                }
            }
        };

        _dbContext.TrainingSessions.AddRange(sessions);

        var measurements = new[]
        {
            new BodyMeasurement
            {
                Id = 1,
                UserId = UserId,
                Date = new DateOnly(2024, 1, 1),
                Weight = 80,
                Height = 180,
                FatMassPercentage = 20,
                MuscleMassPercentage = 40,
                WaistCircumference = 90,
                ChestCircumference = 100,
                ArmCircumference = 35,
                ThighCircumference = 60
            },
            new BodyMeasurement
            {
                Id = 2,
                UserId = UserId,
                Date = new DateOnly(2024, 1, 15),
                Weight = 79,
                Height = 180,
                FatMassPercentage = 19,
                MuscleMassPercentage = 41,
                WaistCircumference = 89,
                ChestCircumference = 101,
                ArmCircumference = 36,
                ThighCircumference = 61
            }
        };

        _dbContext.BodyMeasurements.AddRange(measurements);
        _dbContext.SaveChanges();
    }

    [Fact]
    public async Task GetUserExercisesInRange_ReturnsCorrectExerciseSummaries()
    {
        // Act
        var result = await _sut.GetUserExercisesInRange(UserId, _startDate, _endDate);

        // Assert
        result.Should().HaveCount(1);
        result.Single().Should().Match<ExerciseAnalyticsSummary>(x =>
            x.ExerciseId == 1 &&
            x.ExerciseName == "Bench Press" &&
            x.TimesPerformed == 2);
    }

    [Fact]
    public async Task GetExerciseProgressionChart_ReturnsCorrectChartData()
    {
        // Act
        var result = await _sut.GetExerciseProgressionChart(UserId, 1, _startDate, _endDate);

        // Assert
        result.Labels.Should().HaveCount(2);
        result.Datasets.Should().HaveCount(3); // Max Weight, Average Weight, Estimated 1RM

        var maxWeightDataset = result.Datasets!.First() as LineChartDataset;
        maxWeightDataset.Should().NotBeNull();
        maxWeightDataset!.Data.Should().BeEquivalentTo(new double?[] { 120, 130 });

        var avgWeightDataset = result.Datasets!.ElementAt(1) as LineChartDataset;
        avgWeightDataset.Should().NotBeNull();
        avgWeightDataset!.Data.Should().BeEquivalentTo(new double?[] { 110, 120 });
    }

    [Fact]
    public async Task GetExerciseVolumeChart_ReturnsCorrectVolumeData()
    {
        // Act
        var result = await _sut.GetExerciseVolumeChart(UserId, 1, _startDate, _endDate);

        // Assert
        result.Labels.Should().HaveCount(2);
        result.Datasets.Should().ContainSingle();

        var dataset = result.Datasets!.Single() as LineChartDataset;
        dataset.Should().NotBeNull();
        dataset!.Label.Should().Be("Volume Load (kg)");

        // First session: (10 * 100) + (8 * 120) = 1960
        // Second session: (12 * 110) + (10 * 130) = 2620
        dataset.Data.Should().BeEquivalentTo(new double?[] { 1960, 2620 });
    }

    [Fact]
    public async Task GetExerciseSetsRepsDistribution_ReturnsCorrectDistribution()
    {
        // Act
        var result = await _sut.GetExerciseSetsRepsDistribution(UserId, 1, _startDate, _endDate);

        // Assert
        result.Labels.Should().BeEquivalentTo(new[] { "8", "10", "12" });
        result.Datasets.Should().ContainSingle();

        var dataset = result.Datasets!.Single() as BarChartDataset;
        dataset.Should().NotBeNull();
        dataset!.Label.Should().Be("Sets per Rep Range");
        dataset.Data.Should().BeEquivalentTo(new double?[] { 1, 2, 1 }); // 1x8 reps, 2x10 reps, 1x12 reps
    }

    [Fact]
    public async Task GetBodyMeasurementsChart_ReturnsCorrectMeasurementData()
    {
        // Act
        var result = await _sut.GetBodyMeasurementsChart(UserId, _startDate, _endDate);

        // Assert
        result.Labels.Should().HaveCount(2);
        result.Datasets.Should().HaveCount(2); // Weight and BMI

        var weightDataset = result.Datasets!.First() as LineChartDataset;
        weightDataset.Should().NotBeNull();
        weightDataset!.Label.Should().Be("Weight (kg)");
        weightDataset.Data.Should().BeEquivalentTo(new double?[] { 80, 79 });

        var bmiDataset = result.Datasets!.ElementAt(1) as LineChartDataset;
        bmiDataset.Should().NotBeNull();
        bmiDataset!.Label.Should().Be("BMI");
        bmiDataset.Data.Should().HaveCount(2);
        ((double)bmiDataset.Data[0]!).Should().BeApproximately(24.69, 0.01);
        ((double)bmiDataset.Data[1]!).Should().BeApproximately(24.38, 0.01);
    }

    [Fact]
    public async Task GetBodyCircumferencesChart_ReturnsCorrectCircumferenceData()
    {
        // Act
        var result = await _sut.GetBodyCircumferencesChart(UserId, _startDate, _endDate);

        // Assert
        result.Labels.Should().HaveCount(2);
        result.Datasets.Should().HaveCount(4); // Chest, Waist, Arm, Thigh

        var chestDataset = result.Datasets!.First() as LineChartDataset;
        chestDataset.Should().NotBeNull();
        chestDataset!.Label.Should().Be("Chest (cm)");
        chestDataset.Data.Should().BeEquivalentTo(new double?[] { 100, 101 });

        var waistDataset = result.Datasets!.ElementAt(1) as LineChartDataset;
        waistDataset.Should().NotBeNull();
        waistDataset!.Label.Should().Be("Waist (cm)");
        waistDataset.Data.Should().BeEquivalentTo(new double?[] { 90, 89 });
    }

    [Fact]
    public async Task GetBodyCompositionChart_ReturnsCorrectCompositionData()
    {
        // Act
        var result = await _sut.GetBodyCompositionChart(UserId, _startDate, _endDate);

        // Assert
        result.Labels.Should().HaveCount(2);
        result.Datasets.Should().HaveCount(2); // Fat Mass and Muscle Mass

        var fatMassDataset = result.Datasets!.First() as LineChartDataset;
        fatMassDataset.Should().NotBeNull();
        fatMassDataset!.Label.Should().Be("Fat Mass (%)");
        fatMassDataset.Data.Should().BeEquivalentTo(new double?[] { 20, 19 });

        var muscleMassDataset = result.Datasets!.ElementAt(1) as LineChartDataset;
        muscleMassDataset.Should().NotBeNull();
        muscleMassDataset!.Label.Should().Be("Muscle Mass (%)");
        muscleMassDataset.Data.Should().BeEquivalentTo(new double?[] { 40, 41 });
    }

    [Fact]
    public async Task GetUserExercisesInRange_WithNoData_ReturnsEmptyList()
    {
        // Arrange
        var emptyDateRange = new DateOnly(2023, 1, 1);

        // Act
        var result = await _sut.GetUserExercisesInRange(UserId, emptyDateRange, emptyDateRange);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetExerciseProgressionChart_WithInvalidExercise_ReturnsEmptyDatasets()
    {
        // Act
        var result = await _sut.GetExerciseProgressionChart(UserId, 999, _startDate, _endDate);

        // Assert
        result.Labels.Should().BeEmpty();
        result.Datasets.Should().HaveCount(3);
        result.Datasets!.Cast<LineChartDataset>().All(d => d.Data!.Count == 0).Should().BeTrue();
    }

    [Fact]
    public async Task GetBodyMeasurementsChart_WithNoMeasurements_ReturnsEmptyDatasets()
    {
        // Arrange
        _dbContext.Database.EnsureDeleted();

        // Act
        var result = await _sut.GetBodyMeasurementsChart(UserId, _startDate, _endDate);

        // Assert
        result.Labels.Should().BeEmpty();
        result.Datasets.Should().BeEmpty();
    }
}