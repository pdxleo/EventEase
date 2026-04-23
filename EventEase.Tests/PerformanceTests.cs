using EventEase.Data;
using EventEase.Models;
using System.Diagnostics;
using Xunit;

namespace EventEase.Tests;

public class PerformanceTests
{
    private const int LargeDatasetSize = 10000;
    private const int MediumDatasetSize = 1000;
    private const int PerformanceThresholdMs = 100; // 100ms threshold

    [Fact]
    public void GetAll_WithCurrentDataset_CompletesWithinThreshold()
    {
        // Arrange
        const int iterations = 2000;
        _ = EventRepository.GetAll();
        var stopwatch = Stopwatch.StartNew();

        // Act
        IReadOnlyList<EventItem>? result = null;
        for (int i = 0; i < iterations; i++)
        {
            result = EventRepository.GetAll();
        }

        stopwatch.Stop();
        var averageMs = stopwatch.Elapsed.TotalMilliseconds / iterations;

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.True(averageMs < 1,
            $"GetAll average took {averageMs:F4}ms, expected < 1ms");
    }

    [Fact]
    public void GetById_WithCurrentDataset_CompletesWithinThreshold()
    {
        // Arrange
        const int iterations = 2000;
        _ = EventRepository.GetById(1);
        var stopwatch = Stopwatch.StartNew();

        // Act
        EventItem? result = null;
        for (int i = 0; i < iterations; i++)
        {
            result = EventRepository.GetById(1);
        }

        stopwatch.Stop();
        var averageMs = stopwatch.Elapsed.TotalMilliseconds / iterations;

        // Assert
        Assert.NotNull(result);
        Assert.True(averageMs < 1,
            $"GetById average took {averageMs:F4}ms, expected < 1ms");
    }

    [Fact]
    public void MultipleGetById_Calls_MaintainPerformance()
    {
        // Arrange
        var currentEvents = EventRepository.GetAll();
        var stopwatch = Stopwatch.StartNew();

        // Act
        for (int i = 0; i < 1000; i++)
        {
            foreach (var eventItem in currentEvents)
            {
                _ = EventRepository.GetById(eventItem.Id);
            }
        }

        stopwatch.Stop();

        // Assert
        double avgTimePerCall = stopwatch.Elapsed.TotalMilliseconds / 3000; // 3 events * 1000 iterations
        Assert.True(avgTimePerCall < 1, // Less than 1ms per call on average
            $"Average time per GetById call: {avgTimePerCall}ms, expected < 1ms");
    }

    [Fact]
    public void GetAll_IsMemoryEfficient()
    {
        // Arrange
        var initialMemory = GC.GetTotalMemory(true);

        // Act
        var events = EventRepository.GetAll();
        var afterLoadMemory = GC.GetTotalMemory(false);

        // Assert
        var memoryUsed = afterLoadMemory - initialMemory;
        Assert.True(memoryUsed < 1_000_000, // Less than 1MB
            $"GetAll used {memoryUsed} bytes, expected < 1,000,000 bytes");
    }

    [Fact]
    public void ScalabilityTest_SimulateLargeDataset()
    {
        // Arrange: Create a simulated large dataset
        var largeDataset = GenerateLargeEventDataset(LargeDatasetSize).ToList();
        const int iterations = 20;

        _ = largeDataset
            .Where(e => e.Date >= DateTime.Today)
            .OrderBy(e => e.Date)
            .Take(100)
            .ToList();

        var stopwatch = Stopwatch.StartNew();

        // Act: Test LINQ operations that would be used in filtering
        List<EventItem>? results = null;
        for (int i = 0; i < iterations; i++)
        {
            results = largeDataset
                .Where(e => e.Date >= DateTime.Today)
                .OrderBy(e => e.Date)
                .Take(100)
                .ToList();
        }

        stopwatch.Stop();
        var averageMs = stopwatch.Elapsed.TotalMilliseconds / iterations;

        // Assert
        Assert.NotNull(results);
        Assert.Equal(100, results.Count);
        Assert.True(averageMs < 500,
            $"Large dataset query average took {averageMs:F4}ms for {LargeDatasetSize} events, expected < 500ms");
    }

