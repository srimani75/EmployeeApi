using EmployeeApi.Contracts;
using EmployeeApi.Models;

namespace EmployeeApi.Tests;

/// <summary>
/// Builders for test input (ids are assigned by the repository). Emails are
/// unique per call because the real repository's collection is static and therefore shared between tests.
/// </summary>
internal static class TestData
{
    /// <summary>An id no test will ever reach.</summary>
    public const int UnknownId = int.MaxValue;

    public static string UniqueEmail() => $"user-{Guid.NewGuid():N}@example.com";

    public static CreateEmployeeRequest CreateRequest(string? email = null) => new()
    {
        FirstName = "Ada",
        LastName = "Lovelace",
        Email = email ?? UniqueEmail(),
        Department = "Engineering",
    };

    public static Employee Employee(string? email = null) => new()
    {
        FirstName = "Grace",
        LastName = "Hopper",
        Email = email ?? UniqueEmail(),
        Department = "Research",
        CreatedAtUtc = DateTime.UtcNow,
    };
}
