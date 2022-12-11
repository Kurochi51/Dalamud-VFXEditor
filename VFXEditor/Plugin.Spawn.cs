using VfxEditor.Structs.Vfx;

namespace VfxEditor {
    public unsafe partial class Plugin {
        public static BaseVfx Spawn { get; private set; }

        public static void RemoveSpawn() {
            if( Spawn == null ) return;
            Spawn?.Remove();
            Spawn = null;
        }

        public static void SpawnOnGround( string path ) {
            Spawn = new StaticVfx( path, ClientState.LocalPlayer.Position, ClientState.LocalPlayer.Rotation );
        }

        public static void SpawnOnSelf( string path ) {
            Spawn = new ActorVfx( ClientState.LocalPlayer, ClientState.LocalPlayer, path );
        }

        public static void SpawnOnTarget( string path ) {
            var target = TargetManager.Target;
            if( target != null ) {
                Spawn = new ActorVfx( target, target, path );
            }
        }

        public static void ClearSpawn() {
            Spawn = null;
        }

        public static bool SpawnExists() => Spawn != null;
    }
}
