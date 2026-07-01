using SedaWears.Domain.Enums;

namespace SedaWears.Application.Common;

public static class CacheKeys
{
    public static string Profile(int userId) => $"profile:{userId}";
    public static string ProfileAddresses(int userId) => $"profile:addresses:{userId}";
    
    public static string Product(int productId) => $"product:{productId}";
}
