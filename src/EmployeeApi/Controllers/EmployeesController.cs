using EmployeeApi.Common;
using EmployeeApi.Contracts;
using EmployeeApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeApi.Controllers;

/// <summary>
/// HTTP surface only. Invalid bodies are rejected with 400 by [ApiController];
/// business failures returned by the service become ProblemDetails responses here;
/// anything unexpected is caught by GlobalExceptionHandler.
/// </summary>
[ApiController]
[Route("api/employees")]
[Produces("application/json")]
public sealed class EmployeesController(IEmployeeService employeeService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyCollection<EmployeeResponse>), StatusCodes.Status200OK)]
    public IActionResult GetAll() =>
        Ok(employeeService.GetAll());

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(EmployeeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public IActionResult GetById(int id) =>
        employeeService.GetById(id).Match<IActionResult>(
            employee => Ok(employee),
            error => ToProblem(error));

    [HttpPost]
    [ProducesResponseType(typeof(EmployeeResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public IActionResult Create([FromBody] CreateEmployeeRequest request) =>
        employeeService.Create(request).Match<IActionResult>(
            created => CreatedAtAction(nameof(GetById), new { id = created.Id }, created),
            error => ToProblem(error));

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public IActionResult Delete(int id) =>
        employeeService.Delete(id).Match<IActionResult>(
            () => NoContent(),
            error => ToProblem(error));

    private ObjectResult ToProblem(Error error) => Problem(
        title: error.Title,
        detail: error.Detail,
        statusCode: error.Type switch
        {
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status500InternalServerError,
        });
}
