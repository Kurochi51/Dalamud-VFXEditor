using ImGuiNET;
using VfxEditor.Select.Rows;

namespace VfxEditor.Select.VfxSelect {
    public class VfxMountSelect : VfxSelectTab<XivMount, XivMountSelected> {
        private ImGuiScene.TextureWrap Icon;

        public VfxMountSelect( string parentId, string tabId, VfxSelectDialog dialog ) :
            base( parentId, tabId, SheetManager.Mounts, dialog ) {
        }

        protected override void OnSelect() {
            LoadIcon( Selected.Icon, ref Icon );
        }

        protected override bool CheckMatch( XivMount item, string searchInput ) {
            return Matches( item.Name, searchInput );
        }

        protected override void DrawSelected( XivMountSelected loadedItem ) {
            if( loadedItem == null ) { return; }
            ImGui.Text( loadedItem.Mount.Name );
            ImGui.SetCursorPosY( ImGui.GetCursorPosY() + 5 );

            DrawIcon( Icon );

            ImGui.Text( "Variant: " + loadedItem.Mount.Variant );
            ImGui.Text( "IMC Count: " + loadedItem.Count );
            ImGui.Text( "VFX Id: " + loadedItem.VfxId );

            ImGui.Text( "IMC Path: " );
            ImGui.SameLine();
            DisplayPath( loadedItem.ImcPath );

            DrawPath( "VFX Path", loadedItem.GetVFXPath(), Id, Dialog, SelectResultType.GameNpc, "NPC", loadedItem.Mount.Name, play: true );
        }

        protected override string UniqueRowTitle( XivMount item ) {
            return item.Name + Id;
        }
    }
}