using EmployeeApi.ErrorHandling;
using EmployeeApi.Repositories;
using EmployeeApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddSingleton<IEmployeeRepository, InMemoryEmployeeRepository>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();

var app = builder.Build();

app.UseExceptionHandler();
app.UseSwagger();
app.UseSwaggerUI();

// Serves the manual test page in wwwroot at "/".
app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers();

app.Run();

/// <summary>Exposed so the test project can host the API in memory.</summary>
public partial class Program
{
}
