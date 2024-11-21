using FluentAssertions;
using GymTracker.Components.Analytics;
using GymTracker.Services;
using Moq;
using BlazorBootstrap;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;

namespace GymTracker.Tests.Components.Analytics;

public class DateRangeSelectorTests : TestContextBase
{
    [Fact]
    public void DateRangeSelector_ShouldRenderWithInitialDates()
    {
        // Arrange
        var startDate = DateTime.Today.AddMonths(-1);
        var endDate = DateTime.Today;

        // Act
        var cut = RenderComponent<DateRangeSelector>(parameters => parameters
            .Add(p => p.StartDate, startDate)
            .Add(p => p.EndDate, endDate));

        // Assert
        var dateInputs = cut.FindAll("input[type='date']");
        dateInputs.Count.Should().Be(2);
        dateInputs[0].GetAttribute("value").Should().Be(startDate.ToString("yyyy-MM-dd"));
        dateInputs[1].GetAttribute("value").Should().Be(endDate.ToString("yyyy-MM-dd"));
    }

    [Fact]
    public async Task DateRangeSelector_ShouldTriggerCallback_WhenDatesChange()
    {
        // Arrange
        var startDate = DateTime.Today.AddMonths(-1);
        var endDate = DateTime.Today;
        var onDateRangeChangedInvoked = false;
        (DateTime start, DateTime end) capturedRange = default;

        var cut = RenderComponent<DateRangeSelector>(parameters => parameters
            .Add(p => p.StartDate, startDate)
            .Add(p => p.EndDate, endDate)
            .Add(p => p.OnDateRangeChanged, (range) =>
            {
                onDateRangeChangedInvoked = true;
                capturedRange = range;
            }));

        // Act
        var newStartDate = DateTime.Today.AddMonths(-2);
        var startDateInput = cut.Find("input[type='date']:nth-child(1)");
        await startDateInput.ChangeAsync(new ChangeEventArgs
        {
            Value = newStartDate.ToString("yyyy-MM-dd")
        });

        // Assert
        onDateRangeChangedInvoked.Should().BeTrue();
        capturedRange.start.Date.Should().Be(newStartDate.Date);
        capturedRange.end.Should().Be(endDate);
    }
}