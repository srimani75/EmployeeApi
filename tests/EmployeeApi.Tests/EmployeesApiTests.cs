using System.Net;
using System.Net.Http.Json;
using EmployeeApi.Contracts;
using Microsoft.AspNetCore.Mvc.Testing;

namespace EmployeeApi.Tests;

/// <summary>End-to-end tests: real routing, validation, DI and error handling.</summary>
public class EmployeesApiTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    private const string Route = "/api/employees";

    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Post_WithValidBody_Returns201_WithLocationOfNewEmployee()
    {
        var request = TestData.CreateRequest();

        var response = await _client.PostAsJsonAsync(Route, request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<EmployeeResponse>();
        Assert.NotNull(created);
        Assert.Equal(request.Email, created.Email);
        Assert.NotNull(response.Headers.Location);

        var fetched = await _client.GetFromJsonAsync<EmployeeResponse>(response.Headers.Location);
        Assert.Equal(created, fetched);
    }

    [Fact]
    public async Task Post_WithInvalidBody_Returns400()
    {
        var invalid = new { firstName = "", lastName = "Lovelace", email = "not-an-email", department = "Engineering" };

        var response = await _client.PostAsJsonAsync(Route, invalid);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Post_WithEmailAlreadyInUse_Returns409()
    {
        var request = TestData.CreateRequest();
        await _client.PostAsJsonAsync(Route, request);

        var response = await _client.PostAsJsonAsync(Route, request);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task GetAll_IncludesCreatedEmployee()
    {
        var created = await CreateEmployeeAsync();

        var all = await _client.GetFromJsonAsync<List<EmployeeResponse>>(Route);

        Assert.NotNull(all);
        Assert.Contains(created, all);
    }

    [Fact]
    public async Task GetById_ForUnknownId_Returns404()
    {
        var response = await _client.GetAsync($"{Route}/{TestData.UnknownId}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetById_WithNonNumericId_Returns404()
    {
        var response = await _client.GetAsync($"{Route}/not-a-number");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_ForExistingEmployee_Returns204_AndEmployeeIsGone()
    {
        var created = await CreateEmployeeAsync();

        var deleteResponse = await _client.DeleteAsync($"{Route}/{created.Id}");
        var getResponse = await _client.GetAsync($"{Route}/{created.Id}");

        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task Delete_ForUnknownId_Returns404()
    {
        var response = await _client.DeleteAsync($"{Route}/{TestData.UnknownId}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private async Task<EmployeeResponse> CreateEmployeeAsync()
    {
        var response = await _client.PostAsJsonAsync(Route, TestData.CreateRequest());
        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadFromJsonAsync<EmployeeResponse>())!;
    }
}
