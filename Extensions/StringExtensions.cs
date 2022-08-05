using System.Text;

namespace TwitchDice.Extensions
{
    public static class StringExtensions
    {
        public static string EscapeTags(this string str)
        {
            StringBuilder builder = new();

            for (int index = 0, length = str.Length; index < length; index++)
            {
                char c = str[index];

                bool tagChar = c == '<' || c == '>' || c == '=';

                if (tagChar)
                {
                    builder.Append("<noparse>");
                }

                builder.Append(c);

                if (tagChar)
                {
                    builder.Append("</noparse>");
                }
            }

            return builder.ToString();
        }
    }
}
