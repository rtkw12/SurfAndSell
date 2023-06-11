using MongoDB.Driver;

namespace Common.MongoDb;

public interface IPaginatedView<T>
{
    IAsyncCursor<T> Cursor { get; }
    IPages Pagination { get; }
}