namespace ApiContracts;

public class QueryDTO<T>
{
    public required int TotalResults { get; init; }
    public required int StartIndex { get; init; }
    public required int EndIndex { get; init; }
    public required T[] Results { get; init; }
}