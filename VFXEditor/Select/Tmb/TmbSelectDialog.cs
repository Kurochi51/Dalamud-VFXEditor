using ImGuiNET;
using OtterGui.Raii;
using System.Collections.Generic;
using VfxEditor.FileManager;
using VfxEditor.Select.Tmb.Action;
using VfxEditor.Select.Tmb.Common;
using VfxEditor.Select.Tmb.Emote;
using VfxEditor.Select.Tmb.Npc;

namespace VfxEditor.Select.Tmb {
    public class TmbSelectDialog : SelectDialog {
        private readonly List<SelectTab> GameTabs;

        public TmbSelectDialog( string id, FileManagerWindow manager, bool isSourceDialog ) : base( id, "tmb", manager, isSourceDialog ) {
            GameTabs = new List<SelectTab>( new SelectTab[]{
                new ActionTab( this, "Action" ),
                new NonPlayerActionTab( this, "Non-Player Action" ),
                new EmoteTab( this, "Emote" ),
                new NpcTmbTab( this, "Npc" ),
                new CommonTab( this, "Common" )
            } );
        }

        public override void Play( string path ) {
            using var _ = ImRaii.PushId( "Spawn" );

            ImGui.SameLine();
            if( Plugin.ActorAnimationManager.CanReset ) {
                if( ImGui.Button( "Reset" ) ) Plugin.ActorAnimationManager.Reset();
            }
            else {
                if( ImGui.Button( "Play" ) ) Plugin.ActorAnimationManager.Apply( path );
            }
        }

        protected override List<SelectTab> GetTabs() => GameTabs;
    }
}
