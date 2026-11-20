namespace EmployeeApi.Models;

/// <summary>
/// Domain entity. Immutable; the repository assigns <see cref="Id"/> when the
/// employee is stored.
/// </summary>
public sealed record Employee
{
    public int Id { get; init; }

    public required string FirstName { get; init; }

    public required string LastName { get; init; }

    public required string Email { get; init; }

    public required string Department { get; init; }

    public required DateTime CreatedAtUtc { get; init; }
}
