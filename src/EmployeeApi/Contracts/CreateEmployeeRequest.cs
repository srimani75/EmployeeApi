using System.ComponentModel.DataAnnotations;

namespace EmployeeApi.Contracts;

/// <summary>Body of POST /api/employees.</summary>
public sealed class CreateEmployeeRequest
{
    [Required]
    [StringLength(50)]
    public string FirstName { get; init; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string LastName { get; init; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(254)]
    public string Email { get; init; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Department { get; init; } = string.Empty;
}
