using System.IO;
using VfxEditor.Parsing;

namespace VfxEditor.TmbFormat {
    public class TmtrUnknownData {
        public readonly bool PapEmbedded;

        private readonly ParsedInt Unk1 = new( "Unknown 1" );
        private readonly ParsedShort Unk2 = new( "Unknown 2" );
        private readonly ParsedShort Unk3 = new( "Unknown 3" );
        private readonly ParsedInt Unk4 = new( "Unknown 4" );

        public TmtrUnknownData( bool papEmbedded ) {
            PapEmbedded = papEmbedded;
        }

        public TmtrUnknownData( BinaryReader reader, bool papEmbedded ) : this( papEmbedded ) {
            Unk1.Read( reader );
            Unk2.Read( reader );
            Unk3.Read( reader );
            Unk4.Read( reader );
        }

        public void Write( BinaryWriter writer ) {
            Unk1.Write( writer );
            Unk2.Write( writer );
            Unk3.Write( writer );
            Unk4.Write( writer );
        }

        public void Draw() {
            Unk1.Draw( PapEmbedded ? CommandManager.Pap : CommandManager.Tmb );
            Unk2.Draw( PapEmbedded ? CommandManager.Pap : CommandManager.Tmb );
            Unk3.Draw( PapEmbedded ? CommandManager.Pap : CommandManager.Tmb );
        }
    }
}
