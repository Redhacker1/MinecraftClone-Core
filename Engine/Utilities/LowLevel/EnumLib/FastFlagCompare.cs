using System.Runtime.CompilerServices;

namespace Engine.Utilities.LowLevel.EnumLib;

public static class FastFlagCompare
{
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool HasFlag( int var, int flag )
    {
        return (var & flag) != 0;
    }
}