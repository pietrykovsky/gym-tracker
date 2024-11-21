using FluentAssertions;
using GymTracker.Components.Analytics;
using GymTracker.Services;
using Moq;
using BlazorBootstrap;
using System;
using System.Collections.Generic;

namespace GymTracker.Tests.Components.Analytics;

public class BodyAnalyticsTests : TestContextBase
{
    private readonly Mock<IAnalyticsService> _analyticsMock;
    private readonly string _userId = "testUser";
    private readonly DateTime _startDate = DateTime.Today.AddMonths(-1);
    private readonly DateTime _endDate = DateTime.Today;

    public BodyAnalyticsTests()
    {
        _analyticsMock = new Mock<IAnalyticsService>();
        Services.AddSingleton(_analyticsMock.Object);
    }

    [Fact]
    public void BodyAnalytics_ShouldRenderCharts_WhenDataLoaded()
    {
        // Arrange
        SetupMockData();

        // Act
        var cut = RenderComponent<BodyAnalytics>(parameters => parameters
            .Add(p => p.UserId, _userId)
            .Add(p => p.StartDate, _startDate)
            .Add(p => p.EndDate, _endDate));

        // Assert
        cut.FindAll(".card-title").Count.Should().Be(3);
        cut.FindAll(".card").Count.Should().Be(3);
    }

    private void SetupMockData()
    {
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

        _analyticsMock.Setup(x => x.GetBodyMeasurementsChart(
            It.IsAny<string>(),
            It.IsAny<DateOnly>(),
            It.IsAny<DateOnly>()))
            .ReturnsAsync(mockChartData);

        _analyticsMock.Setup(x => x.GetBodyCompositionChart(
            It.IsAny<string>(),
            It.IsAny<DateOnly>(),
            It.IsAny<DateOnly>()))
            .ReturnsAsync(mockChartData);

        _analyticsMock.Setup(x => x.GetBodyCircumferencesChart(
            It.IsAny<string>(),
            It.IsAny<DateOnly>(),
            It.IsAny<DateOnly>()))
            .ReturnsAsync(mockChartData);
    }
}
