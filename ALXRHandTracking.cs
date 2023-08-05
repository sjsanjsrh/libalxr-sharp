using System;
using System.Runtime.InteropServices;

namespace LibALXR
{
    [Flags]
    public enum ALXRSpaceLocationFlags : ulong
    {
        OrientationValidBit = 0x00000001,
        PositionValidBit = 0x00000002,
        OrientationTrackedBit = 0x00000004,
        PositionTrackedBit = 0x00000008
    }

    [Flags]
    public enum ALXRSpaceVelocityFlags : ulong
    {
        LinearValidBit = 0x00000001,
        AngularValidBit = 0x00000002
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ALXRHandJointLocation
    {
        [MarshalAs(UnmanagedType.U8)]
        public ALXRSpaceLocationFlags locationFlags;
        public ALXRPosef pose;
        public float radius;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ALXRHandJointVelocity
    {
        [MarshalAs(UnmanagedType.U8)]
        public ALXRSpaceVelocityFlags velocityFlags;
        public ALXRVector3f linearVelocity;
        public ALXRVector3f angularVelocity;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ALXRHandJointLocations
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 26)]
        public ALXRHandJointLocation[] jointLocations;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 26)]
        public ALXRHandJointVelocity[] jointVelocities;
        [MarshalAs(UnmanagedType.U1)]
        public bool isActive;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ALXRHandTracking
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public ALXRHandJointLocations[] hands;
    }
}
