using System.Text.RegularExpressions;

namespace SLCDM.Application.Common;

public static class BssidFormat
{
    private static readonly Regex Hex = new("[^0-9a-fA-F]", RegexOptions.Compiled);

    public static string? Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var hex = Hex.Replace(value, string.Empty).ToLowerInvariant();
        if (hex.Length != 12)
        {
            return null;
        }

        return string.Create(17, hex, static (span, src) =>
        {
            var offset = 0;
            for (var i = 0; i < 6; i++)
            {
                if (i > 0)
                {
                    span[offset++] = ':';
                }

                span[offset++] = src[i * 2];
                span[offset++] = src[(i * 2) + 1];
            }
        });
    }
}
