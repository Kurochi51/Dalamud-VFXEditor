using System.Runtime.InteropServices;

namespace VfxEditor.Structs.Animation {
    public enum CharacterModes : byte {
        None = 0,
        Normal = 1,
        EmoteLoop = 3,
        Mounted = 4,
        AnimLock = 8,
        Carrying = 9,
        InPositionLoop = 11,
        Performance = 16,
    }

    public enum AnimationSlots : int {
        FullBody = 0,
        UpperBody = 1,
        Facial = 2,
        Add = 3,
        Lips = 7,
        Parts1 = 8,
        Parts2 = 9,
        Parts3 = 10,
        Parts4 = 11,
        Overlay = 12,
    }

    [StructLayout( LayoutKind.Explicit )]
    public unsafe struct ActorMemoryStruct {
        [FieldOffset( 0x1AD4 )] public byte CharacterMode;
        [FieldOffset( 0x1AD5 )] public byte CharacterModeInput;
        [FieldOffset( 0x09C0 )] public AnimationMemory Animation;

        public bool CanAnimate => ( CharacterModes)CharacterMode == CharacterModes.Normal || ( CharacterModes)CharacterMode == CharacterModes.AnimLock;
        public bool IsAnimationOverride => ( CharacterModes )CharacterMode == CharacterModes.AnimLock;
    }

    [StructLayout( LayoutKind.Explicit )]
    public unsafe struct AnimationMemory {
        [FieldOffset( 0x000 )] public fixed ushort AnimationIds[13];
        [FieldOffset( 0x074 )] public fixed float Speeds[13];
        [FieldOffset( 0x102 )] public byte SpeedTrigger;
        [FieldOffset( 0x1EC )] public ushort BaseOverride;
        [FieldOffset( 0x1EE )] public ushort LipsOverride;
    }
}
