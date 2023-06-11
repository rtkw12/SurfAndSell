namespace Common.Caching;

public static class CacheKeyGenerator
{
    public static string Generate<T>(string prefix, string? suffix = null)
    {
        return suffix != null 
            ? prefix + $"_{typeof(T)}_" + suffix
            : prefix + $"_{typeof(T)}";
    }
}