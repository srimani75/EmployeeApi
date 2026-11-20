namespace EmployeeApi.Contracts;

/// <summary>Employee as returned by the API.</summary>
public sealed record EmployeeResponse(
    int Id,
    string FirstName,
    string LastName,
    string Email,
    string Department,
    DateTime CreatedAtUtc);
