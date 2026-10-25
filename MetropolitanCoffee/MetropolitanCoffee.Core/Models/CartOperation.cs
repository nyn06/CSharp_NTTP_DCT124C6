namespace MetropolitanCoffee.Core.Models;

public sealed record CartOperation(bool Success, string? Error = null);

public sealed record CartOptions(string Size, string Ice, string Sugar)
{
    public static CartOptions ForAdd(string? size, string? ice, string? sugar, bool hasSize, bool hasIce, bool hasSugar)
        => new(Normalize(size, hasSize, ["S", "M", "L"], "M"),
               Normalize(ice, hasIce, ["less", "normal", "more"], "normal"),
               Normalize(sugar, hasSugar, ["less", "normal", "more"], "normal"));

    public static CartOptions ForUpdate(string? size, string? ice, string? sugar, CartLine current)
        => new(Normalize(size ?? current.Size, current.HasSize || current.Size != "none", ["S", "M", "L"], current.Size == "none" ? "M" : current.Size),
               Normalize(ice ?? current.Ice, current.HasIce || current.Ice != "none", ["less", "normal", "more"], current.Ice == "none" ? "normal" : current.Ice),
               Normalize(sugar ?? current.Sugar, current.HasSugar || current.Sugar != "none", ["less", "normal", "more"], current.Sugar == "none" ? "normal" : current.Sugar));

    private static string Normalize(string? value, bool enabled, string[] allowed, string fallback)
    {
        if (!enabled) return "none";
        value = value?.Trim();
        return value is not null && allowed.Contains(value, StringComparer.Ordinal) ? value : fallback;
    }
}
