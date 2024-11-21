using GymTracker.Components.Dashboard;
using System.Linq;
using FluentAssertions;
using System;
using System.Collections.Generic;

namespace GymTracker.Tests.Components.Dashboard;

public class CalendarTests : TestContext
{
    [Fact]
    public void Calendar_ShouldInitialize_WithCurrentDate()
    {
        // Arrange & Act
        var cut = RenderComponent<Calendar>();

        // Assert
        var today = DateOnly.FromDateTime(DateTime.Today);
        var currentMonthYear = DateTime.Today.ToString("MMMM yyyy");

        cut.Find("[data-testid='month-display']").TextContent.Should().Be(currentMonthYear);
        cut.Find(".today").Should().NotBeNull();
    }

    [Fact]
    public void Calendar_ShouldDisplay_SevenDaysOfWeek()
    {
        // Arrange & Act
        var cut = RenderComponent<Calendar>();

        // Assert
        var daysOfWeek = cut.FindAll(".row:first-child .col").ToList();
        daysOfWeek.Count.Should().Be(7);
        daysOfWeek[0].TextContent.Should().Be("Mon");
        daysOfWeek[6].TextContent.Should().Be("Sun");
    }

    [Fact]
    public void NextMonth_ShouldAdvance_CalendarByOneMonth()
    {
        // Arrange
        var cut = RenderComponent<Calendar>();
        var initialMonth = DateTime.Today;
        var expectedMonth = initialMonth.AddMonths(1).ToString("MMMM yyyy");

        // Act
        cut.Find("[data-testid='next-month-btn']").Click();

        // Assert
        cut.Find("[data-testid='month-display']").TextContent.Should().Be(expectedMonth);
    }

    [Fact]
    public void PreviousMonth_ShouldMove_CalendarBackOneMonth()
    {
        // Arrange
        var cut = RenderComponent<Calendar>();
        var initialMonth = DateTime.Today;
        var expectedMonth = initialMonth.AddMonths(-1).ToString("MMMM yyyy");

        // Act
        cut.Find("[data-testid='prev-month-btn']").Click();

        // Assert
        cut.Find("[data-testid='month-display']").TextContent.Should().Be(expectedMonth);
    }

    [Fact]
    public void NextYear_ShouldAdvance_CalendarByOneYear()
    {
        // Arrange
        var cut = RenderComponent<Calendar>();
        var initialMonth = DateTime.Today;
        var expectedMonth = initialMonth.AddYears(1).ToString("MMMM yyyy");

        // Act
        cut.Find("[data-testid='next-year-btn']").Click();

        // Assert
        cut.Find("[data-testid='month-display']").TextContent.Should().Be(expectedMonth);
    }

    [Fact]
    public void PreviousYear_ShouldMove_CalendarBackOneYear()
    {
        // Arrange
        var cut = RenderComponent<Calendar>();
        var initialMonth = DateTime.Today;
        var expectedMonth = initialMonth.AddYears(-1).ToString("MMMM yyyy");

        // Act
        cut.Find("[data-testid='prev-year-btn']").Click();

        // Assert
        cut.Find("[data-testid='month-display']").TextContent.Should().Be(expectedMonth);
    }

    [Fact]
    public void SelectDate_ShouldUpdate_SelectedDateParameter()
    {
        // Arrange
        DateOnly? selectedDate = null;
        var cut = RenderComponent<Calendar>(parameters => parameters
            .Add(p => p.SelectedDateChanged, (DateOnly? date) => selectedDate = date));

        // Find today's date element and click it
        var today = DateOnly.FromDateTime(DateTime.Today);
        var todayElement = cut.Find($"[data-testid='calendar-day-{today:yyyy-MM-dd}']");

        // Act
        todayElement.Click();

        // Assert
        selectedDate.Should().Be(today);
    }

    [Fact]
    public void Today_ShouldReset_CalendarToCurrentDate()
    {
        // Arrange
        var cut = RenderComponent<Calendar>();
        var currentMonthYear = DateTime.Today.ToString("MMMM yyyy");

        // First move away from current month
        cut.Find("[data-testid='next-month-btn']").Click();

        // Act
        cut.Find("[data-testid='today-btn']").Click();

        // Assert
        cut.Find("[data-testid='month-display']").TextContent.Should().Be(currentMonthYear);
        cut.Find(".today").Should().NotBeNull();
    }

