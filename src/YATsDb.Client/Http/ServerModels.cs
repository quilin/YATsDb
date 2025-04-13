namespace YATsDb.Client.Http;

// TODO: All those types should be shared between client and server.
internal sealed record CreateBucketDto(string BucketName, string? Description);

internal sealed record QueryResult(List<object?[]> Result);
