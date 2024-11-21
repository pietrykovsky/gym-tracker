using FluentAssertions;
using GymTracker.Components.Analytics;
using GymTracker.Services;
using Moq;
using BlazorBootstrap;
using System;
using System.Collections.Generic;


namespace GymTracker.Tests.Components.Analytics;

public class ExerciseAnalyticsTests : TestContextBase
{
    private readonly Mock<IAnalyticsService> _analyticsMock;
    private readonly string _userId = "testUser";
    private readonly DateTime _startDate = DateTime.Today.AddMonths(-1);
    private readonly DateTime _endDate = DateTime.Today;

    public ExerciseAnalyticsTests()
    {
        _analyticsMock = new Mock<IAnalyticsService>();
        Services.AddSingleton(_analyticsMock.Object);
    }

    [Fact]
    public void ExerciseAnalytics_ShouldRenderExerciseSelector_WhenDataLoaded()
    {
        // Arrange
        SetupMockData();

        // Act
        var cut = RenderComponent<ExerciseAnalytics>(parameters => parameters
            .Add(p => p.UserId, _userId)
            .Add(p => p.StartDate, _startDate)
            .Add(p => p.EndDate, _endDate));

        // Assert
        cut.Find("select").Should().NotBeNull();
        var options = cut.FindAll("option");
        options.Count.Should().Be(1);
    }

    [Fact]
    public void ExerciseAnalytics_ShouldRenderCharts_WhenExerciseSelected()
    {
        // Arrange
        SetupMockData();

        // Act
        var cut = RenderComponent<ExerciseAnalytics>(parameters => parameters
            .Add(p => p.UserId, _userId)
            .Add(p => p.StartDate, _startDate)
            .Add(p => p.EndDate, _endDate));

        // Assert
        cut.FindAll(".card-title").Count.Should().Be(3);
        cut.FindAll(".card").Count.Should().Be(3);
    }

    private void SetupMockData()
    {
        var mockExercises = new List<ExerciseAnalyticsSummary>
        {
            new()
            {
                ExerciseId = 1,
                ExerciseName = "Test Exercise",
                TimesPerformed = 10
            }
        };

        var mockChartData = new ChartData
        {
            Labels = new List<string> { "01/01", "01/02" },
            Datasets = new List<IChartDataset>
            {
                new LineChartDataset
                {
                    Label = "Test Dataset",
                    Data = new List<double?> { 10, 20 }
                }
            }
        };

        _analyticsMock.Setup(x => x.GetUserExercisesInRange(
            It.IsAny<string>(),
            It.IsAny<DateOnly>(),
            It.IsAny<DateOnly>()))
            .ReturnsAsync(mockExercises);

        _analyticsMock.Setup(x => x.GetExerciseProgressionChart(
            It.IsAny<string>(),
            It.IsAny<int>(),
            It.IsAny<DateOnly>(),
            It.IsAny<DateOnly>()))
            .ReturnsAsync(mockChartData);

        _analyticsMock.Setup(x => x.GetExerciseVolumeChart(
            It.IsAny<string>(),
            It.IsAny<int>(),
            It.IsAny<DateOnly>(),
            It.IsAny<DateOnly>()))
            .ReturnsAsync(mockChartData);

        _analyticsMock.Setup(x => x.GetExerciseSetsRepsDistribution(
            It.IsAny<string>(),
            It.IsAny<int>(),
            It.IsAny<DateOnly>(),
            It.IsAny<DateOnly>()))
            .ReturnsAsync(mockChartData);
    }
}
