namespace VfxEditor.Select.Shared.Npc {
    public class NpcRow {
        public enum NpcType {
            Demihuman = 2,
            Monster = 3
        }

        public string Name;
        public readonly int RowId;

        public readonly int Key;
        public readonly int ModelId;
        public readonly int BaseId;
        public readonly int Variant;
        public readonly NpcType Type;

        public readonly string ModelString;
        public readonly string BaseIdString;
        public readonly string RootPath;

        public NpcRow( Lumina.Excel.GeneratedSheets.ModelChara npc ) : this( npc, null ) { }

        public NpcRow( Lumina.Excel.GeneratedSheets.ModelChara npc, string name ) {
            Name = name;
            RowId = ( int )npc.RowId;
            ModelId = npc.Model;
            BaseId = npc.Base;
            Variant = npc.Variant;
            Type = ( NpcType )npc.Type;
            BaseIdString = BaseId.ToString().PadLeft( 4, '0' );

            if( Type == NpcType.Monster ) {
                ModelString = "m" + ModelId.ToString().PadLeft( 4, '0' );
                RootPath = "chara/monster/" + ModelString + "/obj/body/b" + BaseIdString + "/";
            }
            else { // demihuman
                ModelString = "d" + ModelId.ToString().PadLeft( 4, '0' );
                RootPath = "chara/demihuman/" + ModelString + "/obj/equipment/e" + BaseIdString + "/";
            }
        }

        // chara/monster/m0519/skeleton/base/b0001/eid_m0519b0001.eid
        // chara/demihuman/d0002/skeleton/base/b0001/eid_d0002b0001.eid
        public string GetEidPath() =>
            "chara/" + ( Type == NpcType.Monster ? "monster/" : "demihuman/" ) + ModelString +
            "/skeleton/base/b0001/eid_" + ModelString + "b0001.eid";

        public string GetImcPath() => RootPath + ( Type == NpcType.Monster ? "b" : "e" ) + BaseIdString + ".imc";

        // chara/monster/m0624/obj/body/b0001/vfx/eff/vm0001.avfx
        // chara/demihuman/d1003/obj/equipment/e0006/vfx/eff/ve0006.avfx
        public string GetVfxPath( int vfxId ) => RootPath + "vfx/eff/v" + ( Type == NpcType.Monster ? "m" : "e" ) + vfxId.ToString().PadLeft( 4, '0' ) + ".avfx";
    }
}
