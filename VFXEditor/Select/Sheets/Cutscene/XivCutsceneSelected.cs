using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace VfxEditor.Select.Rows {
    public class XivCutsceneSelected {
        public XivCutscene Cutscene;

        public HashSet<string> VfxPaths = new();

        public static readonly Regex Rx = new( @"\u0000([a-zA-Z0-9\/_]*?)\.avfx", RegexOptions.Compiled );

        public XivCutsceneSelected( XivCutscene cutscene, Lumina.Data.FileResource file ) {
            Cutscene = cutscene;

            var data = file.Data;
            var stringData = Encoding.UTF8.GetString( data );
            var matches = Rx.Matches( stringData );
            foreach( Match m in matches ) {
                VfxPaths.Add( m.Value.Trim( '\u0000' ) );
            }
        }
    }
}
