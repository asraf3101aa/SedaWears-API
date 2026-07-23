using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SedaWears.Application.Features.Wishlist.Commands;
using SedaWears.Application.Features.Wishlist.Queries;


namespace SedaWears.Presentation.Controllers;

[ApiController]
[Route("profile/wishlist")]
[Authorize]
public class WishlistController(ISender mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetWishlist(CancellationToken ct)
        => Ok(await mediator.Send(new GetWishlistQuery(), ct));

    [HttpPost("{productId:int}")]
    public async Task<IActionResult> AddToWishlist(int productId, CancellationToken ct)
    {
        await mediator.Send(new AddToWishlistCommand(productId), ct);
        return StatusCode(StatusCodes.Status201Created);
    }

    [HttpDelete("{productId:int}")]
    public async Task<IActionResult> RemoveFromWishlist(int productId, CancellationToken ct)
    {
        await mediator.Send(new RemoveFromWishlistCommand(productId), ct);
        return NoContent();
    }
}
