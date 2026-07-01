using Microsoft.Extensions.Options;
using MediatR;
using Microsoft.EntityFrameworkCore;
using FluentValidation;
using SedaWears.Application.Common.Interfaces;
using SedaWears.Application.Common.Settings;
using SedaWears.Application.Features.Invitations.Models;

namespace SedaWears.Application.Features.Invitations.Queries;

public record GetAdminInvitationDetailsQuery(string? Email, string? Token) : IRequest<InvitationDetailsResponse>;

public class GetAdminInvitationDetailsValidator : AbstractValidator<GetAdminInvitationDetailsQuery>
{
    public GetAdminInvitationDetailsValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email address is required.")
            .EmailAddress().WithMessage("Please enter a valid email address.");

        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Token is required.");
    }
}

public class GetAdminInvitationDetailsHandler(
    IApplicationDbContext dbContext,
    IOptions<AuthConfig> authConfigOptions) : IRequestHandler<GetAdminInvitationDetailsQuery, InvitationDetailsResponse>
{
    public async Task<InvitationDetailsResponse> Handle(GetAdminInvitationDetailsQuery request, CancellationToken ct)
    {
        var invitation = await dbContext.InvitedAdmins
            .FirstOrDefaultAsync(ia => ia.Email == request.Email && ia.Token == request.Token, ct);

        if (invitation == null || invitation.CreatedAt.AddHours(authConfigOptions.Value.AdminInvitationExpiry) < DateTime.UtcNow)
        {
            return new InvitationDetailsResponse(IsValid: false, UserExists: false);
        }

        var userExists = await dbContext.Users.AnyAsync(u => u.Email == request.Email, ct);
        return new InvitationDetailsResponse(IsValid: true, UserExists: userExists);
    }
}