    [Fact]
    public void Calendar_ShouldShowActivityDots_ForDatesWithActivities()
    {
        // Arrange
        var today = DateOnly.FromDateTime(DateTime.Today);
        var measurementDates = new Dictionary<DateOnly, int>
        {
            { today, 1 },
            { today.AddDays(1), 2 },
            { today.AddDays(-1), 3 }
        };

        var cut = RenderComponent<Calendar>(parameters => parameters
            .Add(p => p.DatesWithMeasurements, measurementDates));

        // Act & Assert
        var dotsContainers = cut.FindAll(".activity-dots");
        dotsContainers.Count.Should().Be(3);

        // Verify individual dots within containers
        foreach (var container in dotsContainers)
        {
            var dots = container.QuerySelectorAll(".dot");
            dots.Length.Should().BeGreaterThan(0);
            dots.Length.Should().BeLessOrEqualTo(5); // Max 5 dots per day as per component logic
        }
    }

    [Fact]
    public void Calendar_ShouldInitialize_WithProvidedSelectedDate()
    {
        // Arrange
        var selectedDate = DateOnly.FromDateTime(DateTime.Today);

        // Act
        var cut = RenderComponent<Calendar>(parameters => parameters
            .Add(p => p.SelectedDate, selectedDate));

        // Assert
        var selectedDateElement = cut.Find($"[data-testid='calendar-day-{selectedDate:yyyy-MM-dd}']");
        selectedDateElement.Should().NotBeNull();
        selectedDateElement.ClassList.Should().Contain("today");
    }
    [Fact]
    public void Calendar_ShouldHandle_YearTransition_Forward()
    {
        // Arrange
        var cut = RenderComponent<Calendar>();
        var december = new DateTime(DateTime.Today.Year, 12, 1);
        var expectedJanuary = december.AddMonths(1).ToString("MMMM yyyy");

        // Navigate to December
        while (cut.Find("[data-testid='month-display']").TextContent != december.ToString("MMMM yyyy"))
        {
            cut.Find("[data-testid='next-month-btn']").Click();
        }

        // Act
        cut.Find("[data-testid='next-month-btn']").Click();

        // Assert
        cut.Find("[data-testid='month-display']").TextContent.Should().Be(expectedJanuary);
    }

    [Fact]
    public void Calendar_ShouldHandle_YearTransition_Backward()
    {
        // Arrange
        var cut = RenderComponent<Calendar>();
        var january = new DateTime(DateTime.Today.Year, 1, 1);
        var expectedDecember = january.AddMonths(-1).ToString("MMMM yyyy");

        // Navigate to January
        while (cut.Find("[data-testid='month-display']").TextContent != january.ToString("MMMM yyyy"))
        {
            cut.Find("[data-testid='prev-month-btn']").Click();
        }

        // Act
        cut.Find("[data-testid='prev-month-btn']").Click();

        // Assert
        cut.Find("[data-testid='month-display']").TextContent.Should().Be(expectedDecember);
    }

    [Fact]
    public void Calendar_ShouldHandle_LeapYear()
    {
        // Arrange
        var cut = RenderComponent<Calendar>();
        var nextLeapYear = DateTime.Today.Year + (4 - DateTime.Today.Year % 4);
        var february = new DateTime(nextLeapYear, 2, 1);

        // Navigate to leap year February
        while (cut.Find("[data-testid='month-display']").TextContent != february.ToString("MMMM yyyy"))
        {
            cut.Find("[data-testid='next-month-btn']").Click();
        }

        // Act & Assert
        var lastDayOfFebruary = cut.FindAll(".calendar-day")
            .Select(e => e.TextContent.Trim())
            .Where(t => !string.IsNullOrEmpty(t))
            .Select(int.Parse)
            .Max();

        lastDayOfFebruary.Should().Be(29);
    }

