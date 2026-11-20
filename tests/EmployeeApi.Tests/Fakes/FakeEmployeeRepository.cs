using System.Diagnostics.CodeAnalysis;
using EmployeeApi.Models;
using EmployeeApi.Repositories;

namespace EmployeeApi.Tests.Fakes;

/// <summary>Per-test, non-static repository so service tests are fully isolated.</summary>
internal sealed class FakeEmployeeRepository : IEmployeeRepository
{
    private readonly Dictionary<int, Employee> _employees = new();
    private int _lastId;

    public IReadOnlyCollection<Employee> GetAll() => _employees.Values.ToList();

    public Employee? GetById(int id) => _employees.GetValueOrDefault(id);

    public bool TryAdd(Employee candidate, [NotNullWhen(true)] out Employee? added)
    {
        var emailTaken = _employees.Values.Any(existing =>
            string.Equals(existing.Email, candidate.Email, StringComparison.OrdinalIgnoreCase));

        if (emailTaken)
        {
            added = null;
            return false;
        }

        added = candidate with { Id = ++_lastId };
        _employees.Add(added.Id, added);
        return true;
    }

    public bool Remove(int id) => _employees.Remove(id);
}
