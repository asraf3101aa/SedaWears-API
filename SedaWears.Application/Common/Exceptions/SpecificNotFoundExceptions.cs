namespace SedaWears.Application.Common.Exceptions;

public class UserNotFoundException : NotFoundException
{
    public UserNotFoundException() : base("User not found.") { }
    public UserNotFoundException(string customMessage) : base(customMessage) { }
}

public class AddressNotFoundException : NotFoundException
{
    public AddressNotFoundException() : base("Address not found.") { }
    public AddressNotFoundException(string customMessage) : base(customMessage) { }
}

public class ProductNotFoundException : NotFoundException
{
    public ProductNotFoundException() : base("Product not found.") { }
    public ProductNotFoundException(string customMessage) : base(customMessage) { }
}

public class CategoryNotFoundException : NotFoundException
{
    public CategoryNotFoundException() : base("Category not found.") { }
    public CategoryNotFoundException(string customMessage) : base(customMessage) { }
}

public class ShopNotFoundException : NotFoundException
{
    public ShopNotFoundException() : base("Shop not found.") { }
    public ShopNotFoundException(string customMessage) : base(customMessage) { }
}

public class PromoCodeNotFoundException : NotFoundException
{
    public PromoCodeNotFoundException() : base("Promo code not found.") { }
    public PromoCodeNotFoundException(string customMessage) : base(customMessage) { }
}

public class InvitationNotFoundException : NotFoundException
{
    public InvitationNotFoundException() : base("Invitation not found.") { }
    public InvitationNotFoundException(string customMessage) : base(customMessage) { }
}

public class CartItemNotFoundException : NotFoundException
{
    public CartItemNotFoundException() : base("Cart item not found.") { }
    public CartItemNotFoundException(string customMessage) : base(customMessage) { }
}
