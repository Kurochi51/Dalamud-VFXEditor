using Dalamud.Plugin;
using Dalamud.Logging;
using Lumina.Excel.GeneratedSheets;
using System;
using System.Collections.Generic;
using System.Linq;
using VFXSelect.Data.Rows;

namespace VFXSelect.Data.Sheets {
    public class GimmickSheetLoader : SheetLoader<XivGimmick, XivGimmickSelected> {
        public GimmickSheetLoader( SheetManager manager, DalamudPluginInterface pluginInterface ) : base( manager, pluginInterface ) {
        }

        public override void OnLoad() {
            var territories = PluginInterface.Data.GetExcelSheet<TerritoryType>().Where( x => !string.IsNullOrEmpty( x.Name ) ).ToList();
            var suffixToName = new Dictionary<string, string>();
            foreach( var zone in territories ) {
                suffixToName[zone.Name.ToString()] = zone.PlaceName.Value?.Name.ToString();
            }

            var sheet = PluginInterface.Data.GetExcelSheet<ActionTimeline>().Where( x => x.Key.ToString().Contains( "gimmick" ) );
            foreach( var item in sheet ) {
                var i = new XivGimmick( item, suffixToName );
                Items.Add( i );
            }
        }

        public override bool SelectItem( XivGimmick item, out XivGimmickSelected selectedItem ) {
            selectedItem = null;
            var tmbPath = item.GetTmbPath();
            var result = PluginInterface.Data.FileExists( tmbPath );
            if( result ) {
                try {
                    var file = PluginInterface.Data.GetFile( tmbPath );
                    selectedItem = new XivGimmickSelected( file, item );
                }
                catch( Exception e ) {
                    PluginLog.Error( "Error reading TMB file " + tmbPath, e );
                    return false;
                }
            }
            else {
                PluginLog.Error( tmbPath + " does not exist" );
            }
            return result;
        }
    }
}
