using ImGuiNET;
using System.Linq;
using VfxEditor.Select.Shared;

namespace VfxEditor.Select.Vfx.Emote {
    public class EmoteTab : SelectTab<EmoteRow, ParseAvfx> {
        public EmoteTab( SelectDialog dialog, string name ) : base( dialog, name, "Vfx-Emote" ) { }

        // ===== LOADING =====

        public override void LoadData() {
            var sheet = Plugin.DataManager.GetExcelSheet<Lumina.Excel.GeneratedSheets.Emote>().Where( x => !string.IsNullOrEmpty( x.Name ) );

            foreach( var item in sheet ) {
                var emoteItem = new EmoteRow( item );
                if( emoteItem.PapFiles.Count > 0 ) Items.Add( emoteItem );
            }
        }

        public override void LoadSelection( EmoteRow item, out ParseAvfx loaded ) => ParseAvfx.ReadFile( item.PapFiles, out loaded );

        // ===== DRAWING ======

        protected override void OnSelect() => LoadIcon( Selected.Icon );

        protected override void DrawSelected() {
            SelectTabUtils.DrawIcon( Icon );
            ImGui.TextDisabled( Selected.Command );

            Dialog.DrawPaths( "VFX", Loaded.VfxPaths, SelectResultType.GameEmote, Selected.Name, true );
        }

        protected override string GetName( EmoteRow item ) => item.Name;

        protected override bool CheckMatch( EmoteRow item, string searchInput ) => base.CheckMatch( item, SearchInput ) || SelectTabUtils.Matches( item.Command, searchInput );
    }
}
