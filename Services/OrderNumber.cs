using System.Security.Cryptography;

namespace RaqmiWeb.Services
{
    /// <summary>
    /// Korta, läsbara ordernummer utan databas.
    /// Format: TS-<mmmmmm><rr> (base36) – ca 10 tecken totalt.
    /// </summary>
    public static class OrderNumber
    {
        private static readonly DateTime Epoch = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static string New(string prefix = "TS", int randomChars = 2)
        {
            // 1) Tidsdel = minuter sedan 2024-01-01 UTC → base36, 5 tecken
            var minutes = (int)(DateTime.UtcNow - Epoch).TotalMinutes;   // räcker i många år
            var timePart = ToBase36(minutes).PadLeft(5, '0');

            // 2) Slumpdel = random base36, t.ex. 2 tecken (36^2 = 1296 per minut)
            var randPart = RandomBase36(randomChars);

            return $"{prefix}-{timePart}{randPart}";
        }

        private static string RandomBase36(int len)
        {
            if (len <= 0) return "";
            Span<byte> buf = stackalloc byte[len];
            RandomNumberGenerator.Fill(buf);
            const string alphabet = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            Span<char> chars = stackalloc char[len];
            for (int i = 0; i < len; i++)
                chars[i] = alphabet[buf[i] % 36];
            return new string(chars);
        }

        private static string ToBase36(int value)
        {
            const string alphabet = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            if (value == 0) return "0";
            Span<char> buf = stackalloc char[16];
            int i = buf.Length;
            uint v = (uint)value; // säker
            while (v > 0)
            {
                buf[--i] = alphabet[(int)(v % 36)];
                v /= 36;
            }
            return new string(buf[i..]);
        }
    }
}
