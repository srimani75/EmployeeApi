namespace EmployeeApi.Exceptions;

public sealed class EmployeeNotFoundException(int employeeId)
    : Exception($"Employee '{employeeId}' was not found.")
{
    public int EmployeeId { get; } = employeeId;
}
