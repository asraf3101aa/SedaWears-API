using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using SedaWears.Application.Common.Interfaces;
using SedaWears.Application.Common.Exceptions;
using SedaWears.Application.Features.PromoCodes.Models;
using SedaWears.Domain.Entities;
using SedaWears.Domain.Enums;

namespace SedaWears.Application.Features.PromoCodes.Queries;

public record GetPromoCodesQuery(int? ShopId, bool? IsActive) : IRequest<List<PromoCodeDto>>;

public record GetPromoCodeByIdQuery(int Id) : IRequest<PromoCodeDto>;

public class GetPromoCodesQueryHandler(
    IApplicationDbContext dbContext,
    ICurrentUser currentUser,
    UserManager<User> userManager) : IRequestHandler<GetPromoCodesQuery, List<PromoCodeDto>>
{
    public async Task<List<PromoCodeDto>> Handle(GetPromoCodesQuery request, CancellationToken ct)
    {
        var user = currentUser.Id.HasValue ? await userManager.FindByIdAsync(currentUser.Id.Value.ToString()) : null;
        var isAdmin = user != null && await userManager.IsInRoleAsync(user, nameof(UserRole.Admin));

        if (request.ShopId.HasValue)
        {
            // Shop-level list permission check
            if (!isAdmin)
            {
                var isMember = await dbContext.ShopOwners.AnyAsync(so => so.UserId == currentUser.Id && so.ShopId == request.ShopId.Value, ct)
                               || await dbContext.ShopManagers.AnyAsync(sm => sm.UserId == currentUser.Id && sm.ShopId == request.ShopId.Value, ct);

                if (!isMember)
                    throw new ForbiddenException("You are not authorized to view promo codes for this shop.");
            }
        }
        else if (!isAdmin)
        {
            throw new ForbiddenException("Only administrators can view global promo codes.");
        }

        var query = dbContext.PromoCodes.AsNoTracking();

        if (request.ShopId.HasValue)
        {
            query = query.Where(p => p.ShopId == request.ShopId.Value);
        }
        else
        {
            query = query.Where(p => p.ShopId == null);
        }

        if (request.IsActive.HasValue)
        {
            query = query.Where(p => p.IsActive == request.IsActive.Value);
        }

        var list = await query
            .OrderByDescending(p => p.Id)
            .ToListAsync(ct);

        return list.Select(PromoCodeDto.FromEntity).ToList();
    }
}

public class GetPromoCodeByIdQueryHandler(
    IApplicationDbContext dbContext,
    ICurrentUser currentUser,
    UserManager<User> userManager) : IRequestHandler<GetPromoCodeByIdQuery, PromoCodeDto>
{
    public async Task<PromoCodeDto> Handle(GetPromoCodeByIdQuery request, CancellationToken ct)
    {
        var promoCode = await dbContext.PromoCodes
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.Id, ct)
            ?? throw new NotFoundException("Promo code not found.");

        var user = currentUser.Id.HasValue ? await userManager.FindByIdAsync(currentUser.Id.Value.ToString()) : null;
        var isAdmin = user != null && await userManager.IsInRoleAsync(user, nameof(UserRole.Admin));

        if (promoCode.ShopId.HasValue)
        {
            if (!isAdmin)
            {
                var isMember = await dbContext.ShopOwners.AnyAsync(so => so.UserId == currentUser.Id && so.ShopId == promoCode.ShopId.Value, ct)
                               || await dbContext.ShopManagers.AnyAsync(sm => sm.UserId == currentUser.Id && sm.ShopId == promoCode.ShopId.Value, ct);

                if (!isMember)
                    throw new ForbiddenException("You are not authorized to view this shop's promo codes.");
            }
        }
        else if (!isAdmin)
        {
            throw new ForbiddenException("Only administrators can view global promo codes.");
        }

        return PromoCodeDto.FromEntity(promoCode);
    }
}
