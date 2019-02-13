using DbUp.Engine.Output;
using System;

namespace DbUp.Cli
{
    public class ConsoleLogger: IUpgradeLog
    {
        public void WriteError(string format, params object[] args)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            try
            {
                Console.WriteLine($"[ERR] {string.Format(format, args)}");
            }
            finally
            {
                Console.ResetColor();
            }
        }

        public void WriteInformation(string format, params object[] args)
        {
            Console.WriteLine(string.Format(format, args));
        }

        public void WriteWarning(string format, params object[] args)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            try
            {
                Console.WriteLine($"[WRN] {string.Format(format, args)}");
            }
            finally
            {
                Console.ResetColor();
            }

        }
    }
}
