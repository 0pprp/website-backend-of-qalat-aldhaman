using System.Text;
using System.Text.RegularExpressions;

namespace QalatAldhaman.Store.Api.Services;

/// <summary>
/// يبقي الأحرف العربية كما هي بدل تحويلها صوتياً لحروف لاتينية (ترجمة صوتية دقيقة غير موثوقة)،
/// لأن المتصفحات ومحركات البحث الحديثة تدعم الروابط بالعربي (Unicode) مباشرة.
/// </summary>
public static class SlugGenerator
{
    private static readonly Regex InvalidChars = new(@"[^\p{L}\p{Nd}]+", RegexOptions.Compiled);

    public static string Slugify(string input)
    {
        var trimmed = input.Trim();
        var withHyphens = InvalidChars.Replace(trimmed, "-").Trim('-');

        var builder = new StringBuilder(withHyphens.Length);
        foreach (var c in withHyphens)
        {
            builder.Append(char.IsLetter(c) && c <= 'z' ? char.ToLowerInvariant(c) : c);
        }

        var slug = builder.ToString();
        return string.IsNullOrWhiteSpace(slug) ? Guid.NewGuid().ToString("N")[..8] : slug;
    }
}
