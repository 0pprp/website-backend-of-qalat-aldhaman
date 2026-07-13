namespace QalatAldhaman.Store.Api.DTOs.Admin;

public class UpdateProductContractRequestDto
{
    /// <summary>رابط ملف PDF مرفوع مسبقاً عبر POST /api/uploads</summary>
    public string ContractPdfUrl { get; set; } = string.Empty;
}
