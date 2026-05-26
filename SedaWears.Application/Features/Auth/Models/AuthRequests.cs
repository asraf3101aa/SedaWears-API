using SedaWears.Domain.Enums;

namespace SedaWears.Application.Features.Auth.Models;

public record LoginRequest(string? Email, string? Password, bool RememberMe = false);
public record RegisterRequest(string? Email, string? Password, string? FirstName, string? LastName, string? Phone);
public record ForgotPasswordRequest(string? Email);
public record ResetPasswordRequest(string? Email, string? Token, string? NewPassword);
public record AcceptShopMemberInvitationRequest(int? ShopId, string? Email, string? Token, string? FirstName, string? LastName, string? Password);
public record AcceptAdminInvitationRequest(string? Email, string? Token, string? FirstName, string? LastName, string? Password);
public record RefreshRequest(string? RefreshToken);
