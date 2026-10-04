namespace TokenGenerator.Services
{
    public static class CaesarCipher
    {
        public static string Encrypt(string text, int shift)
        {
            var normalizedShift = ((shift % 26) + 26) % 26;
            var chars = text.ToCharArray();

            for (var i = 0; i < chars.Length; i++)
            {
                var c = chars[i];
                if (c is >= 'a' and <= 'z')
                    chars[i] = (char)('a' + (c - 'a' + normalizedShift) % 26);
                else if (c is >= 'A' and <= 'Z')
                    chars[i] = (char)('A' + (c - 'A' + normalizedShift) % 26);
            }

            return new string(chars);
        }
    }
}
