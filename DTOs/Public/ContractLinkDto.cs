using QalatAldhaman.Store.Api.Entities.Enums;

namespace QalatAldhaman.Store.Api.DTOs.Public;

public class ContractLinkDto
{
    public PurchaseMethod PurchaseMethod { get; set; }
    public string ContractPdfUrl { get; set; } = string.Empty;
}
