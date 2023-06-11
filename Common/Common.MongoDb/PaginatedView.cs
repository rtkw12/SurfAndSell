using MongoDB.Driver;

namespace Common.MongoDb;

public class PaginatedView<T> : IPaginatedView<T>
{
    public PaginatedView(IAsyncCursor<T> cursor, IPages pagination)
    {
        Cursor = cursor ?? throw new ArgumentNullException(nameof(cursor));
        Pagination = pagination ?? throw new ArgumentNullException(nameof(pagination));
    }

    public IAsyncCursor<T> Cursor { get; }
    public IPages Pagination { get; }
}