    [Fact]
    public void Calendar_ShouldHandle_MonthsWith31Days()
    {
        // Arrange
        var cut = RenderComponent<Calendar>();
        var nextMonth = DateTime.Today.AddMonths(1);
        while (nextMonth.Month is not (1 or 3 or 5 or 7 or 8 or 10 or 12))
        {
            nextMonth = nextMonth.AddMonths(1);
        }

        // Navigate to next month with 31 days
        while (cut.Find("[data-testid='month-display']").TextContent != nextMonth.ToString("MMMM yyyy"))
        {
            cut.Find("[data-testid='next-month-btn']").Click();
        }

        // Act & Assert
        var lastDayOfMonth = cut.FindAll(".calendar-day")
            .Select(e => e.TextContent.Trim())
            .Where(t => !string.IsNullOrEmpty(t))
            .Select(int.Parse)
            .Max();

        lastDayOfMonth.Should().Be(31);
    }

    [Fact]
    public void Calendar_ShouldHandle_DifferentWeekStarts()
    {
        // Arrange & Act
        var cut = RenderComponent<Calendar>();

        // Assert
        var firstDayOfWeek = cut.FindAll(".row:first-child .col")
            .First()
            .TextContent;
        firstDayOfWeek.Should().Be("Mon");
    }

    [Fact]
    public void Calendar_ShouldHandle_SelectingLastDayOfMonth()
    {
        // Arrange
        var lastDayOfMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month,
            DateTime.DaysInMonth(DateTime.Today.Year, DateTime.Today.Month));
        DateOnly? selectedDate = null;

        var cut = RenderComponent<Calendar>(parameters => parameters
            .Add(p => p.SelectedDateChanged, (DateOnly? date) => selectedDate = date));

        // Act
        var lastDay = cut.Find($"[data-testid='calendar-day-{DateOnly.FromDateTime(lastDayOfMonth):yyyy-MM-dd}']");
        lastDay.Click();

        // Assert
        selectedDate.Should().Be(DateOnly.FromDateTime(lastDayOfMonth));
    }

    [Fact]
    public void Calendar_ShouldHandle_RapidMonthChanges()
    {
        // Arrange
        var cut = RenderComponent<Calendar>();
        var initialMonth = DateTime.Today;
        var expectedMonth = initialMonth.AddMonths(3).ToString("MMMM yyyy");

        // Act - Rapidly click next month three times
        var nextMonthBtn = cut.Find("[data-testid='next-month-btn']");
        for (int i = 0; i < 3; i++)
        {
            nextMonthBtn.Click();
        }

        // Assert
        cut.Find("[data-testid='month-display']").TextContent.Should().Be(expectedMonth);
    }

    [Fact]
    public void Calendar_ShouldHandle_YearRollover()
    {
        // Arrange
        var cut = RenderComponent<Calendar>();
        var initialYear = DateTime.Today.Year;
        var rolloverYear = initialYear + 10;
        var expectedDisplay = DateTime.Today.ToString("MMMM ") + rolloverYear;

        // Act - Click next year multiple times
        var nextYearBtn = cut.Find("[data-testid='next-year-btn']");
        for (int i = 0; i < 10; i++)
        {
            nextYearBtn.Click();
        }

        // Assert
        cut.Find("[data-testid='month-display']").TextContent.Should().Be(expectedDisplay);
    }

    [Fact]
    public void Calendar_ShouldHandle_SelectingSameDateTwice()
    {
        // Arrange
        int selectionCount = 0;
        var cut = RenderComponent<Calendar>(parameters => parameters
            .Add(p => p.SelectedDateChanged, (DateOnly? _) => selectionCount++));

        var today = DateOnly.FromDateTime(DateTime.Today);
        var todayElement = cut.Find($"[data-testid='calendar-day-{today:yyyy-MM-dd}']");

        // Act
        todayElement.Click();
        todayElement.Click();

        // Assert
        selectionCount.Should().Be(2);
    }

    [Fact]
    public void Calendar_ShouldHandle_MultipleMonthNavigations()
    {
        // Arrange
        var cut = RenderComponent<Calendar>();
        var initialMonth = DateTime.Today;

        // Act - Navigate forward 12 months and back 12 months
        for (int i = 0; i < 12; i++)
            cut.Find("[data-testid='next-month-btn']").Click();

        for (int i = 0; i < 12; i++)
            cut.Find("[data-testid='prev-month-btn']").Click();

        // Assert
        cut.Find("[data-testid='month-display']").TextContent
            .Should().Be(initialMonth.ToString("MMMM yyyy"));
    }
}