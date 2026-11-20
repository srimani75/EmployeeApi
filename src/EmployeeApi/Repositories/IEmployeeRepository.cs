using System.Diagnostics.CodeAnalysis;
using EmployeeApi.Models;

namespace EmployeeApi.Repositories;

public interface IEmployeeRepository
{
    IReadOnlyCollection<Employee> GetAll();

    Employee? GetById(int id);

    /// <summary>
    /// Stores the employee under the next sequential id (1, 2, 3, ...) unless
    /// another employee already uses the same email (compared case-insensitively).
    /// Any id on <paramref name="candidate"/> is ignored.
    /// </summary>
    /// <param name="candidate">The employee to store.</param>
    /// <param name="added">The stored employee, including its assigned id.</param>
    /// <returns><c>true</c> if stored; <c>false</c> if the email is taken.</returns>
    bool TryAdd(Employee candidate, [NotNullWhen(true)] out Employee? added);

    /// <returns><c>true</c> if an employee was removed; <c>false</c> if the id is unknown.</returns>
    bool Remove(int id);
}
