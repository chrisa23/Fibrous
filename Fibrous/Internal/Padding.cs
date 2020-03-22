using System.Runtime.InteropServices;

namespace Fibrous.Internal
{
    /// <summary>
    /// A pad of 56 bytes, useful in combination with a 8 byte data field.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    internal struct Pad56
    {
        long p00, p01, p02, p03, p04, p05, p06;
    }

    /// <summary>
    /// A pad of 64 bytes.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    internal struct Pad64
    {
        long p00, p01, p02, p03, p04, p05, p06, p07;
    }

    /// <summary>
    /// A pad of 120 bytes, useful in combination with a 8 byte data field.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    internal struct Pad120
    {
        long p00, p01, p02, p03, p04, p05, p06;
        long p08, p09, p10, p11, p12, p13, p14, p15;
    }

    /// <summary>
    /// A pad of 112 bytes, useful in combination with a 16 byte data field.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    internal struct Pad112
    {
        long p00, p01, p02, p03, p04, p05;
        long p08, p09, p10, p11, p12, p13, p14, p15;
    }

    /// <summary>
    /// A pad of 14 bytes, useful in combination with a 24 byte data field.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    internal struct Pad104
    {
        long p00, p01, p02, p03, p04;
        long p08, p09, p10, p11, p12, p13, p14, p15;
    }

    /// <summary>
    /// A pad of 128 bytes.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    internal struct Pad128
    {
        long p00, p01, p02, p03, p04, p05, p06, p07;
        long p08, p09, p10, p11, p12, p13, p14, p15;
    }
}
