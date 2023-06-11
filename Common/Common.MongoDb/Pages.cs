namespace Common.MongoDb;

public class Pages : IPages
{
    public Pages(int currentPage, int pageSize, long documentCount, int numberOfPages)
    {
        CurrentPage = currentPage;
        PageSize = pageSize;
        DocumentCount = documentCount;
        NumberOfPages = numberOfPages;
    }


    public int CurrentPage { get; }
    public int PageSize { get; }
    public long DocumentCount { get; }
    public int NumberOfPages { get; }
}