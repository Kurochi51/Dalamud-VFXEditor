using ImGuiNET;
using System.IO;
using VfxEditor.Parsing;
using VfxEditor.TmbFormat.Utils;
using VfxEditor.Utils;

namespace VfxEditor.TmbFormat {
    public class Tmpp : TmbItem {
        public override string Magic => "TMPP";
        public override int Size => 0x0C;
        public override int ExtraSize => 0;

        public bool IsAssigned => Assigned.Value == true;
        private readonly ParsedByteBool Assigned = new( "Use Face Library", defaultValue: false );
        private readonly TmbOffsetString Path = new( "Face Library Path" );

        public Tmpp( TmbReader reader, bool papEmbedded ) : base( reader, papEmbedded ) {
            var savePos = reader.Reader.BaseStream.Position;
            var magic = reader.ReadString( 4 );// TMAL or TMPP

            if( magic == "TMPP" ) { // TMPP
                Assigned.Value = true;
                reader.ReadInt32(); // 0x0C
                Path.Read( reader );
            }
            else { // TMAL, reset
                reader.Reader.BaseStream.Seek( savePos, SeekOrigin.Begin );
            }
        }

        public override void Write( TmbWriter writer ) {
            WriteHeader( writer );
            Path.Write( writer );
        }

        public void Draw() {
            Assigned.Draw( Command );
            ImGui.SameLine();
            UiUtils.WikiButton( "https://github.com/0ceal0t/Dalamud-VFXEditor/wiki/Using-Facial-Expressions" );

            if( IsAssigned ) Path.Draw( Command );
        }
    }
}
