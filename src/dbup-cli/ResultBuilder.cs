using Optional;
using System;

namespace DbUp.Cli
{
    internal class ResultBuilder
    {
        public Option<int, Error> FromUpgradeResult(Engine.DatabaseUpgradeResult result)
        {
            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            if (result.Successful)
            {
                return Option.Some<int, Error>(0);
            }

            var msg = "Failed to perform upgrade: ";
            if (result.Error == null)
            {
                msg += "Undefined error";
            }
            else
            {
                msg += result.Error.Message;
                if (result.Error.InnerException != null)
                {
                    msg += $"{Environment.NewLine}    Details: {result.Error.InnerException.Message}";
                }
            }
            if (result.ErrorScript != null)
            {
                msg += $"{Environment.NewLine}    Script: {result.ErrorScript.Name}";
            }

            return Option.None<int, Error>(Error.Create(msg));
        }
    }
}
