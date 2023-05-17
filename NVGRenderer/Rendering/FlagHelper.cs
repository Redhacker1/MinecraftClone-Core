using System.Runtime.CompilerServices;

namespace NVGRenderer.Rendering
{
    public static class FlagHelper
    {
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static bool HasFlag( int var, int flag )
        {
            return (var & flag) == flag;
        }
    }    
}
