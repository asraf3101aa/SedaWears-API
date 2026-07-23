using SedaWears.Application.Features.Users.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SedaWears.Application.Common.Exceptions;
using SedaWears.Application.Common.Interfaces;
using SedaWears.Domain.Entities;
using SedaWears.Application.Common;
using FluentValidation;
using ZiggyCreatures.Caching.Fusion;

namespace SedaWears.Application.Features.Profile.Commands;

public record AddAddressCommand(
    string? Label,
    string? FullName,
    string? Email,
    string? Phone,
    string? Street,
    string? City,
    string? ZipCode
) : IRequest<AddressDto>
{
    public string? Label { get; init; } = Label?.Trim();
    public string? FullName { get; init; } = FullName?.Trim();
    public string? Email { get; init; } = Email?.Trim();
    public string? Phone { get; init; } = Phone?.Trim();
    public string? Street { get; init; } = Street?.Trim();
    public string? City { get; init; } = City?.Trim();
    public string? ZipCode { get; init; } = ZipCode?.Trim();
}

public class AddAddressCommandValidator : AbstractValidator<AddAddressCommand>
{
    public AddAddressCommandValidator()
    {
        RuleFor(v => v.Label).NotEmpty().WithMessage("Label is required.");
        RuleFor(v => v.FullName).NotEmpty().WithMessage("Full name is required.");
        RuleFor(v => v.Email).NotEmpty().WithMessage("Email address is required.").EmailAddress().WithMessage("Please provide a valid email address.");
        RuleFor(v => v.Phone).NotEmpty().WithMessage("Phone number is required.");
        RuleFor(v => v.Street).NotEmpty().WithMessage("Street address is required.");
        RuleFor(v => v.City).NotEmpty().WithMessage("City is required.");
        RuleFor(v => v.ZipCode).NotEmpty().WithMessage("Zip code is required.");
    }
}

public class AddAddressCommandHandler(IApplicationDbContext dbContext, ICurrentUser currentUser, IFusionCache fusionCache) :
    IRequestHandler<AddAddressCommand, AddressDto>
{
    public async Task<AddressDto> Handle(AddAddressCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUser.Id;
        var user = await dbContext.Users.Include(u => u.Addresses).FirstOrDefaultAsync(u => u.Id == userId, cancellationToken) ?? throw new UnauthorizedAccessException();

        var entity = new Address
        {
            UserId = userId,
            Label = request.Label ?? string.Empty,
            FullName = request.FullName ?? string.Empty,
            Email = request.Email ?? string.Empty,
            Phone = request.Phone ?? string.Empty,
            Street = request.Street ?? string.Empty,
            City = request.City ?? string.Empty,
            ZipCode = request.ZipCode ?? string.Empty
        };

        user.Addresses.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);

        await fusionCache.RemoveAsync(CacheKeys.UserAddresses(userId), token: cancellationToken);

        return new AddressDto(entity.Id, entity.Label, entity.FullName, entity.Email, entity.Phone, entity.Street, entity.City, entity.ZipCode);
    }
}
