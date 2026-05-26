using SedaWears.Domain.Common;

namespace SedaWears.Domain.Entities;

public class Guest : BaseEntity
{
    public string Email { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Phone { get; set; } = null!;
    public string Street { get; set; } = null!;
    public string City { get; set; } = null!;
    public string ZipCode { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
