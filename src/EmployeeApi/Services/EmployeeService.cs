using EmployeeApi.Common;
using EmployeeApi.Contracts;
using EmployeeApi.Mapping;
using EmployeeApi.Repositories;

namespace EmployeeApi.Services;

public sealed class EmployeeService(
    IEmployeeRepository repository,
    ILogger<EmployeeService> logger) : IEmployeeService
{
    public IReadOnlyCollection<EmployeeResponse> GetAll() =>
        repository.GetAll()
            .Select(employee => employee.ToResponse())
            .ToList();

    public Result<EmployeeResponse> GetById(int id)
    {
        var employee = repository.GetById(id);

        if (employee is null)
        {
            return EmployeeErrors.NotFound(id);
        }

        return employee.ToResponse();
    }

    public Result<EmployeeResponse> Create(CreateEmployeeRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var candidate = request.ToEmployee();

        if (!repository.TryAdd(candidate, out var employee))
        {
            logger.LogWarning("Rejected employee: email {Email} is already in use", candidate.Email);
            return EmployeeErrors.DuplicateEmail(candidate.Email);
        }

        logger.LogInformation("Created employee {EmployeeId}", employee.Id);

        return employee.ToResponse();
    }

    public Result Delete(int id)
    {
        if (!repository.Remove(id))
        {
            logger.LogWarning("Cannot delete employee {EmployeeId}: not found", id);
            return EmployeeErrors.NotFound(id);
        }

        logger.LogInformation("Deleted employee {EmployeeId}", id);

        return Result.Success();
    }
}
