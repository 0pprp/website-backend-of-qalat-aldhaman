using QalatAldhaman.Store.Api.Entities.Enums;

namespace QalatAldhaman.Store.Api.DTOs.Public;

public class PackageOrderItemDto
{
    public int ProductId { get; set; }
    public int Quantity { get; set; } = 1;
}

public class CreatePackageOrderRequestDto
{
    public int PackageId { get; set; }
    public PurchaseMethod PurchaseMethod { get; set; }
    public List<PackageOrderItemDto> PackageOrderItems { get; set; } = [];
    public string CustomerName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public int GovernorateId { get; set; }
    public string? ShopName { get; set; }
    public string? ShopAddress { get; set; }
    public string? HomeAddress { get; set; }
    public string? NearestLandmark { get; set; }
    public string? MediaUrl { get; set; }
    public MediaType? MediaType { get; set; }
    public decimal? GpsLat { get; set; }
    public decimal? GpsLng { get; set; }
    public string? CustomProductDescription { get; set; }
}
