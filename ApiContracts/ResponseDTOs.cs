namespace ApiContracts;

public class QueryResponseDTO<T>
{
    public required int TotalResults { get; init; }
    public required int StartIndex { get; init; }
    public required int EndIndex { get; init; }
    public required T[] Results { get; init; }
}

public class SuccessDTO(string result)
{
    public string Result => result;
}

public class ErrorDTO
{
    public required string Error { get; init; }
}