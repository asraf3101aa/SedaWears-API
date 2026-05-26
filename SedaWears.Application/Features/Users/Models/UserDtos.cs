
namespace SedaWears.Application.Features.Users.Models;

public record AddressDto(
    int Id,
    string Label,
    string FullName,
    string Email,
    string Phone,
    string Street,
    string City,
    string ZipCode
);

public record ShopSummary(
    int Id,
    string Name,
    string? LogoFileName
);

public record UserDto(
    int Id,
    string FirstName,
    string LastName,
    string? Email,
    string? Phone,
    string? AvatarFileName,
    bool IsEmailConfirmed,
    DateTime CreatedAt
);

public record InvitedUserDto(
    int Id,
    string Email,
    DateTime CreatedAt
);
