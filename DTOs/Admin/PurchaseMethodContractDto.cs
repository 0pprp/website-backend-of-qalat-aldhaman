using QalatAldhaman.Store.Api.Entities.Enums;

namespace QalatAldhaman.Store.Api.DTOs.Admin;

public class PurchaseMethodContractDto
{
    public PurchaseMethod PurchaseMethod { get; set; }
    public string? ContractPdfUrl { get; set; }
    public DateTime UpdatedAt { get; set; }
}
