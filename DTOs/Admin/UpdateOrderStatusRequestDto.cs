using QalatAldhaman.Store.Api.Entities.Enums;

namespace QalatAldhaman.Store.Api.DTOs.Admin;

public class UpdateOrderStatusRequestDto
{
    public OrderStatus Status { get; set; }
    public string? Notes { get; set; }
}
