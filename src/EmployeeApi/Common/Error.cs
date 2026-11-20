namespace EmployeeApi.Common;

public enum ErrorType
{
    NotFound,
    Conflict,
}

/// <summary>An expected business failure, described without any HTTP knowledge.</summary>
public sealed record Error(ErrorType Type, string Title, string Detail);
