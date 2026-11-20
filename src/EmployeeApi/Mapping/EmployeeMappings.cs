using EmployeeApi.Contracts;
using EmployeeApi.Models;

namespace EmployeeApi.Mapping;

public static class EmployeeMappings
{
    public static Employee ToEmployee(this CreateEmployeeRequest request) => new()
    {
        FirstName = request.FirstName.Trim(),
        LastName = request.LastName.Trim(),
        Email = request.Email.Trim(),
        Department = request.Department.Trim(),
        CreatedAtUtc = DateTime.UtcNow,
    };

    public static EmployeeResponse ToResponse(this Employee employee) => new(
        employee.Id,
        employee.FirstName,
        employee.LastName,
        employee.Email,
        employee.Department,
        employee.CreatedAtUtc);
}
