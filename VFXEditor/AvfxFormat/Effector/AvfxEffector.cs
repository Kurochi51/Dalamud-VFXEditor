using ImGuiNET;
using OtterGui.Raii;
using System.Collections.Generic;
using System.IO;
using VfxEditor.AvfxFormat.Nodes;
using static VfxEditor.AvfxFormat.Enums;

namespace VfxEditor.AvfxFormat {
    public class AvfxEffector : AvfxNode {
        public const string NAME = "Efct";

        public readonly AvfxEnum<EffectorType> EffectorVariety = new( "Type", "EfVT" );
        public readonly AvfxEnum<RotationOrder> RotationOrder = new( "Rotation Order", "RoOT" );
        public readonly AvfxEnum<CoordComputeOrder> CoordComputeOrder = new( "Coordinate Compute Order", "CCOT" );
        public readonly AvfxBool AffectOtherVfx = new( "Affect Other VFX", "bAOV" );
        public readonly AvfxBool AffectGame = new( "Affect Game", "bAGm" );
        public readonly AvfxInt LoopPointStart = new( "Loop Start", "LpSt" );
        public readonly AvfxInt LoopPointEnd = new( "Loop End", "LpEd" );
        public AvfxData Data;

        private readonly List<AvfxBase> Parsed;

        private readonly UiDisplayList Parameters;

        public AvfxEffector() : base( NAME, AvfxNodeGroupSet.EffectorColor ) {
            Parsed = new() {
                EffectorVariety,
                RotationOrder,
                CoordComputeOrder,
                AffectOtherVfx,
                AffectGame,
                LoopPointStart,
                LoopPointEnd
            };

            EffectorVariety.Parsed.ExtraCommandGenerator = () => {
                return new AvfxEffectorDataCommand( this );
            };

            Parameters = new( "Parameters", new() {
                new UiNodeGraphView( this ),
                RotationOrder,
                CoordComputeOrder,
                AffectOtherVfx,
                AffectGame,
                LoopPointStart,
                LoopPointEnd
            } );
        }

        public override void ReadContents( BinaryReader reader, int size ) {
            Peek( reader, Parsed, size );
            var effectorType = EffectorVariety.GetValue();

            ReadNested( reader, ( BinaryReader _reader, string _name, int _size ) => {
                if( _name == "Data" ) {
                    SetData( effectorType );
                    Data?.Read( _reader, _size );
                }
            }, size );
        }

        protected override void RecurseChildrenAssigned( bool assigned ) {
            RecurseAssigned( Parsed, assigned );
            RecurseAssigned( Data, assigned );
        }

        protected override void WriteContents( BinaryWriter writer ) {
            WriteNested( writer, Parsed );
            Data?.Write( writer );
        }

        public void SetData( EffectorType type ) {
            Data = type switch {
                EffectorType.PointLight => new AvfxEffectorDataPointLight(),
                EffectorType.DirectionalLight => new AvfxEffectorDataDirectionalLight(),
                EffectorType.RadialBlur => new AvfxEffectorDataRadialBlur(),
                EffectorType.BlackHole => null,
                EffectorType.CameraQuake2_Unknown or EffectorType.CameraQuake => new AvfxEffectorDataCameraQuake(),
                _ => null
            };
            Data?.SetAssigned( true );
        }

        public override void Draw() {
            using var _ = ImRaii.PushId( "Effector" );
            DrawRename();
            EffectorVariety.Draw();
            ImGui.SetCursorPosY( ImGui.GetCursorPosY() + 5 );

            using var tabBar = ImRaii.TabBar( "Tabs", ImGuiTabBarFlags.NoCloseWithMiddleMouseButton );
            if( !tabBar ) return;

            DrawParameters();
            DrawData();
        }

        private void DrawParameters() {
            using var tabItem = ImRaii.TabItem( "Parameters" );
            if( !tabItem ) return;

            Parameters.Draw();
        }

        private void DrawData() {
            if( Data == null ) return;

            using var tabItem = ImRaii.TabItem( "Data" );
            if( !tabItem ) return;

            Data.Draw();
        }

        public override string GetDefaultText() => $"Effector {GetIdx()}({EffectorVariety.GetValue()})";

        public override string GetWorkspaceId() => $"Effct{GetIdx()}";
    }
}
