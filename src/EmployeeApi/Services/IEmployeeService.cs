using EmployeeApi.Common;
using EmployeeApi.Contracts;

namespace EmployeeApi.Services;

public interface IEmployeeService
{
    IReadOnlyCollection<EmployeeResponse> GetAll();

    /// <returns>The employee, or a NotFound error for an unknown id.</returns>
    Result<EmployeeResponse> GetById(int id);

    /// <returns>The created employee, or a Conflict error if the email is already in use.</returns>
    Result<EmployeeResponse> Create(CreateEmployeeRequest request);

    /// <returns>Success, or a NotFound error for an unknown id.</returns>
    Result Delete(int id);
}
