using OtterGui.Raii;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using VfxEditor.Parsing;
using VfxEditor.Ui.Interfaces;

namespace VfxEditor.UldFormat.Timeline.Frames {
    public class UldFrame : IUiItem {
        public readonly ParsedUInt StartFrame = new( "Start Frame" );
        public readonly ParsedUInt EndFrame = new( "End Frame" );
        public readonly List<UldKeyGroup> KeyGroups = new();

        public readonly UldKeyGroupSplitView KeyGroupView;

        public UldFrame() {
            KeyGroupView = new( KeyGroups );
        }

        public UldFrame( BinaryReader reader ) : this() {
            var pos = reader.BaseStream.Position;

            StartFrame.Read( reader );
            EndFrame.Read( reader );
            var size = reader.ReadUInt32();
            var count = reader.ReadUInt32();
            for( var i = 0; i < count; i++ ) KeyGroups.Add( new( reader ) );

            reader.BaseStream.Position = pos + size;
        }

        public void Write( BinaryWriter writer ) {
            var pos = writer.BaseStream.Position;

            StartFrame.Write( writer );
            EndFrame.Write( writer );

            var savePos = writer.BaseStream.Position;
            writer.Write( ( uint )0 );

            writer.Write( ( uint )KeyGroups.Count );
            KeyGroups.ForEach( x => x.Write( writer ) );

            var finalPos = writer.BaseStream.Position;
            var size = finalPos - pos;
            writer.BaseStream.Position = savePos;
            writer.Write( ( uint )size );
            writer.BaseStream.Position = finalPos;
        }

        public void Draw() {
            StartFrame.Draw( CommandManager.Uld );
            EndFrame.Draw( CommandManager.Uld );

            using var child = ImRaii.Child( "Child", new Vector2( -1, -1 ), true );
            KeyGroupView.Draw();
        }
    }
}
