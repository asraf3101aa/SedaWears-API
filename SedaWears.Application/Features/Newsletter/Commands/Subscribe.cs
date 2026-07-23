using MediatR;
using Microsoft.EntityFrameworkCore;
using SedaWears.Application.Common.Interfaces;
using SedaWears.Application.Common.Exceptions;
using SedaWears.Domain.Entities;

namespace SedaWears.Application.Features.Newsletter.Commands;

public record SubscribeCommand(string Email) : IRequest
{
    public string Email { get; init; } = Email.Trim();
}

public class SubscribeCommandHandler(IApplicationDbContext context) : IRequestHandler<SubscribeCommand>
{
    public async Task Handle(SubscribeCommand request, CancellationToken cancellationToken)
    {
        var existing = await context.NewsletterSubscribers
            .FirstOrDefaultAsync(n => n.Email == request.Email, cancellationToken);

        if (existing != null)
        {
            if (existing.IsSubscribed) throw new ConflictException("You are already subscribed!");
            
            existing.IsSubscribed = true;
            existing.UnsubscribeToken = Guid.NewGuid().ToString("N");
        }
        else
        {
            context.NewsletterSubscribers.Add(new NewsletterSubscriber
            {
                Email = request.Email,
                IsSubscribed = true,
                UnsubscribeToken = Guid.NewGuid().ToString("N")
            });
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}
