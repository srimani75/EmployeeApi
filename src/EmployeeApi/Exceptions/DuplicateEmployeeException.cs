namespace EmployeeApi.Exceptions;

public sealed class DuplicateEmployeeException(string email)
    : Exception($"An employee with email '{email}' already exists.")
{
    public string Email { get; } = email;
}
