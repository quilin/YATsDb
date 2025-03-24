namespace YATsDB.Server.Endpoints.Common;

public class QueryResult
{
    public List<object?[]> Result { get; }

    public QueryResult(List<object?[]> result)
    {
        Result = result;
    }
}