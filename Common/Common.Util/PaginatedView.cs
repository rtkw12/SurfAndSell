using Common.MongoDb;

namespace Common.Util;

public class PaginatedViewResult<T>
{
    public PaginatedViewResult(IEnumerable<T> values, IPages pages)
    {
        if (pages == null) throw new ArgumentNullException(nameof(pages));

        Collection = values ?? throw new ArgumentNullException(nameof(values));
        Pagination = new Pagination(pages);
    }

    public IEnumerable<T> Collection { get; }
    public Pagination Pagination { get; }
}

public class Pagination : IPages
{
    public Pagination(IPages pages)
    {
        if (pages == null) throw new ArgumentNullException(nameof(pages));

        CurrentPage = pages.CurrentPage;
        PageSize = pages.PageSize;
        DocumentCount = pages.DocumentCount;
        NumberOfPages = pages.NumberOfPages;
    }

    public int CurrentPage { get; }
    public int PageSize { get; }
    public long DocumentCount { get; }
    public int NumberOfPages { get; }
}