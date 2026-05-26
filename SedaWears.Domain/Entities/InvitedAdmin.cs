using SedaWears.Domain.Common;

namespace SedaWears.Domain.Entities;

public class InvitedAdmin : BaseEntity
{
    public string Email { get; set; } = null!;
    public string Token { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
