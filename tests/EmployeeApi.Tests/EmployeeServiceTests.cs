using EmployeeApi.Common;
using EmployeeApi.Services;
using EmployeeApi.Tests.Fakes;
using Microsoft.Extensions.Logging.Abstractions;

namespace EmployeeApi.Tests;

public class EmployeeServiceTests
{
    private readonly FakeEmployeeRepository _repository = new();
    private readonly EmployeeService _service;

    public EmployeeServiceTests()
    {
        _service = new EmployeeService(_repository, NullLogger<EmployeeService>.Instance);
    }

    [Fact]
    public void GetAll_WhenEmpty_ReturnsEmptyCollection()
    {
        Assert.Empty(_service.GetAll());
    }

    [Fact]
    public void Create_StoresEmployee_AndReturnsItWithGeneratedId()
    {
        var request = TestData.CreateRequest();

        var result = _service.Create(request);

        Assert.True(result.IsSuccess);
        var created = result.Value!;
        Assert.True(created.Id > 0);
        Assert.Equal(request.FirstName, created.FirstName);
        Assert.Equal(request.LastName, created.LastName);
        Assert.Equal(request.Email, created.Email);
        Assert.Equal(request.Department, created.Department);
        Assert.NotNull(_repository.GetById(created.Id));
    }

    [Fact]
    public void Create_AssignsSequentialIds()
    {
        var first = _service.Create(TestData.CreateRequest()).Value!;
        var second = _service.Create(TestData.CreateRequest()).Value!;

        Assert.Equal(first.Id + 1, second.Id);
    }

    [Fact]
    public void Create_TrimsWhitespace()
    {
        var email = TestData.UniqueEmail();

        var result = _service.Create(TestData.CreateRequest($"  {email}  "));

        Assert.Equal(email, result.Value!.Email);
    }

    [Fact]
    public void Create_WithEmailAlreadyInUse_ReturnsConflict_AndStoresNothing()
    {
        var email = TestData.UniqueEmail();
        _service.Create(TestData.CreateRequest(email));

        var result = _service.Create(TestData.CreateRequest(email.ToUpperInvariant()));

        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.Equal(ErrorType.Conflict, result.Error!.Type);
        Assert.Single(_repository.GetAll());
    }

    [Fact]
    public void Create_WithNullRequest_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _service.Create(null!));
    }

    [Fact]
    public void GetById_ForExistingEmployee_ReturnsIt()
    {
        var created = _service.Create(TestData.CreateRequest()).Value!;

        var result = _service.GetById(created.Id);

        Assert.True(result.IsSuccess);
        Assert.Equal(created, result.Value);
    }

    [Fact]
    public void GetById_ForUnknownId_ReturnsNotFound()
    {
        var result = _service.GetById(TestData.UnknownId);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
        Assert.Contains(TestData.UnknownId.ToString(), result.Error.Detail);
    }

    [Fact]
    public void GetAll_ReturnsEveryCreatedEmployee()
    {
        var first = _service.Create(TestData.CreateRequest()).Value!;
        var second = _service.Create(TestData.CreateRequest()).Value!;

        var all = _service.GetAll();

        Assert.Equal(2, all.Count);
        Assert.Contains(first, all);
        Assert.Contains(second, all);
    }

    [Fact]
    public void Delete_RemovesEmployee()
    {
        var created = _service.Create(TestData.CreateRequest()).Value!;

        var result = _service.Delete(created.Id);

        Assert.True(result.IsSuccess);
        Assert.Empty(_service.GetAll());
    }

    [Fact]
    public void Delete_ForUnknownId_ReturnsNotFound_WithoutThrowing()
    {
        var result = _service.Delete(TestData.UnknownId);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
    }
}
