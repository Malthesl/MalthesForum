namespace ApiContracts;

public class QueryResponseDTO<T>
{
    public required int TotalResults { get; set; }
    public required int StartIndex { get; set; }
    public required int EndIndex { get; set; }
    public required T[] Results { get; set; }
}

public class SuccessDTO(string result)
{
    public string Result => result;
}

public class ErrorDTO
{
    public required string Error { get; set; }
}