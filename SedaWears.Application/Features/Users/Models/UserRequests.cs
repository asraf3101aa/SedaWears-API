namespace SedaWears.Application.Features.Users.Models;

public record UpdateUserRequest(string? FirstName, string? LastName);
public record ChangeUserPasswordRequest(string? OldPassword, string? NewPassword);
public record InviteAdminRequest(string? Email);
