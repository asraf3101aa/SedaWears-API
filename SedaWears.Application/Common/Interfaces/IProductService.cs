using SedaWears.Domain.Entities;

namespace SedaWears.Application.Common.Interfaces;

public interface IProductService
{
    Task<Product> GetProductByIdAsync(int productId, CancellationToken ct = default);
}
