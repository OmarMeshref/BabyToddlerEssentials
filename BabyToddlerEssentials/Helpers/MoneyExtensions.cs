namespace BabyToddlerEssentials.Helpers
{
    public static class MoneyExtensions
    {
        // Formats a decimal as Jordanian Dinar, e.g. 12.50 JOD (2 decimals)
        public static string ToJod(this decimal amount) => $"{amount:0.00} JOD";
    }
}