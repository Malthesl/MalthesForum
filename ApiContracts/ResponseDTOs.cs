namespace ApiContracts;

public class QueryResponseDTO<T>
{
    public bool Success => true;
    public required int TotalResults { get; init; }
    public required int StartIndex { get; init; }
    public required int EndIndex { get; init; }
    public required T[] Results { get; init; }
}

public class ResponseDTO(Object result)
{
    public bool Success => true;
    public Object Result => result;
}

public class ErrorResponseDTO
{
    public bool Success => false;
    public required string Error { get; init; }
}