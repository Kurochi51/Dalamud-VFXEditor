using Lumina.Excel.GeneratedSheets;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace VfxEditor.Select.Shared.Npc {
    public struct NpcFilesStruct {
        public List<string> vfx;
        public List<string> tmb;
        public List<string> pap;

        public NpcFilesStruct() {
            vfx = new();
            tmb = new();
            pap = new();
        }
    }

    public struct BnpcStruct {
        public uint bnpcBase;
        public uint bnpcName;
    }

    public abstract class NpcTab : SelectTab<NpcRow, List<string>> {
        // Shared across multiple dialogs
        private static Dictionary<string, NpcFilesStruct> NpcFiles = new();

        public NpcTab( SelectDialog dialog, string name ) : base( dialog, name, "Shared-Npc" ) { }

        // ===== LOADING =====

        public override void LoadData() {
            // maps uint ids to npc names
            var nameToString = Plugin.DataManager.GetExcelSheet<BNpcName>()
                .Where( x => !string.IsNullOrEmpty( x.Singular ) )
                .ToDictionary(
                    x => x.RowId,
                    x => x.Singular.ToString()
                );

            var baseToName = JsonConvert.DeserializeObject<Dictionary<string, List<BnpcStruct>>>( File.ReadAllText( SelectUtils.BnpcPath ) )["bnpc"];

            var battleNpcSheet = Plugin.DataManager.GetExcelSheet<BNpcBase>();

            foreach( var entry in baseToName ) {
                if( !nameToString.TryGetValue( entry.bnpcName, out var name ) ) continue;
                var bnpcRow = battleNpcSheet.GetRow( entry.bnpcBase );
                if( bnpcRow == null || bnpcRow.ModelChara.Value == null || bnpcRow.ModelChara.Value.Model == 0 ) continue;
                if( bnpcRow.ModelChara.Value.Type != 2 && bnpcRow.ModelChara.Value.Type != 3 ) continue;
                Items.Add( new NpcRow( bnpcRow.ModelChara.Value, name ) );
            }

            NpcFiles = JsonConvert.DeserializeObject<Dictionary<string, NpcFilesStruct>>( File.ReadAllText( SelectUtils.NpcFilesPath ) );
        }

        public override void LoadSelection( NpcRow item, out List<string> loaded ) {
            var files = NpcFiles.TryGetValue( item.ModelString, out var paths ) ? paths : new NpcFilesStruct();
            FilesToSelected( files, out loaded );
        }

        protected abstract void FilesToSelected( NpcFilesStruct files, out List<string> selected );

        // ===== DRAWING ======

        protected override bool CheckMatch( NpcRow item, string searchInput ) =>
            SelectTabUtils.Matches( item.Name, searchInput ) || SelectTabUtils.Matches( item.ModelString, searchInput );

        protected override void DrawExtra() => SelectTabUtils.NpcThankYou();

        protected override string GetName( NpcRow item ) => item.Name;
    }
}
