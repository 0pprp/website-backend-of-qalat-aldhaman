namespace QalatAldhaman.Store.Api.DTOs.Admin;

public class UpdateContractRequestDto
{
    /// <summary>رابط ملف PDF مرفوع مسبقاً عبر POST /api/uploads?folder=contracts</summary>
    public string ContractPdfUrl { get; set; } = string.Empty;
}
