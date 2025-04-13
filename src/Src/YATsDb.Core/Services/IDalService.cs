
namespace YATsDb.Core.Services;

public interface IDalService
{
    void InsertLines(string bucketName, string lines);

    List<object?[]> Query(string bucketName, string query, QueryParameters parameters);
}
