namespace SedaWears.Application.Features.Shops.Models;

public record UpsertShopRequest(string Name, string SubdomainSlug, string? Description, string? LogoFileName, string? BannerFileName);
public record UpdateShopActiveStatusRequest(bool IsActive);
public record ShopMemberInviteRequest(string Email);
public record UpdateShopMemberRequest(string FirstName, string LastName);
