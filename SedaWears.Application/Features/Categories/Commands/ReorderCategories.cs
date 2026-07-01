using MediatR;
using Microsoft.EntityFrameworkCore;
using SedaWears.Application.Common.Interfaces;

namespace SedaWears.Application.Features.Categories.Commands;

public record ReorderCategoryItem(int Id, int DisplayOrder);

public record ReorderCategoriesCommand(List<ReorderCategoryItem>? Orders, int? ShopId = null) : IRequest;

public class ReorderCategoriesHandler(IApplicationDbContext dbContext) : IRequestHandler<ReorderCategoriesCommand>
{
    public async Task Handle(ReorderCategoriesCommand request, CancellationToken ct)
    {
        var orderItemIds = request.Orders!.Select(o => o.Id).ToList();
        var query = dbContext.Categories.Where(c => orderItemIds.Contains(c.Id));
        if (request.ShopId.HasValue) query = query.Where(c => c.ShopId == request.ShopId);
        var categories = await query.ToListAsync(ct);

        foreach (var order in request.Orders!)
        {
            var category = categories.FirstOrDefault(c => c.Id == order.Id);
            if (category != null) category.DisplayOrder = order.DisplayOrder;
        }

        await dbContext.SaveChangesAsync(ct);
    }
}
