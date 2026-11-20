# Employee API

A small ASP.NET Core Web API that exposes employees through **GET**, **POST** and **DELETE**.
Employees are held in a static in-memory collection; there is no database.

It also includes a browser test page, a script that loads sample data, and an xUnit test suite.

## Contents

- [Requirements](#requirements)
- [Getting started](#getting-started)
- [API reference](#api-reference)
- [Test page and sample data](#test-page-and-sample-data)
- [Running the tests](#running-the-tests)
- [Project structure](#project-structure)
- [Design](#design)
- [Limitations](#limitations)

## Requirements

- .NET 8 SDK or later (the projects target `net8.0` and roll forward to a newer runtime if .NET 8 is not installed)
- Optional: Node.js 18 or later, only for running the sample-data script from the command line

## Getting started

From the solution folder:

```
dotnet run --project src/EmployeeApi
```

Or open `EmployeeApi.sln` in Visual Studio and press F5.

| What        | URL                                  |
|-------------|--------------------------------------|
| Test page   | http://localhost:5080/               |
| Swagger UI  | http://localhost:5080/swagger        |
| API         | http://localhost:5080/api/employees  |

## API reference

| Method | Route                 | Success              | Errors                                    |
|--------|-----------------------|----------------------|-------------------------------------------|
| GET    | `/api/employees`      | 200 + list           |                                           |
| GET    | `/api/employees/{id}` | 200 + employee       | 404 unknown id                            |
| POST   | `/api/employees`      | 201 + employee       | 400 invalid body, 409 email already used  |
| DELETE | `/api/employees/{id}` | 204 (no body)        | 404 unknown id                            |

`{id}` is an integer. A non-numeric id does not match the route and returns 404.

### Create an employee

```
curl -X POST http://localhost:5080/api/employees ^
  -H "Content-Type: application/json" ^
  -d "{\"firstName\":\"Ada\",\"lastName\":\"Lovelace\",\"email\":\"ada@example.com\",\"department\":\"Engineering\"}"
```

Request body rules:

| Field        | Rules                                        |
|--------------|----------------------------------------------|
| `firstName`  | required, up to 50 characters                |
| `lastName`   | required, up to 50 characters                |
| `email`      | required, valid email, up to 254 characters, unique (case-insensitive) |
| `department` | required, up to 100 characters               |

Response (`201 Created`, with a `Location` header pointing at the new employee):

```json
{
  "id": 1,
  "firstName": "Ada",
  "lastName": "Lovelace",
  "email": "ada@example.com",
  "department": "Engineering",
  "createdAtUtc": "2026-11-20T19:25:43.5686125Z"
}
```

### List, get and delete

```
curl http://localhost:5080/api/employees
curl http://localhost:5080/api/employees/1
curl -X DELETE http://localhost:5080/api/employees/1
```

### Error responses

Every error is returned as RFC 7807 `application/problem+json`.

Unknown id (`404`):

```json
{
  "title": "Employee not found",
  "status": 404,
  "detail": "Employee '2' was not found."
}
```

Duplicate email (`409`):

```json
{
  "title": "Employee already exists",
  "status": 409,
  "detail": "An employee with email 'ada@example.com' already exists."
}
```

Invalid body (`400`):

```json
{
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "FirstName": ["The FirstName field is required."],
    "Email": ["The Email field is not a valid e-mail address."]
  }
}
```

An unexpected failure returns a generic `500` with no internal details; the exception is written to the log.

## Test page and sample data

With the API running, open http://localhost:5080/. The page lets you:

- add an employee
- get or delete an employee by id
- see the full list, with a Delete button per row
- see every API response, including errors, in a log at the bottom

The form does no validation of its own, so invalid input reaches the API and its `400` response is visible.

To load 10 sample employees, use any of:

- the **Load sample data** button on the test page
- `node src/EmployeeApi/wwwroot/load-data.js` (an optional base URL can follow; the default is `http://localhost:5080`)
- paste `load-data.js` into the browser console on the test page, then run `await loadSampleData()`

Employees whose email already exists are skipped, so the script is safe to run more than once.

## Running the tests

```
dotnet test
```

| Test class                        | What it covers                                                                 |
|-----------------------------------|--------------------------------------------------------------------------------|
| `EmployeeServiceTests`            | Business rules, against a fake repository so each test is isolated             |
| `InMemoryEmployeeRepositoryTests` | The static store: id assignment, duplicate emails, deletes, concurrent inserts |
| `EmployeesApiTests`               | The HTTP endpoints end to end (routing, validation, status codes) through `WebApplicationFactory` |

## Project structure

```
EmployeeApi.sln
src/EmployeeApi
  Program.cs       composition root: dependency injection and the request pipeline
  Controllers/     HTTP only: routing and status codes
  Services/        business rules (IEmployeeService, EmployeeService, EmployeeErrors)
  Repositories/    storage (IEmployeeRepository, static in-memory implementation)
  Models/          domain entity (Employee)
  Contracts/       request and response DTOs
  Mapping/         entity <-> DTO conversion
  Common/          Result and Error types for expected failures
  ErrorHandling/   global safety net: unexpected exceptions -> 500 ProblemDetails
  wwwroot/         test page (index.html, app.js) and load-data.js
tests/EmployeeApi.Tests
  service, repository and end-to-end API tests
```

## Design

**Layers.** A request flows Controller -> Service -> Repository. Each layer depends only on the
interface of the one below it, and the wiring happens once in `Program.cs`.

**Patterns used**

- **Repository**: `IEmployeeRepository` hides how employees are stored. The static collection
  could be replaced by a database without touching the service or controller.
- **Service layer**: rules such as "email must be unique" live in `EmployeeService`, so the
  controller stays thin.
- **Dependency injection**: constructor injection throughout, which is also what makes the
  service testable against a fake repository.
- **DTOs**: `CreateEmployeeRequest` and `EmployeeResponse` keep the API contract separate from
  the domain entity.
- **Result**: expected failures are returned as a `Result` carrying an `Error`, not thrown.

**Error handling.** Three layers, all answering with ProblemDetails:

1. *Input validation*: data annotations on the request DTO; `[ApiController]` returns `400` automatically.
2. *Expected business failures* (unknown id, duplicate email): the service returns a `Result`,
   and the controller maps the error type to `404` or `409`. No exception is thrown for a normal outcome.
3. *Unexpected exceptions*: `GlobalExceptionHandler` logs the exception and returns a generic
   `500`. One failed request does not stop the application or leak internal details.

**Ids.** Sequential integers (1, 2, 3, ...) assigned by the repository. An id is never reused
after a delete, and a rejected POST does not consume one.

**Thread safety.** The static store is guarded by a single lock, so the email-uniqueness check,
the id assignment and the insert are one atomic step. Two simultaneous POSTs with the same email
cannot both succeed.

## Limitations

- Data lives in memory only: restarting the application clears all employees and restarts ids at 1.
- There is no update (PUT/PATCH) endpoint, authentication, or paging; none were in scope.
