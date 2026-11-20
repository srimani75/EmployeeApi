using EmployeeApi.Repositories;

namespace EmployeeApi.Tests;

public class InMemoryEmployeeRepositoryTests
{
    private readonly InMemoryEmployeeRepository _repository = new();

    [Fact]
    public void TryAdd_AssignsPositiveId_AndStoresEmployee()
    {
        var candidate = TestData.Employee();

        Assert.True(_repository.TryAdd(candidate, out var added));

        Assert.True(added.Id > 0);
        Assert.Equal(candidate.Email, added.Email);
        Assert.Equal(added, _repository.GetById(added.Id));
        Assert.Contains(added, _repository.GetAll());
    }

    [Fact]
    public void TryAdd_AssignsIncreasingIds()
    {
        _repository.TryAdd(TestData.Employee(), out var first);
        _repository.TryAdd(TestData.Employee(), out var second);

        Assert.True(second!.Id > first!.Id);
    }

    [Fact]
    public void TryAdd_WithSameEmailInDifferentCase_ReturnsFalse()
    {
        var email = TestData.UniqueEmail();
        _repository.TryAdd(TestData.Employee(email), out _);

        var stored = _repository.TryAdd(TestData.Employee(email.ToUpperInvariant()), out var added);

        Assert.False(stored);
        Assert.Null(added);
    }

    [Fact]
    public void Remove_DeletesEmployee_AndReportsUnknownIds()
    {
        _repository.TryAdd(TestData.Employee(), out var added);

        Assert.True(_repository.Remove(added!.Id));
        Assert.Null(_repository.GetById(added.Id));
        Assert.False(_repository.Remove(added.Id));
    }

    [Fact]
    public void Collection_IsStatic_SharedAcrossInstances()
    {
        _repository.TryAdd(TestData.Employee(), out var added);

        var otherInstance = new InMemoryEmployeeRepository();

        Assert.Equal(added, otherInstance.GetById(added!.Id));
    }

    [Fact]
    public void TryAdd_ConcurrentlyWithSameEmail_AddsExactlyOne()
    {
        var email = TestData.UniqueEmail();
        var successes = 0;

        Parallel.For(0, 50, _ =>
        {
            if (_repository.TryAdd(TestData.Employee(email), out var _))
            {
                Interlocked.Increment(ref successes);
            }
        });

        Assert.Equal(1, successes);
    }

    [Fact]
    public void TryAdd_Concurrently_AssignsUniqueIds()
    {
        var ids = new System.Collections.Concurrent.ConcurrentBag<int>();

        Parallel.For(0, 50, _ =>
        {
            if (_repository.TryAdd(TestData.Employee(), out var added))
            {
                ids.Add(added.Id);
            }
        });

        Assert.Equal(50, ids.Distinct().Count());
    }
}
