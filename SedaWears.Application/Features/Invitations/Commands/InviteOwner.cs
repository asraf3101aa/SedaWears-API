using SedaWears.Application.Common.Exceptions;
using SedaWears.Application.Common.Interfaces;
using SedaWears.Application.Common.Settings;
using SedaWears.Domain.Entities;
using MediatR;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using System.Web;
using Microsoft.AspNetCore.Identity;
using SedaWears.Domain.Enums;
using SedaWears.Application.Features.Invitations.Models;

namespace SedaWears.Application.Features.Invitations.Commands;

public record InviteOwnerCommand(int? ShopId, string? Email) : IRequest;

public class InviteOwnerValidator : AbstractValidator<InviteOwnerCommand>
{
    public InviteOwnerValidator()
    {
        RuleFor(x => x.ShopId)
            .NotEmpty().WithMessage("A valid shop identifier is required.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email address is required.")
            .EmailAddress().WithMessage("Please enter a valid email address.");
    }
}

public class InviteOwnerHandler(
    IApplicationDbContext dbContext,
    IEmailService emailService,
    AppConfig appConfig) : IRequestHandler<InviteOwnerCommand>
{
    public async Task Handle(InviteOwnerCommand request, CancellationToken ct)
    {
        var shop = await dbContext.Shops.AsNoTracking().FirstOrDefaultAsync(s => s.Id == request.ShopId, ct)
            ?? throw new NotFoundException("Shop not found.");

        var isOwner = await dbContext.ShopOwners
            .AnyAsync(so => so.ShopId == request.ShopId && so.User.Email == request.Email, ct);

        if (isOwner)
            throw new BadRequestException("Email is already a shop owner.");

        var isInvited = await dbContext.InvitedShopOwners
            .AnyAsync(iso => iso.ShopId == request.ShopId && iso.Email == request.Email, ct);

        if (isInvited)
            throw new BadRequestException("Email is already invited as a shop owner.");

        var token = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");

        var invitation = new InvitedShopOwner
        {
            ShopId = request.ShopId!.Value,
            Email = request.Email!,
            Token = token
        };

        dbContext.InvitedShopOwners.Add(invitation);
        await dbContext.SaveChangesAsync(ct);

        var url = $"{appConfig.OwnerFrontendUrl}/accept-invitation?email={invitation.Email}&token={HttpUtility.UrlEncode(token)}&shopId={request.ShopId}";

        await emailService.SendEmailAsync(
            invitation.Email,
            $"SedaWears Shop Owner Invitation for {shop.Name}",
            $"<p>You have been invited as a Shop Owner for <b>{shop.Name}</b> on SedaWears.</p><p>Click <a href='{url}'>here</a> to accept the invitation and set your password.</p>");
    }
}

public record AcceptShopOwnerInvitationCommand(
    int? ShopId,
    string? Email,
    string? Token,
    string? FirstName,
    string? LastName,
    string? Password) : IRequest;

public class AcceptShopOwnerInvitationValidator : AbstractValidator<AcceptShopOwnerInvitationCommand>
{
    public AcceptShopOwnerInvitationValidator(IApplicationDbContext dbContext)
    {
        RuleFor(x => x.ShopId)
            .NotEmpty().WithMessage("Shop identifier is required.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email address is required.")
            .EmailAddress().WithMessage("Please enter a valid email address.");

        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Invitation token is required.");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .MaximumLength(50).WithMessage("First name must not exceed 50 characters.")
            .WhenAsync(async (cmd, ct) => !await UserExists(dbContext, cmd.Email, ct));

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required.")
            .MaximumLength(50).WithMessage("Last name must not exceed 50 characters.")
            .WhenAsync(async (cmd, ct) => !await UserExists(dbContext, cmd.Email, ct));

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters long.")
            .WhenAsync(async (cmd, ct) => !await UserExists(dbContext, cmd.Email, ct));
    }

    private static async Task<bool> UserExists(IApplicationDbContext dbContext, string? email, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(email)) return false;
        return await dbContext.Users.AnyAsync(u => u.Email == email, ct);
    }
}

public class AcceptShopOwnerInvitationHandler(
    UserManager<User> userManager,
    IApplicationDbContext dbContext,
    AppConfig appConfig) : IRequestHandler<AcceptShopOwnerInvitationCommand>
{
    public async Task Handle(AcceptShopOwnerInvitationCommand request, CancellationToken ct)
    {
        var invitedOwner = await dbContext.InvitedShopOwners
            .FirstOrDefaultAsync(iso => iso.ShopId == request.ShopId && iso.Email == request.Email && iso.Token == request.Token, ct);

        if (invitedOwner == null || invitedOwner.CreatedAt.AddHours(appConfig.OwnerInvitationExpiry) < DateTime.UtcNow)
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

        if (!await userManager.IsInRoleAsync(user, nameof(UserRole.Owner)))
            await userManager.AddToRoleAsync(user, nameof(UserRole.Owner));

        var activeOwner = new ShopOwner
        {
            ShopId = request.ShopId!.Value,
            UserId = user.Id
        };
        dbContext.ShopOwners.Add(activeOwner);
        dbContext.InvitedShopOwners.Remove(invitedOwner);

        await dbContext.SaveChangesAsync(ct);
    }
}

public record ResendShopOwnerInvitationCommand(int ShopId, int InvitationId) : IRequest;

public class ResendShopOwnerInvitationValidator : AbstractValidator<ResendShopOwnerInvitationCommand>
{
    public ResendShopOwnerInvitationValidator()
    {
        RuleFor(x => x.ShopId)
            .NotEmpty().WithMessage("Shop identifier is required.");

        RuleFor(x => x.InvitationId)
            .NotEmpty().WithMessage("Invitation identifier is required.");
    }
}

public class ResendShopOwnerInvitationHandler(
    IApplicationDbContext dbContext,
    IEmailService emailService,
    AppConfig appConfig) : IRequestHandler<ResendShopOwnerInvitationCommand>
{
    public async Task Handle(ResendShopOwnerInvitationCommand request, CancellationToken ct)
    {
        var shop = await dbContext.Shops
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == request.ShopId, ct)
            ?? throw new NotFoundException("Shop not found.");

        var invitation = await dbContext.InvitedShopOwners
            .FirstOrDefaultAsync(iso => iso.ShopId == request.ShopId && iso.Id == request.InvitationId, ct)
            ?? throw new NotFoundException("Shop owner invitation not found.");

        var token = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");
        invitation.Token = token;
        invitation.CreatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(ct);

        var url = $"{appConfig.OwnerFrontendUrl}/accept-invitation?email={invitation.Email}&token={HttpUtility.UrlEncode(token)}&shopId={request.ShopId}";

        var subject = $"SedaWears Shop Owner Invitation for {shop.Name}";
        var body = $"<p>You have been invited as a Shop Owner for <b>{shop.Name}</b> on SedaWears.</p><p>Click <a href='{url}'>here</a> to accept the invitation and set your password.</p>";

        await emailService.SendEmailAsync(invitation.Email, subject, body);
    }
}

