using System;
using System.Collections.Generic;
using System.Text;

namespace DbUp.Cli
{
    public enum Transaction
    {
        None,
        PerScript,
        Single
    }
}
