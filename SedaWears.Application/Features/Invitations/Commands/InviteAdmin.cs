using Microsoft.Extensions.Options;
using SedaWears.Application.Common.Exceptions;
using SedaWears.Application.Common.Interfaces;
using SedaWears.Application.Common.Settings;
using SedaWears.Domain.Enums;
using SedaWears.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using FluentValidation;
using System.Web;

namespace SedaWears.Application.Features.Invitations.Commands;

public record InviteAdminCommand(string? Email) : IRequest;
public record ResendAdminInvitationCommand(int InvitationId) : IRequest;

public class InviteAdminValidator : AbstractValidator<InviteAdminCommand>
{
    public InviteAdminValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email address is required.")
            .EmailAddress().WithMessage("Please enter a valid email address.");
    }
}

public class AdminInvitationHandlers(
    UserManager<User> userManager,
    IApplicationDbContext dbContext,
    IEmailService emailService,
    IOptions<HostUrlsConfig> hostUrlsConfigOptions) :
    IRequestHandler<InviteAdminCommand>,
    IRequestHandler<ResendAdminInvitationCommand>
{
    public async Task Handle(InviteAdminCommand request, CancellationToken ct)
    {
        var user = await userManager.FindByEmailAsync(request.Email!);

        if (user != null && await userManager.IsInRoleAsync(user, nameof(UserRole.Admin)))
        {
            throw new BadRequestException("Email is already an Admin.");
        }

        var invitation = await dbContext.InvitedAdmins
            .FirstOrDefaultAsync(ia => ia.Email == request.Email, ct);

        var token = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");

        if (invitation == null)
        {
            invitation = new InvitedAdmin
            {
                Email = request.Email!,
                Token = token
            };
            dbContext.InvitedAdmins.Add(invitation);
        }
        else
        {
            invitation.Token = token;
            invitation.CreatedAt = DateTime.UtcNow;
        }

        await dbContext.SaveChangesAsync(ct);

        var url = $"{hostUrlsConfigOptions.Value.Admin}/accept-invitation?email={invitation.Email}&token={HttpUtility.UrlEncode(token)}";

        await emailService.SendAdminInvitationEmailAsync(invitation.Email, url);
    }

    public async Task Handle(ResendAdminInvitationCommand request, CancellationToken ct)
    {
        var invitation = await dbContext.InvitedAdmins
            .FirstOrDefaultAsync(ia => ia.Id == request.InvitationId, ct)
            ?? throw new InvitationNotFoundException("Admin invitation not found.");

        var token = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");
        invitation.Token = token;
        invitation.CreatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(ct);

        var url = $"{hostUrlsConfigOptions.Value.Admin}/accept-invitation?email={invitation.Email}&token={HttpUtility.UrlEncode(token)}";

        await emailService.SendAdminInvitationEmailAsync(invitation.Email, url);
    }
}

public record AcceptAdminInvitationCommand(
    string? Email,
    string? Token,
    string? FirstName,
    string? LastName,
    string? Password) : IRequest;

public class AcceptAdminInvitationValidator : AbstractValidator<AcceptAdminInvitationCommand>
{
    public AcceptAdminInvitationValidator(IApplicationDbContext dbContext)
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email address is required.")
            .EmailAddress().WithMessage("Please enter a valid email address.");

        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Invitation token is required.");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .MaximumLength(50).WithMessage("First name must not exceed 50 characters.")
            .WhenAsync(async (cmd, ct) => !await UserExists(dbContext, cmd.Email!, ct));

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required.")
            .MaximumLength(50).WithMessage("Last name must not exceed 50 characters.")
            .WhenAsync(async (cmd, ct) => !await UserExists(dbContext, cmd.Email!, ct));

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters long.")
            .WhenAsync(async (cmd, ct) => !await UserExists(dbContext, cmd.Email!, ct));
    }

    private static async Task<bool> UserExists(IApplicationDbContext dbContext, string email, CancellationToken ct)
    {
        return await dbContext.Users.AnyAsync(u => u.Email == email, ct);
    }
}

public class AcceptAdminInvitationHandler(
    UserManager<User> userManager,
    IApplicationDbContext dbContext,
    IOptions<AuthConfig> authConfigOptions) : IRequestHandler<AcceptAdminInvitationCommand>
{
    public async Task Handle(AcceptAdminInvitationCommand request, CancellationToken ct)
    {
        var invitedAdmin = await dbContext.InvitedAdmins
            .FirstOrDefaultAsync(ia => ia.Email == request.Email && ia.Token == request.Token, ct);

        if (invitedAdmin == null || invitedAdmin.CreatedAt.AddHours(authConfigOptions.Value.AdminInvitationExpiry) < DateTime.UtcNow)
        {
            throw new BadRequestException("Invalid or expired invitation token or email.");
        }

        var user = await userManager.FindByEmailAsync(request.Email!);
        if (user == null)
        {
            user = new User
            {
                UserName = request.Email!,
                Email = request.Email!,
                FirstName = request.FirstName!,
                LastName = request.LastName!
            };
            var result = await userManager.CreateAsync(user, request.Password!);
            if (!result.Succeeded)
                throw new BadRequestException(result.Errors.First().Description);
        }
        else
        {
            if (string.IsNullOrEmpty(user.FirstName) && !string.IsNullOrEmpty(request.FirstName))
                user.FirstName = request.FirstName!;
            if (string.IsNullOrEmpty(user.LastName) && !string.IsNullOrEmpty(request.LastName))
                user.LastName = request.LastName!;
            await userManager.UpdateAsync(user);
        }

        if (!await userManager.IsInRoleAsync(user, nameof(UserRole.Admin)))
            await userManager.AddToRoleAsync(user, nameof(UserRole.Admin));

        dbContext.InvitedAdmins.Remove(invitedAdmin);
        await dbContext.SaveChangesAsync(ct);
    }
}

