using System;
using System.Text;

namespace DbUp.Cli
{
    public static class StringUtils
    {
        public static string ExpandEnvironmentVariables(string name)
        {
            // Implementation grabbed fromn here: https://github.com/mono/mono/blob/master/mcs/class/corlib/System/Environment.cs

            if (name == null)
                throw new ArgumentNullException(nameof(name));

            char marker = '$';

            int off1 = name.IndexOf(marker);
            if (off1 == -1)
                return name;

            int len = name.Length;
            int off2;
            if (off1 == len - 1 || (off2 = name.IndexOf(marker, off1 + 1)) == -1)
                return name;

            var result = new StringBuilder();
            result.Append(name, 0, off1);
            do
            {
                string var = name.Substring(off1 + 1, off2 - off1 - 1);
                string value = Environment.GetEnvironmentVariable(var);

                // If value not found, add $FOO to stream,
                //  and use the closing $ for the next iteration.
                // If value found, expand it in place of $FOO$
                int realOldOff2 = off2;
                if (value == null)
                {
                    result.Append(marker);
                    result.Append(var);
                    off2--;
                }
                else
                {
                    result.Append(value);
                }
                int oldOff2 = off2;
                off1 = name.IndexOf(marker, off2 + 1);
                // If no $ found for off1, don't look for one for off2
                off2 = (off1 == -1 || off2 > len - 1) ? -1 : name.IndexOf(marker, off1 + 1);
                // textLen is the length of text between the closing $ of current iteration
                //  and the starting $ of the next iteration if any. This text is added to output
                int textLen;
                // If no new $ found, use all the remaining text
                if (off1 == -1 || off2 == -1)
                    textLen = len - oldOff2 - 1;
                // If value found in current iteration, use text after current closing $ and next $
                else if (value != null)
                    textLen = off1 - oldOff2 - 1;
                // If value not found in current iteration, but a $ was found for next iteration,
                //  use text from current closing $ to the next $.
                else
                    textLen = off1 - realOldOff2;
                if (off1 >= oldOff2 || off1 == -1)
                    result.Append(name, oldOff2 + 1, textLen);
            } while (off2 > -1 && off2 < len);

            return result.ToString();
        }
    }
}
