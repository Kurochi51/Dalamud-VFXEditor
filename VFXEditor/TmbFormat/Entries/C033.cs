using System.Collections.Generic;
using VfxEditor.Parsing;
using VfxEditor.TmbFormat.Utils;

namespace VfxEditor.TmbFormat.Entries {
    public class C033 : TmbEntry {
        public const string MAGIC = "C033";
        public const string DISPLAY_NAME = "Crafting Delay (C033)";
        public override string DisplayName => DISPLAY_NAME;
        public override string Magic => MAGIC;

        public override int Size => 0x14;
        public override int ExtraSize => 0;

        private readonly ParsedBool ParsedBool = new( "Enabled" );
        private readonly ParsedInt Unk2 = new( "Unknown 2" );

        public C033( bool papEmbedded ) : base( papEmbedded ) { }

        public C033( TmbReader reader, bool papEmbedded ) : base( reader, papEmbedded ) {
            ReadHeader( reader );
            ReadParsed( reader );
        }

        protected override List<ParsedBase> GetParsed() => new() {
            ParsedBool,
            Unk2
        };
    }
}