    [Fact]
    public void ScalabilityTest_RandomLookupInLargeDataset()
    {
        // Arrange: Create a simulated large dataset
        var largeDataset = GenerateLargeEventDataset(LargeDatasetSize).ToList();
        var random = new Random(42);
        var stopwatch = Stopwatch.StartNew();

        // Act: Perform random lookups
        for (int i = 0; i < 1000; i++)
        {
            int randomId = random.Next(1, LargeDatasetSize + 1);
            _ = largeDataset.FirstOrDefault(e => e.Id == randomId);
        }

        stopwatch.Stop();

        // Assert
        double avgTimePerLookup = stopwatch.Elapsed.TotalMilliseconds / 1000;
        Assert.True(avgTimePerLookup < 5,
            $"Average lookup time: {avgTimePerLookup}ms, expected < 5ms");
    }

    [Fact]
    public void ScalabilityTest_MediumDatasetLoadPerformance()
    {
        // Arrange
        const int iterations = 30;
        _ = GenerateLargeEventDataset(MediumDatasetSize).ToList();
        var stopwatch = Stopwatch.StartNew();

        // Act: Simulate loading medium dataset
        List<EventItem>? mediumDataset = null;
        for (int i = 0; i < iterations; i++)
        {
            mediumDataset = GenerateLargeEventDataset(MediumDatasetSize).ToList();
        }

        stopwatch.Stop();
        var averageMs = stopwatch.Elapsed.TotalMilliseconds / iterations;

        // Assert
        Assert.NotNull(mediumDataset);
        Assert.Equal(MediumDatasetSize, mediumDataset.Count);
        Assert.True(averageMs < 50,
            $"Medium dataset generation average took {averageMs:F4}ms for {MediumDatasetSize} events");
    }

    [Fact]
    public void PerformanceBenchmark_CurrentVsScaledDataset()
    {
        // Arrange
        var currentEvents = EventRepository.GetAll();
        var largeEvents = GenerateLargeEventDataset(10000).ToList();

        // Warm up JIT and LINQ paths before timing.
        _ = currentEvents.Where(e => e.Date >= DateTime.Today).ToList();
        _ = largeEvents.Where(e => e.Date >= DateTime.Today).ToList();

        const int currentIterations = 300;
        const int largeIterations = 30;

        var stopwatch = new Stopwatch();

        // Act - Current dataset (average over many iterations to avoid 0ms timing artifacts)
        stopwatch.Start();
        for (int i = 0; i < currentIterations; i++)
        {
            _ = currentEvents.Where(e => e.Date >= DateTime.Today).ToList();
        }
        stopwatch.Stop();
        var currentAverageMs = stopwatch.Elapsed.TotalMilliseconds / currentIterations;

        // Act - Large dataset
        stopwatch.Restart();
        for (int i = 0; i < largeIterations; i++)
        {
            _ = largeEvents.Where(e => e.Date >= DateTime.Today).ToList();
        }
        stopwatch.Stop();
        var largeAverageMs = stopwatch.Elapsed.TotalMilliseconds / largeIterations;

        // Assert: Performance should scale linearly or better
        var scaleFactor = largeEvents.Count / (double)currentEvents.Count;
        var expectedMaxTimeMs = Math.Max(1.0, currentAverageMs * scaleFactor * 3.0); // Allow extra overhead and enforce non-zero floor

        Assert.True(largeAverageMs < expectedMaxTimeMs,
            $"Large dataset avg query ({largeAverageMs:F4}ms) exceeded expected threshold ({expectedMaxTimeMs:F4}ms) for {scaleFactor:F2}x scale");
    }

    /// <summary>
    /// Helper method to generate a large event dataset for performance testing
    /// </summary>
    private static IEnumerable<EventItem> GenerateLargeEventDataset(int count)
    {
        var events = new List<EventItem>();
        var baseDate = DateTime.Today.AddDays(1);

        for (int i = 1; i <= count; i++)
        {
            events.Add(new EventItem
            {
                Id = i,
                Name = $"Event {i}",
                Date = baseDate.AddDays(i % 365),
                Location = $"Location {i % 100}",
                Description = $"Description for event {i}"
            });
        }

        return events;
    }
}
