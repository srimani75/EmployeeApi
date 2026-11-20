using EmployeeApi.Common;

namespace EmployeeApi.Services;

/// <summary>The business failures the employee operations can report.</summary>
public static class EmployeeErrors
{
    public static Error NotFound(int id) => new(
        ErrorType.NotFound,
        "Employee not found",
        $"Employee '{id}' was not found.");

    public static Error DuplicateEmail(string email) => new(
        ErrorType.Conflict,
        "Employee already exists",
        $"An employee with email '{email}' already exists.");
}
