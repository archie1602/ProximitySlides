namespace ProximitySlides.Core.Extensions;

public static class StringExtensions
{
    public static string ReplaceFirst(this string str, string oldValue, string newValue)
    {
        var position = str.IndexOf(oldValue, StringComparison.Ordinal);
        
        if (position < 0)
        {
            return str;
        }
        
        str = string.Concat(str.AsSpan(0, position), newValue, str.AsSpan(position + oldValue.Length));
        
        return str;
    }
    
    public static string CompressJson(this string jsonStr)
    {
        return jsonStr
            .ReplaceFirst("\"u\":\"", "u:")
            .ReplaceFirst("\"c\":\"", "c:")
            .ReplaceFirst("\"t\":\"", "t:")
            .Replace("\",", ",")
            .Replace("\"}", "")
            .ReplaceFirst("{", "");
    }
    
    public static string DecompressJson(this string jsonStr)
    {
        return "{" + jsonStr
            .ReplaceFirst("u:", "\"u\":\"")
            .ReplaceFirst("c:", "\"c\":\"")
            .ReplaceFirst("t:", "\"t\":\"")
            .Replace(",", "\",") + "\"}";
    }
}