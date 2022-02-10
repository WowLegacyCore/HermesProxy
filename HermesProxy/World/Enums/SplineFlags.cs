using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HermesProxy.World.Enums
{
    [Flags]
    public enum SplineFlagVanilla : uint
    {
        None                = 0x00000000,
        Done                = 0x00000001,
        Falling             = 0x00000002,           // Affects elevation computation
        Unknown3            = 0x00000004,
        Unknown4            = 0x00000008,
        Unknown5            = 0x00000010,
        Unknown6            = 0x00000020,
        Unknown7            = 0x00000040,
        Unknown8            = 0x00000080,
        Runmode             = 0x00000100,
        Flying              = 0x00000200,           // Smooth movement(Catmullrom interpolation mode), flying animation
        NoSpline            = 0x00000400,
        Unknown12           = 0x00000800,
        Unknown13           = 0x00001000,
        Unknown14           = 0x00002000,
        Unknown15           = 0x00004000,
        Unknown16           = 0x00008000,
        FinalPoint          = 0x00010000,
        FinalTarget         = 0x00020000,
        FinalOrientation    = 0x00040000,
        Unknown19           = 0x00080000,           // exists, but unknown what it does
        Cyclic              = 0x00100000,           // Movement by cycled spline
        EnterCycle          = 0x00200000,           // Everytimes appears with cyclic flag in monster move packet, erases first spline vertex after first cycle done
        Frozen              = 0x00400000,           // Will never arrive
        Unknown23           = 0x00800000,
        Unknown24           = 0x01000000,
        Unknown25           = 0x02000000,           // exists, but unknown what it does
        Unknown26           = 0x04000000,
        Unknown27           = 0x08000000,
        Unknown28           = 0x10000000,
        Unknown29           = 0x20000000,
        Unknown30           = 0x40000000,
        Unknown31           = 0x80000000,
    }

    [Flags]
    public enum SplineFlagTBC : uint
    {
        None                = 0x00000000,
        Done                = 0x00000001,
        Falling             = 0x00000002,           // Affects elevation computation
        Unknown3            = 0x00000004,
        Unknown4            = 0x00000008,
        Unknown5            = 0x00000010,
        Unknown6            = 0x00000020,
        Unknown7            = 0x00000040,
        Unknown8            = 0x00000080,
        Runmode             = 0x00000100,
        Flying              = 0x00000200,           // Smooth movement(Catmullrom interpolation mode), flying animation
        NoSpline            = 0x00000400,
        Unknown12           = 0x00000800,
        Unknown13           = 0x00001000,
        Unknown14           = 0x00002000,
        Unknown15           = 0x00004000,
        Unknown16           = 0x00008000,
        FinalPoint          = 0x00010000,
        FinalTarget         = 0x00020000,
        FinalOrientation    = 0x00040000,
        Unknown19           = 0x00080000,           // exists, but unknown what it does
        Cyclic              = 0x00100000,           // Movement by cycled spline
        EnterCycle          = 0x00200000,           // Everytimes appears with cyclic flag in monster move packet, erases first spline vertex after first cycle done
        Frozen              = 0x00400000,           // Will never arrive
        Unknown23           = 0x00800000,
        Unknown24           = 0x01000000,
        Unknown25           = 0x02000000,           // exists, but unknown what it does
        Unknown26           = 0x04000000,
        Unknown27           = 0x08000000,
        Unknown28           = 0x10000000,
        Unknown29           = 0x20000000,
        Unknown30           = 0x40000000,
        Unknown31           = 0x80000000,
    }

    [Flags]
    public enum SplineFlagWotLK : uint
    {
        None                = 0x00000000,
        AnimTierSwim        = 0x00000001,
        AnimTierHover       = 0x00000002,
        AnimTierFly         = 0x00000003,
        AnimTierSubmerged   = 0x00000004,
        Done                = 0x00000100,
        Falling             = 0x00000200,
        NoSpline            = 0x00000400,
        Trajectory          = 0x00000800,
        WalkMode            = 0x00001000,
        Flying              = 0x00002000,
        Knockback           = 0x00004000,
        FinalPoint          = 0x00008000,
        FinalTarget         = 0x00010000,
        FinalOrientation    = 0x00020000,
        CatmullRom          = 0x00040000,
        Cyclic              = 0x00080000,
        EnterCycle          = 0x00100000,
        AnimationTier       = 0x00200000,
        Frozen              = 0x00400000,
        Transport           = 0x00800000,
        TransportExit       = 0x01000000,
        Unknown7            = 0x02000000,
        Unknown8            = 0x04000000,
        OrientationInverted = 0x08000000,
        UsePathSmoothing    = 0x10000000,
        Animation           = 0x20000000,
        UncompressedPath    = 0x40000000,
        Unknown10           = 0x80000000
    }

    [Flags]
    public enum SplineFlagModern : uint
    {
        None                = 0x00000000,
        AnimTierSwim        = 0x00000001,
        AnimTierHover       = 0x00000002,
        AnimTierFly         = 0x00000003,
        AnimTierSubmerged   = 0x00000004,
        Unknown0            = 0x00000008,
        FallingSlow         = 0x00000010,
        Done                = 0x00000020,
        Falling             = 0x00000040,
        NoSpline            = 0x00000080,
        Unknown1            = 0x00000100,
        Flying              = 0x00000200,
        OrientationFixed    = 0x00000400,
        CatmullRom          = 0x00000800,
        Cyclic              = 0x00001000,
        EnterCycle          = 0x00002000,
        Frozen              = 0x00004000,
        TransportEnter      = 0x00008000,
        TransportExit       = 0x00010000,
        Unknown2            = 0x00020000,
        Unknown3            = 0x00040000,
        Backward            = 0x00080000,
        SmoothGroundPath    = 0x00100000,
        CanSwim             = 0x00200000,
        UncompressedPath    = 0x00400000,
        Unknown4            = 0x00800000,
        Unknown5            = 0x01000000,
        Animation           = 0x02000000,
        Parabolic           = 0x04000000,
        FadeObject          = 0x08000000,
        Steering            = 0x10000000,
        Unknown8            = 0x20000000,
        Unknown9            = 0x40000000,
        Unknown10           = 0x80000000,
    }
}
