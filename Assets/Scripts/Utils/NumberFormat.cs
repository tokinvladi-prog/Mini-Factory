using System.Globalization;

public static class NumberFormat
{
    private static readonly string[] Suffixes = { "", "K", "M", "B", "T", "Qa", "Qi", "Sx" };

    public static string Short(float value)
    {
        if (value < 0) return "-" + Short(-value);
        if (value < 1000) return value.ToString("0.##", CultureInfo.InvariantCulture);

        int tier = 0;
        float v = value;
        while (v >= 1000 && tier < Suffixes.Length - 1)
        {
            v /= 1000;
            tier++;
        }
        return v.ToString("0.##", CultureInfo.InvariantCulture) + Suffixes[tier];
    }
}
