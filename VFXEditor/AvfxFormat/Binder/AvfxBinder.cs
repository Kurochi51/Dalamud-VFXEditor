using ImGuiNET;
using OtterGui.Raii;
using System.Collections.Generic;
using System.IO;
using VfxEditor.AvfxFormat.Nodes;
using static VfxEditor.AvfxFormat.Enums;

namespace VfxEditor.AvfxFormat {
    public class AvfxBinder : AvfxNode {
        public const string NAME = "Bind";

        public readonly AvfxBool StartToGlobalDirection = new( "Start To Global Direction", "bStG" );
        public readonly AvfxBool VfxScaleEnabled = new( "VFX Scale", "bVSc" );
        public readonly AvfxFloat VfxScaleBias = new( "VFX Scale Bias", "bVSb" );
        public readonly AvfxBool VfxScaleDepthOffset = new( "VFX Scale Depth Offset", "bVSd" );
        public readonly AvfxBool VfxScaleInterpolation = new( "VFX Scale Interpolation", "bVSi" );
        public readonly AvfxBool TransformScale = new( "Transform Scale", "bTSc" );
        public readonly AvfxBool TransformScaleDepthOffset = new( "Transform Scale Depth Offset", "bTSd" );
        public readonly AvfxBool TransformScaleInterpolation = new( "Transform Scale Interpolation", "bTSi" );
        public readonly AvfxBool FollowingTargetOrientation = new( "Following Target Orientation", "bFTO" );
        public readonly AvfxBool DocumentScaleEnabled = new( "Document Scale Enabled", "bDSE" );
        public readonly AvfxBool AdjustToScreenEnabled = new( "Adjust to Screen", "bATS" );
        public readonly AvfxBool BET_Unknown = new( "BET (Unknown)", "bBET" );
        public readonly AvfxInt Life = new( "Life", "Life" );
        public readonly AvfxEnum<BinderRotation> BinderRotationType = new( "Binder Rotation Type", "RoTp" );
        public readonly AvfxEnum<BinderType> BinderVariety = new( "Type", "BnVr" );
        public readonly AvfxBinderProperties PropStart = new( "Properties Start", "PrpS" );
        public readonly AvfxBinderProperties Prop1 = new( "Properties 1", "Prp1" );
        public readonly AvfxBinderProperties Prop2 = new( "Properties 2", "Prp2" );
        public readonly AvfxBinderProperties PropGoal = new( "Properties Goal", "PrpG" );
        public AvfxData Data;

        private readonly List<AvfxBase> Parsed;

        private readonly AvfxDisplaySplitView<AvfxBinderProperties> PropSplitDisplay;
        private readonly UiDisplayList Parameters;

        public AvfxBinder() : base( NAME, AvfxNodeGroupSet.BinderColor ) {
            Parsed = new() {
                StartToGlobalDirection,
                VfxScaleEnabled,
                VfxScaleBias,
                VfxScaleDepthOffset,
                VfxScaleInterpolation,
                TransformScale,
                TransformScaleDepthOffset,
                TransformScaleInterpolation,
                FollowingTargetOrientation,
                DocumentScaleEnabled,
                AdjustToScreenEnabled,
                BET_Unknown,
                Life,
                BinderRotationType,
                BinderVariety,
                PropStart,
                Prop1,
                Prop2,
                PropGoal
            };

            BinderVariety.Parsed.ExtraCommandGenerator = () => {
                return new AvfxBinderDataCommand( this );
            };

            Parameters = new( "Parameters", new() {
                new UiNodeGraphView( this ),
                StartToGlobalDirection,
                VfxScaleEnabled,
                VfxScaleBias,
                VfxScaleDepthOffset,
                VfxScaleInterpolation,
                TransformScale,
                TransformScaleDepthOffset,
                TransformScaleInterpolation,
                FollowingTargetOrientation,
                DocumentScaleEnabled,
                AdjustToScreenEnabled,
                BET_Unknown,
                Life,
                BinderRotationType
            } );

            PropSplitDisplay = new AvfxDisplaySplitView<AvfxBinderProperties>( "Properties", new() {
                PropStart,
                Prop1,
                Prop2,
                PropGoal
            } );
        }

        public override void ReadContents( BinaryReader reader, int size ) {
            Peek( reader, Parsed, size );
            var binderType = BinderVariety.GetValue();

            ReadNested( reader, ( BinaryReader _reader, string _name, int _size ) => {
                if( _name == "Data" ) {
                    SetData( binderType );
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

        public void SetData( BinderType type ) {
            Data = type switch {
                BinderType.Point => new AvfxBinderDataPoint(),
                BinderType.Linear => new AvfxBinderDataLinear(),
                BinderType.Spline => new AvfxBinderDataSpline(),
                BinderType.Camera => new AvfxBinderDataCamera(),
                _ => null,
            };
            Data?.SetAssigned( true );
        }

        public override void Draw() {
            using var _ = ImRaii.PushId( "Binder" );
            DrawRename();
            BinderVariety.Draw();
            ImGui.SetCursorPosY( ImGui.GetCursorPosY() + 5 );

            using var tabBar = ImRaii.TabBar( "Tabs", ImGuiTabBarFlags.NoCloseWithMiddleMouseButton );
            if( !tabBar ) return;

            DrawParameters();
            DrawData();
            DrawProperties();
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

        private void DrawProperties() {
            using var tabItem = ImRaii.TabItem( "Properties" );
            if( !tabItem ) return;

            PropSplitDisplay.Draw();
        }

        public override string GetDefaultText() => $"Binder {GetIdx()}({BinderVariety.GetValue()})";

        public override string GetWorkspaceId() => $"Bind{GetIdx()}";
    }
}
