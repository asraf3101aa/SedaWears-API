using MediatR;
using Microsoft.EntityFrameworkCore;
using SedaWears.Application.Common.Interfaces;
using SedaWears.Application.Common.Exceptions;

namespace SedaWears.Application.Features.Newsletter.Commands;

public record UnsubscribeCommand(string Email) : IRequest
{
    public string Email { get; init; } = Email.Trim();
}

public class UnsubscribeCommandHandler(IApplicationDbContext context) : IRequestHandler<UnsubscribeCommand>
{
    public async Task Handle(UnsubscribeCommand request, CancellationToken cancellationToken)
    {
        var existing = await context.NewsletterSubscribers
            .FirstOrDefaultAsync(n => n.Email == request.Email, cancellationToken)
            ?? throw new NotFoundException("Subscriber with this email was not found.");

        existing.IsSubscribed = false;
        await context.SaveChangesAsync(cancellationToken);
    }
}
