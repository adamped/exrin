﻿namespace Exrin.Abstraction
{
    using System;

    [Flags]
    public enum AssemblyAction
    {
        StaticInitialize = 1,
        Bootstrapper = 2
    }
}
