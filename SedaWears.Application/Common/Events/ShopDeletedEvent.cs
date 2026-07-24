namespace SedaWears.Application.Common.Events;

public record ShopDeletedEvent(int ShopId, List<int> AffectedUserIds);
