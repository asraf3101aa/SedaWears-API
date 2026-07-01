using Microsoft.Extensions.Options;
using SedaWears.Application.Common.Interfaces;
using SedaWears.Application.Common.Settings;
using SedaWears.Application.Features.Invitations.Models;
using MediatR;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace SedaWears.Application.Features.Invitations.Queries;

public record GetShopManagerInvitationDetailsQuery(int? ShopId, string? Email, string? Token) : IRequest<InvitationDetailsResponse>;

public class GetShopManagerInvitationDetailsValidator : AbstractValidator<GetShopManagerInvitationDetailsQuery>
{
    public GetShopManagerInvitationDetailsValidator()
    {
        RuleFor(x => x.ShopId)
            .NotEmpty().WithMessage("Shop identifier is required.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email address is required.")
            .EmailAddress().WithMessage("Please enter a valid email address.");

        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Token is required.");
    }
}

public class GetShopManagerInvitationDetailsHandler(
    IApplicationDbContext dbContext,
    IOptions<AuthConfig> authConfigOptions) : IRequestHandler<GetShopManagerInvitationDetailsQuery, InvitationDetailsResponse>
{
    public async Task<InvitationDetailsResponse> Handle(GetShopManagerInvitationDetailsQuery request, CancellationToken ct)
    {
        var invitation = await dbContext.InvitedShopManagers
            .FirstOrDefaultAsync(ism => ism.ShopId == request.ShopId && ism.Email == request.Email && ism.Token == request.Token, ct);

        if (invitation == null || invitation.CreatedAt.AddHours(authConfigOptions.Value.ManagerInvitationExpiry) < DateTime.UtcNow)
        {
            return new InvitationDetailsResponse(IsValid: false, UserExists: false);
        }

        var userExists = await dbContext.Users.AnyAsync(u => u.Email == request.Email, ct);
        return new InvitationDetailsResponse(IsValid: true, UserExists: userExists);
    }
}
