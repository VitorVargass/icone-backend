using System.Globalization;

namespace icone_backend.Utils;

public static class NormalizeQuantity
{
    public static double Normalize(this string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return 0d;

        var v = raw.Trim();

        v = v.Replace(',', '.');

       
        var parts = v.Split('.', 2);
        var intPart = parts[0];

        
        intPart = intPart.TrimStart('0');
        if (intPart == string.Empty)
            intPart = "0";

        v = parts.Length == 2 ? $"{intPart}.{parts[1]}" : intPart;

        if (!double.TryParse(v,NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
        {
            throw new InvalidOperationException($"Invalid quantity: '{raw}'");
        }

        return result;
    }
}