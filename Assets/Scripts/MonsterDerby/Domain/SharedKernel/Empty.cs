namespace MonsterDerby.Domain.SharedKernel
{
    using System;

    internal static class Empty<T>
    {
        public static readonly T[] Array = System.Array.Empty<T>();
    }
}