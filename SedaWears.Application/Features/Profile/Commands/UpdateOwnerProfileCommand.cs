using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SedaWears.Application.Common.Exceptions;
using SedaWears.Application.Common.Interfaces;

namespace SedaWears.Application.Features.Profile.Commands;

public record UpdateOwnerProfileCommand(string? FirstName, string? LastName, string? Phone) : IRequest;

public class UpdateOwnerProfileCommandValidator : AbstractValidator<UpdateOwnerProfileCommand>
{
    public UpdateOwnerProfileCommandValidator()
    {
        RuleFor(v => v.FirstName).NotEmpty().WithMessage("First name is required.").MaximumLength(50).WithMessage("First name must not exceed 50 characters.");
        RuleFor(v => v.LastName).NotEmpty().WithMessage("Last name is required.").MaximumLength(50).WithMessage("Last name must not exceed 50 characters.");
        RuleFor(v => v.Phone).NotEmpty().WithMessage("Phone number is required.").Matches(@"^\+?[0-9]{7,15}$").WithMessage("A valid phone number is required.");
    }
}

public class UpdateOwnerProfileCommandHandler(IApplicationDbContext dbContext, ICurrentUser currentUser) :
    IRequestHandler<UpdateOwnerProfileCommand>
{
    public async Task Handle(UpdateOwnerProfileCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUser.Id!.Value;

        var rowsAffected = await dbContext.Users
            .Where(u => u.Id == userId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(u => u.FirstName, request.FirstName)
                .SetProperty(u => u.LastName, request.LastName)
                .SetProperty(u => u.PhoneNumber, request.Phone), 
                cancellationToken);

        if (rowsAffected == 0)
        {
            throw new NotFoundException("User not found.");
        }
    }
}
