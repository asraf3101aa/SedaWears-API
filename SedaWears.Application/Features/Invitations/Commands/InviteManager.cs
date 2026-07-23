using Microsoft.Extensions.Options;
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

public record InviteManagerCommand(int? ShopId, string? Email) : IRequest
{
    public string? Email { get; init; } = Email?.Trim();
}

public class InviteManagerValidator : AbstractValidator<InviteManagerCommand>
{
    public InviteManagerValidator()
    {
        RuleFor(x => x.ShopId)
            .NotEmpty().WithMessage("A valid shop identifier is required.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email address is required.")
            .EmailAddress().WithMessage("Please enter a valid email address.");
    }
}

public class InviteManagerHandler(
    IApplicationDbContext dbContext,
    IEmailService emailService,
    IUserService userService,
    IOriginContext originContext,
    ICurrentUser currentUser,
    IOptions<HostUrlsConfig> hostUrlsConfigOptions) : IRequestHandler<InviteManagerCommand>
{
    public async Task Handle(InviteManagerCommand request, CancellationToken ct)
    {
        var user = await userService.FindByIdAsync(currentUser.Id, ct) ?? throw new UnauthorizedAccessException();

        if (request.ShopId == 1)
            throw new ForbiddenException("Manager cant be added to admin shop.");


        if (!user.Roles.Contains(originContext.OriginRole))
            throw new ForbiddenException("You are not authorized to invite a shop manager.");

        if (originContext.OriginRole == UserRole.Owner)
        {
            var isOwner = await dbContext.ShopOwners
                .AnyAsync(so => so.ShopId == request.ShopId && so.UserId == currentUser.Id, ct);

            if (!isOwner)
                throw new ForbiddenException("You are not authorized to invite a manager for this shop.");
        }

        var shop = await dbContext.Shops.AsNoTracking().FirstOrDefaultAsync(s => s.Id == request.ShopId, ct)
            ?? throw new ShopNotFoundException();


        var isManager = await dbContext.ShopManagers
            .AnyAsync(sm => sm.ShopId == request.ShopId && sm.User.Email == request.Email, ct);

        if (isManager)
            throw new BadRequestException("Email is already a shop manager.");

        var isInvited = await dbContext.InvitedShopManagers
            .AnyAsync(ism => ism.ShopId == request.ShopId && ism.Email == request.Email, ct);

        if (isInvited)
            throw new BadRequestException("Email is already invited as a shop manager.");

        var token = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");

        var invitation = new InvitedShopManager
        {
            ShopId = request.ShopId!.Value,
            Email = request.Email!,
            Token = token
        };

        dbContext.InvitedShopManagers.Add(invitation);
        await dbContext.SaveChangesAsync(ct);

        var url = $"{hostUrlsConfigOptions.Value.Manager}/accept-invitation?email={invitation.Email}&token={HttpUtility.UrlEncode(token)}&shopId={request.ShopId}";

        await emailService.SendManagerInvitationEmailAsync(invitation.Email, shop.Name, url);
    }
}

public record AcceptShopManagerInvitationCommand(
    int? ShopId,
    string? Email,
    string? Token,
    string? FirstName,
    string? LastName,
    string? Password) : IRequest
{
    public string? Email { get; init; } = Email?.Trim();
    public string? Token { get; init; } = Token?.Trim();
    public string? FirstName { get; init; } = FirstName?.Trim();
    public string? LastName { get; init; } = LastName?.Trim();
}

public class AcceptShopManagerInvitationValidator : AbstractValidator<AcceptShopManagerInvitationCommand>
{
    public AcceptShopManagerInvitationValidator(IApplicationDbContext dbContext)
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

public class AcceptShopManagerInvitationHandler(
    UserManager<User> userManager,
    IApplicationDbContext dbContext,
    IOptions<AuthConfig> authConfigOptions) : IRequestHandler<AcceptShopManagerInvitationCommand>
{
    public async Task Handle(AcceptShopManagerInvitationCommand request, CancellationToken ct)
    {
        var invitedManager = await dbContext.InvitedShopManagers
            .FirstOrDefaultAsync(ism => ism.ShopId == request.ShopId && ism.Email == request.Email && ism.Token == request.Token, ct);

        if (invitedManager == null || invitedManager.CreatedAt.AddHours(authConfigOptions.Value.ManagerInvitationExpiry) < DateTime.UtcNow)
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

        if (!await userManager.IsInRoleAsync(user, nameof(UserRole.Manager)))
            await userManager.AddToRoleAsync(user, nameof(UserRole.Manager));

        var activeManager = new ShopManager
        {
            ShopId = request.ShopId!.Value,
            UserId = user.Id
        };
        dbContext.ShopManagers.Add(activeManager);
        dbContext.InvitedShopManagers.Remove(invitedManager);

        await dbContext.SaveChangesAsync(ct);
    }
}

public record ResendShopManagerInvitationCommand(int ShopId, int InvitationId) : IRequest;

public class ResendShopManagerInvitationValidator : AbstractValidator<ResendShopManagerInvitationCommand>
{
    public ResendShopManagerInvitationValidator()
    {
        RuleFor(x => x.ShopId)
            .NotEmpty().WithMessage("Shop identifier is required.");

        RuleFor(x => x.InvitationId)
            .NotEmpty().WithMessage("Invitation identifier is required.");
    }
}

public class ResendShopManagerInvitationHandler(
    IApplicationDbContext dbContext,
    IEmailService emailService,
    IOptions<HostUrlsConfig> hostUrlsConfigOptions) : IRequestHandler<ResendShopManagerInvitationCommand>
{
    public async Task Handle(ResendShopManagerInvitationCommand request, CancellationToken ct)
    {
        var shop = await dbContext.Shops
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == request.ShopId, ct)
            ?? throw new ShopNotFoundException();

        var invitation = await dbContext.InvitedShopManagers
            .FirstOrDefaultAsync(ism => ism.ShopId == request.ShopId && ism.Id == request.InvitationId, ct)
            ?? throw new InvitationNotFoundException("Shop manager invitation not found.");

        var token = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");
        invitation.Token = token;
        invitation.CreatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(ct);

        var url = $"{hostUrlsConfigOptions.Value.Manager}/accept-invitation?email={invitation.Email}&token={HttpUtility.UrlEncode(token)}&shopId={request.ShopId}";

        await emailService.SendManagerInvitationEmailAsync(invitation.Email, shop.Name, url);
    }
}


