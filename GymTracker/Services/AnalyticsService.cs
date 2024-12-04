using BlazorBootstrap;
using GymTracker.Data;
using Microsoft.EntityFrameworkCore;

namespace GymTracker.Services;

public class AnalyticsService : IAnalyticsService
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

    public AnalyticsService(IDbContextFactory<ApplicationDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<List<ExerciseAnalyticsSummary>> GetUserExercisesInRange(string userId, DateOnly startDate, DateOnly endDate)
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        var exercisesWithCount = await context.TrainingSessions
            .Where(s => s.UserId == userId && s.Date >= startDate && s.Date <= endDate)
            .SelectMany(s => s.Activities)
            .GroupBy(a => new { a.ExerciseId, a.Exercise!.Name })
            .Select(g => new ExerciseAnalyticsSummary
            {
                ExerciseId = g.Key.ExerciseId,
                ExerciseName = g.Key.Name,
                TimesPerformed = g.Count()
            })
            .OrderByDescending(x => x.TimesPerformed)
            .ToListAsync();

        return exercisesWithCount;
    }

    public async Task<ChartData> GetExerciseProgressionChart(string userId, int exerciseId, DateOnly startDate, DateOnly endDate)
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        var exerciseData = await context.TrainingSessions
            .Where(s => s.UserId == userId && s.Date >= startDate && s.Date <= endDate)
            .SelectMany(s => s.Activities)
            .Where(a => a.ExerciseId == exerciseId)
            .OrderBy(a => a.TrainingSession.Date)
            .Select(a => new
            {
                Date = a.TrainingSession.Date,
                MaxWeight = a.Sets.Max(s => s.Weight ?? 0),
                AvgWeight = a.Sets.Average(s => s.Weight ?? 0),
                EstimatedOneRm = a.Sets.Max(s => (s.Weight ?? 0) * (1 + (s.Repetitions * 0.0333))) // Baechle formula
            })
            .ToListAsync();

        var labels = exerciseData.Select(d => d.Date.ToString("MM/dd")).ToList();

        var datasets = new List<IChartDataset>
        {
            new LineChartDataset
            {
                Label = "Max Weight (kg)",
                Data = exerciseData.Select(d => (double?)d.MaxWeight).ToList(),
                BorderColor = "#ff6384",
                Fill = false
            },
            new LineChartDataset
            {
                Label = "Average Weight (kg)",
                Data = exerciseData.Select(d => (double?)d.AvgWeight).ToList(),
                BorderColor = "#36a2eb",
                Fill = false
            },
            new LineChartDataset
            {
                Label = "Estimated 1RM (kg)",
                Data = exerciseData.Select(d => (double?)d.EstimatedOneRm).ToList(),
                BorderColor = "#4bc0c0",
                Fill = false
            }
        };

        return new ChartData
        {
            Labels = labels,
            Datasets = datasets
        };
    }

    public async Task<ChartData> GetExerciseVolumeChart(string userId, int exerciseId, DateOnly startDate, DateOnly endDate)
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        var volumeData = await context.TrainingSessions
            .Where(s => s.UserId == userId && s.Date >= startDate && s.Date <= endDate)
            .SelectMany(s => s.Activities)
            .Where(a => a.ExerciseId == exerciseId)
            .OrderBy(a => a.TrainingSession.Date)
            .Select(a => new
            {
                Date = a.TrainingSession.Date,
                Volume = a.Sets.Sum(s => (s.Weight ?? 0) * s.Repetitions)
            })
            .ToListAsync();

        return new ChartData
        {
            Labels = volumeData.Select(d => d.Date.ToString("MM/dd")).ToList(),
            Datasets = new List<IChartDataset>
            {
                new LineChartDataset
                {
                    Label = "Volume Load (kg)",
                    Data = volumeData.Select(d => (double?)d.Volume).ToList(),
                    BorderColor = "#ff6384",
                    Fill = true,
                    BackgroundColor = "rgba(255, 99, 132, 0.2)"
                }
            }
        };
    }

    public async Task<ChartData> GetExerciseSetsRepsDistribution(string userId, int exerciseId, DateOnly startDate, DateOnly endDate)
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        var repsData = await context.TrainingSessions
            .Where(s => s.UserId == userId && s.Date >= startDate && s.Date <= endDate)
            .SelectMany(s => s.Activities)
            .Where(a => a.ExerciseId == exerciseId)
            .SelectMany(a => a.Sets)
            .GroupBy(s => s.Repetitions)
            .OrderBy(g => g.Key)
            .Select(g => new { Reps = g.Key, Count = g.Count() })
            .ToListAsync();

        return new ChartData
        {
            Labels = repsData.Select(d => d.Reps.ToString()).ToList(),
            Datasets = new List<IChartDataset>
            {
                new BarChartDataset
                {
                    Label = "Sets per Rep Range",
                    Data = repsData.Select(d => (double?)d.Count).ToList(),
                    BackgroundColor = new List<string> { "rgba(54, 162, 235, 0.2)" },
                    BorderColor = new List<string> { "#36a2eb" },
                    BorderWidth = new List<double> { 1 }
                }
            }
        };
    }

    public async Task<ChartData> GetBodyMeasurementsChart(string userId, DateOnly startDate, DateOnly endDate)
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        var measurements = await context.BodyMeasurements
            .Where(m => m.UserId == userId && m.Date >= startDate && m.Date <= endDate)
            .OrderBy(m => m.Date)
            .ToListAsync();

        var labels = measurements.Select(m => m.Date.ToString("MM/dd")).ToList();

        var datasets = new List<IChartDataset>();

        if (measurements.Any(m => m.Weight.HasValue))
        {
            datasets.Add(new LineChartDataset
            {
                Label = "Weight (kg)",
                Data = measurements.Select(m => (double?)m.Weight).ToList(),
                BorderColor = "#ff6384",
                Fill = false,
            });
        }

        // Add BMI if both weight and height are present
        if (measurements.Any(m => m.Weight.HasValue && m.Height.HasValue))
        {
            datasets.Add(new LineChartDataset
            {
                Label = "BMI",
                Data = measurements.Select(m => (double?)m.BMI).ToList(),
                BorderColor = "#4bc0c0",
                Fill = false,
            });
        }

        return new ChartData
        {
            Labels = labels,
            Datasets = datasets
        };
    }

    public async Task<ChartData> GetBodyCircumferencesChart(string userId, DateOnly startDate, DateOnly endDate)
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        var measurements = await context.BodyMeasurements
            .Where(m => m.UserId == userId && m.Date >= startDate && m.Date <= endDate)
            .OrderBy(m => m.Date)
            .ToListAsync();

        var labels = measurements.Select(m => m.Date.ToString("MM/dd")).ToList();
        var datasets = new List<IChartDataset>();

        if (measurements.Any(m => m.ChestCircumference.HasValue))
        {
            datasets.Add(new LineChartDataset
            {
                Label = "Chest (cm)",
                Data = measurements.Select(m => (double?)m.ChestCircumference).ToList(),
                BorderColor = "#ff6384",
                Fill = false
            });
        }

        if (measurements.Any(m => m.WaistCircumference.HasValue))
        {
            datasets.Add(new LineChartDataset
            {
                Label = "Waist (cm)",
                Data = measurements.Select(m => (double?)m.WaistCircumference).ToList(),
                BorderColor = "#36a2eb",
                Fill = false
            });
        }

        if (measurements.Any(m => m.ArmCircumference.HasValue))
        {
            datasets.Add(new LineChartDataset
            {
                Label = "Arm (cm)",
                Data = measurements.Select(m => (double?)m.ArmCircumference).ToList(),
                BorderColor = "#4bc0c0",
                Fill = false
            });
        }

        if (measurements.Any(m => m.ThighCircumference.HasValue))
        {
            datasets.Add(new LineChartDataset
            {
                Label = "Thigh (cm)",
                Data = measurements.Select(m => (double?)m.ThighCircumference).ToList(),
                BorderColor = "#ffcd56",
                Fill = false
            });
        }

        return new ChartData
        {
            Labels = labels,
            Datasets = datasets
        };
    }

    public async Task<ChartData> GetBodyCompositionChart(string userId, DateOnly startDate, DateOnly endDate)
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        var measurements = await context.BodyMeasurements
            .Where(m => m.UserId == userId && m.Date >= startDate && m.Date <= endDate)
            .OrderBy(m => m.Date)
            .ToListAsync();

        var labels = measurements.Select(m => m.Date.ToString("MM/dd")).ToList();
        var datasets = new List<IChartDataset>();

        if (measurements.Any(m => m.FatMassPercentage.HasValue))
        {
            datasets.Add(new LineChartDataset
            {
                Label = "Fat Mass (%)",
                Data = measurements.Select(m => (double?)m.FatMassPercentage).ToList(),
                BorderColor = "#ff6384",
                Fill = false
            });
        }

        if (measurements.Any(m => m.MuscleMassPercentage.HasValue))
        {
            datasets.Add(new LineChartDataset
            {
                Label = "Muscle Mass (%)",
                Data = measurements.Select(m => (double?)m.MuscleMassPercentage).ToList(),
                BorderColor = "#36a2eb",
                Fill = false
            });
        }

        return new ChartData
        {
            Labels = labels,
            Datasets = datasets
        };
    }
}

public record ExerciseAnalyticsSummary
{
    public required int ExerciseId { get; init; }
    public required string ExerciseName { get; init; }
    public required int TimesPerformed { get; init; }
}