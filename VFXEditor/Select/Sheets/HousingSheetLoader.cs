using Dalamud.Plugin;
using Dalamud.Logging;
using Lumina.Excel.GeneratedSheets;
using System;
using System.Linq;
using VFXSelect.Data.Rows;

namespace VFXSelect.Data.Sheets {
    public class HousingSheetLoader : SheetLoader<XivHousing, XivHousingSelected> {
        public HousingSheetLoader( SheetManager manager, DalamudPluginInterface pluginInterface ) : base( manager, pluginInterface ) {
        }

        public override void OnLoad() {
            var sheet = PluginInterface.Data.GetExcelSheet<HousingFurniture>().Where( x => x.ModelKey > 0 );
            foreach( var item in sheet ) {
                Items.Add( new XivHousing( item ) );
            }

            var sheet2 = PluginInterface.Data.GetExcelSheet<HousingYardObject>().Where( x => x.ModelKey > 0 );
            foreach( var item in sheet2 ) {
                Items.Add( new XivHousing( item ) );
            }
        }

        public override bool SelectItem( XivHousing item, out XivHousingSelected selectedItem ) {
            selectedItem = null;
            var sgbPath = item.GetSbgPath();
            var result = PluginInterface.Data.FileExists( sgbPath );
            if( result ) {
                try {
                    var file = PluginInterface.Data.GetFile( sgbPath );
                    selectedItem = new XivHousingSelected( item, file );
                }
                catch( Exception e ) {
                    PluginLog.Error( "Error loading SGB file " + sgbPath, e );
                    return false;
                }
            }
            return result;
        }
    }
}
