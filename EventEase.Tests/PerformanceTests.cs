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
        var stopwatch = Stopwatch.StartNew();

        // Act
        var result = EventRepository.GetAll();

        stopwatch.Stop();

        // Assert
        Assert.NotEmpty(result);
        Assert.True(stopwatch.ElapsedMilliseconds < PerformanceThresholdMs,
            $"GetAll took {stopwatch.ElapsedMilliseconds}ms, expected < {PerformanceThresholdMs}ms");
    }

    [Fact]
    public void GetById_WithCurrentDataset_CompletesWithinThreshold()
    {
        // Arrange
        var stopwatch = Stopwatch.StartNew();

        // Act
        var result = EventRepository.GetById(1);

        stopwatch.Stop();

        // Assert
        Assert.NotNull(result);
        Assert.True(stopwatch.ElapsedMilliseconds < PerformanceThresholdMs,
            $"GetById took {stopwatch.ElapsedMilliseconds}ms, expected < {PerformanceThresholdMs}ms");
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
        var largeDataset = GenerateLargeEventDataset(LargeDatasetSize);
        var stopwatch = Stopwatch.StartNew();

        // Act: Test LINQ operations that would be used in filtering
        var results = largeDataset
            .Where(e => e.Date >= DateTime.Today)
            .OrderBy(e => e.Date)
            .Take(100)
            .ToList();

        stopwatch.Stop();

        // Assert
        Assert.Equal(100, results.Count);
        Assert.True(stopwatch.ElapsedMilliseconds < 500,
            $"Large dataset query took {stopwatch.ElapsedMilliseconds}ms for {LargeDatasetSize} events, expected < 500ms");
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
        var stopwatch = Stopwatch.StartNew();

        // Act: Simulate loading medium dataset
        var mediumDataset = GenerateLargeEventDataset(MediumDatasetSize);

        stopwatch.Stop();

        // Assert
        Assert.True(stopwatch.ElapsedMilliseconds < 50,
            $"Medium dataset generation took {stopwatch.ElapsedMilliseconds}ms for {MediumDatasetSize} events");
    }

    [Fact]
    public void PerformanceBenchmark_CurrentVsScaledDataset()
    {
        // Arrange
        var currentEvents = EventRepository.GetAll();
        var largeEvents = GenerateLargeEventDataset(10000);

        var stopwatch = new Stopwatch();

        // Act - Current dataset
        stopwatch.Start();
        var currentResult = currentEvents.Where(e => e.Date >= DateTime.Today).ToList();
        stopwatch.Stop();
        var currentTime = stopwatch.ElapsedMilliseconds;

        // Act - Large dataset
        stopwatch.Restart();
        var largeResult = largeEvents.Where(e => e.Date >= DateTime.Today).ToList();
        stopwatch.Stop();
        var largeTime = stopwatch.ElapsedMilliseconds;

        // Assert: Performance should scale linearly or better
        var scaleFactor = largeEvents.Count() / (double)currentEvents.Count();
        var expectedMaxTime = currentTime * scaleFactor * 2; // Allow 2x linear scaling overhead

        Assert.True(largeTime < expectedMaxTime,
            $"Large dataset query ({largeTime}ms) exceeded expected threshold ({expectedMaxTime}ms) for {scaleFactor}x scale");
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
