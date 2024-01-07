namespace FSC.Beauty.Compile
{
    internal static class StringExtension
    {
        internal static string SingleTrim(this string value, char charToTrim)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }

            ReadOnlySpan<char> span = value.AsSpan();
            int start = 0, end = span.Length;

            if (span.Length > 0 && span[0] == charToTrim)
            {
                start = 1;
            }

            if (span.Length > 1 && span[^1] == charToTrim)
            {
                end --;
            }

            if (start == 0 && end == span.Length)
            {
                return value;
            }

            return span.Slice(start, end - start).ToString();
        }
    }
}
