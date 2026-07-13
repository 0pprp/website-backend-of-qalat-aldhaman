namespace QalatAldhaman.Store.Api.DTOs.Admin;

public class AdminLoginResponseDto
{
    public string Token { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
}
