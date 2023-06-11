namespace Common.MongoDb;

public interface IPages
{
    int CurrentPage { get; }
    int PageSize { get; }
    long DocumentCount { get; }
    int NumberOfPages { get; }
}