using MediatR;
using Microsoft.EntityFrameworkCore;
using SedaWears.Application.Common.Interfaces;
using SedaWears.Application.Common.Exceptions;

namespace SedaWears.Application.Features.Newsletter.Commands;

public record ConfirmUnsubscribeCommand(string Token) : IRequest
{
    public string Token { get; init; } = Token.Trim();
}

public class ConfirmUnsubscribeCommandHandler(IApplicationDbContext context) : IRequestHandler<ConfirmUnsubscribeCommand>
{
    public async Task Handle(ConfirmUnsubscribeCommand request, CancellationToken cancellationToken)
    {
        var existing = await context.NewsletterSubscribers
            .FirstOrDefaultAsync(n => n.UnsubscribeToken == request.Token, cancellationToken)
            ?? throw new NotFoundException("Invalid or expired unsubscribe link.");

        existing.IsSubscribed = false;
        await context.SaveChangesAsync(cancellationToken);
    }
}
