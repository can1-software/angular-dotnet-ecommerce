using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace ECommerce.API.Helpers;

public static class SlugHelper
{
    public static string Generate(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return "item";

        var normalized = text.Trim().ToLowerInvariant()
            .Replace('ç', 'c').Replace('ğ', 'g').Replace('ı', 'i')
            .Replace('ö', 'o').Replace('ş', 's').Replace('ü', 'u');

        var sb = new StringBuilder();
        foreach (var c in normalized.Normalize(NormalizationForm.FormD))
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.NonSpacingMark)
                continue;

            if (char.IsLetterOrDigit(c))
                sb.Append(c);
            else if (c is ' ' or '-' or '_' or '&' or '/')
                sb.Append('-');
        }

        var slug = Regex.Replace(sb.ToString(), "-+", "-").Trim('-');
        return string.IsNullOrEmpty(slug) ? "item" : slug;
    }

    public static async Task<string> GenerateUniqueAsync(
        string name,
        Func<string, Task<bool>> existsAsync)
    {
        var baseSlug = Generate(name);
        var slug = baseSlug;
        var suffix = 2;

        while (await existsAsync(slug))
        {
            slug = $"{baseSlug}-{suffix}";
            suffix++;
        }

        return slug;
    }
}
