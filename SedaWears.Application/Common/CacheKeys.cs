namespace SedaWears.Application.Common;

public static class CacheKeys
{
    public static string UserAddresses(int userId) => $"user:addresses:{userId}";

    public static string User(int userId) => $"user:{userId}";

    public static string Product(int productId) => $"product:{productId}";

    public static string Shop(int shopId) => $"shop:{shopId}";

    public static string Category(int categoryId) => $"category:{categoryId}";
}
