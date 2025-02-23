using System.IO;
using VfxEditor.Parsing;

namespace VfxEditor.AvfxFormat {
    public class AvfxBool : AvfxDrawable {
        public readonly ParsedNullableBool Parsed;

        public AvfxBool( string name, string avfxName, bool defaultValue, int size = 4 ) : this( name, avfxName, size ) {
            SetValue( defaultValue );
        }

        public AvfxBool( string name, string avfxName, int size = 4 ) : base( avfxName ) {
            Parsed = new( name, size );
        }

        public bool? GetValue() => Parsed.Value;

        public void SetValue( bool? value ) {
            SetAssigned( true );
            Parsed.Value = value;
        }

        public override void ReadContents( BinaryReader reader, int size ) => Parsed.Read( reader, size );

        protected override void RecurseChildrenAssigned( bool assigned ) { }

        protected override void WriteContents( BinaryWriter writer ) => Parsed.Write( writer );

        public override void Draw() {
            // Unassigned
            AssignedCopyPaste( this, Parsed.Name );
            if( DrawAddButton( this, Parsed.Name ) ) return;

            Parsed.Draw( CommandManager.Avfx );

            DrawRemoveContextMenu( this, Parsed.Name );
        }
    }
}
