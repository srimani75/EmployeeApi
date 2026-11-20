using System.Diagnostics.CodeAnalysis;
using EmployeeApi.Models;

namespace EmployeeApi.Repositories;

/// <summary>
/// Keeps employees in a static collection for the lifetime of the process.
/// Every access goes through one lock so that the email-uniqueness check, the
/// id assignment and the insert in <see cref="TryAdd"/> are a single atomic step.
/// Ids are sequential and never reused, even after a delete.
/// </summary>
public sealed class InMemoryEmployeeRepository : IEmployeeRepository
{
    private static readonly Dictionary<int, Employee> Employees = new();
    private static readonly object SyncRoot = new();
    private static int _lastId;

    public IReadOnlyCollection<Employee> GetAll()
    {
        lock (SyncRoot)
        {
            return Employees.Values
                .OrderBy(employee => employee.Id)
                .ToList();
        }
    }

    public Employee? GetById(int id)
    {
        lock (SyncRoot)
        {
            return Employees.GetValueOrDefault(id);
        }
    }

    public bool TryAdd(Employee candidate, [NotNullWhen(true)] out Employee? added)
    {
        ArgumentNullException.ThrowIfNull(candidate);

        lock (SyncRoot)
        {
            var emailTaken = Employees.Values.Any(existing =>
                string.Equals(existing.Email, candidate.Email, StringComparison.OrdinalIgnoreCase));

            if (emailTaken)
            {
                added = null;
                return false;
            }

            added = candidate with { Id = ++_lastId };
            Employees.Add(added.Id, added);
            return true;
        }
    }

    public bool Remove(int id)
    {
        lock (SyncRoot)
        {
            return Employees.Remove(id);
        }
    }
}
