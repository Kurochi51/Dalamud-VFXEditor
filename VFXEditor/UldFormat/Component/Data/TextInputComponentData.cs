using VfxEditor.Parsing;
using VfxEditor.Parsing.Color;

namespace VfxEditor.UldFormat.Component.Data {
    public class TextInputComponentData : UldGenericData {
        public TextInputComponentData() {
            AddUnknown( 16, "Unknown Node Id" );

            Parsed.AddRange( new ParsedBase[] {
                new ParsedSheetColor( "Color" ),
                new ParsedSheetColor( "IME Color" ),
            } );
        }
    }
}
