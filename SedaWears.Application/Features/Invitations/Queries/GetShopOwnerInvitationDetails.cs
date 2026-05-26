using MediatR;
using Microsoft.EntityFrameworkCore;
using FluentValidation;
using SedaWears.Application.Common.Interfaces;
using SedaWears.Application.Common.Settings;
using SedaWears.Application.Features.Invitations.Models;

namespace SedaWears.Application.Features.Invitations.Queries;

public record GetShopOwnerInvitationDetailsQuery(int? ShopId, string? Email, string? Token) : IRequest<InvitationDetailsResponse>;

public class GetShopOwnerInvitationDetailsValidator : AbstractValidator<GetShopOwnerInvitationDetailsQuery>
{
    public GetShopOwnerInvitationDetailsValidator()
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

public class GetShopOwnerInvitationDetailsHandler(
    IApplicationDbContext dbContext,
    AppConfig appConfig) : IRequestHandler<GetShopOwnerInvitationDetailsQuery, InvitationDetailsResponse>
{
    public async Task<InvitationDetailsResponse> Handle(GetShopOwnerInvitationDetailsQuery request, CancellationToken ct)
    {
        var invitation = await dbContext.InvitedShopOwners
            .FirstOrDefaultAsync(iso => iso.ShopId == request.ShopId && iso.Email == request.Email && iso.Token == request.Token, ct);

        if (invitation == null || invitation.CreatedAt.AddHours(appConfig.OwnerInvitationExpiry) < DateTime.UtcNow)
        {
            return new InvitationDetailsResponse(IsValid: false, UserExists: false);
        }

        var userExists = await dbContext.Users.AnyAsync(u => u.Email == request.Email, ct);
        return new InvitationDetailsResponse(IsValid: true, UserExists: userExists);
    }
}
