using Dalamud.Interface;
using ImGuiNET;
using OtterGui.Raii;
using System.Collections.Generic;
using VfxEditor.Data;
using VfxEditor.Ui.Interfaces;
using VfxEditor.Ui.Nodes;
using VfxEditor.Utils;

namespace VfxEditor.AvfxFormat {
    public abstract class UiNodeSelect : IUiItem {
        public readonly AvfxNode Node;
        protected bool OnChangeLinked = false;

        public UiNodeSelect( AvfxNode node ) {
            Node = node;
        }

        public abstract void UpdateLiteral();

        public abstract void Initialize();

        public abstract void LinkOnIndexChange();

        public abstract void UnlinkOnIndexChange();

        public abstract void Draw();

        // For when something happens to the selector

        public abstract void Enable();

        public abstract void Disable();

        // For when something happens to the selected node

        public abstract List<int> GetSelectedIdx( AvfxNode node );

        public abstract void EnableNode( AvfxNode node, int idx );

        public abstract void DisableNode( AvfxNode node );

        public void UnlinkParentChild( AvfxNode node ) {
            if( node == null ) return;

            Node.ChildNodes.Remove( node );
            node.Parents.Remove( this );
            OutdatedGraph( node );
        }

        public void LinkParentChild( AvfxNode node ) {
            if( node == null ) return;

            Node.ChildNodes.Add( node );
            node.Parents.Add( this );
            OutdatedGraph( node );
        }

        private void OutdatedGraph( AvfxNode node ) {
            node.Graph?.NowOutdated();
            node.ChildNodes.ForEach( OutdatedGraph ); // children are also outdated
        }
    }

    public class UiNodeSelect<T> : UiNodeSelect where T : AvfxNode {
        public T Selected = null;
        public readonly AvfxInt Literal;
        public readonly NodeGroup<T> Group; // the group being selected from
        public readonly string Name;

        private bool Enabled = true;

        public UiNodeSelect( AvfxNode node, string name, NodeGroup<T> group, AvfxInt literal ) : base( node ) {
            Name = name;
            Group = group;
            Literal = literal;
            if( Group.ImportInProgress ) group.OnImportFinish += ImportFinished;
            else {
                LinkOnIndexChange();
                if( Group.IsInitialized ) Initialize(); // already good to go
                else Group.OnInit += Initialize;
            }
            node.Selectors.Add( this );
        }

        public override void UpdateLiteral() => Literal.SetValue( Selected != null ? Selected.GetIdx() : -1 ); // an item was removed from the group, for example

        public override void Initialize() {
            var value = Literal.GetValue();
            if( value >= 0 && value < Group.Items.Count ) {
                Selected = Group.Items[value];
                LinkParentChild( Selected );
            }
        }

        public void ImportFinished() {
            if( Literal.GetValue() >= 0 ) Literal.SetValue( Literal.GetValue() + Group.PreImportSize );
            LinkOnIndexChange();
            Initialize();
        }

        public void Select( T item ) {
            if( item == null ) SelectNone();
            else SelectItem( item );
        }

        private void SelectNone() {
            if( Selected == null ) return;
            UnlinkParentChild( Selected );
            Selected = null;
            UpdateLiteral();
        }

        private void SelectItem( T item ) {
            if( Selected == item ) return;
            UnlinkParentChild( Selected );
            LinkParentChild( item );
            Selected = item;
            UpdateLiteral();
        }

        public override void LinkOnIndexChange() {
            if( OnChangeLinked ) return;
            OnChangeLinked = true;
            Group.OnChange += UpdateLiteral;
        }

        public override void UnlinkOnIndexChange() {
            if( !OnChangeLinked ) return;
            OnChangeLinked = false;
            Group.OnChange -= UpdateLiteral;
        }

        // For when something happens to the selector

        public override void Enable() {
            if( Enabled ) return;
            Enabled = true;
            LinkOnIndexChange();

            if( Selected != null ) LinkParentChild( Selected );
        }

        public override void Disable() {
            if( !Enabled ) return;
            Enabled = false;
            UnlinkOnIndexChange();
            if( Selected != null ) UnlinkParentChild( Selected );
        }

        // For when something happens to the selected node

        public override List<int> GetSelectedIdx( AvfxNode node ) => null; // not used for this one, since there is only every 1 selector

        public override void EnableNode( AvfxNode node, int _ ) {
            Selected = ( T )node;
            UpdateLiteral();
        }

        public override void DisableNode( AvfxNode node ) {
            Selected = null;
            UpdateLiteral();
        }

        public override void Draw() {
            using var _ = ImRaii.PushId( $"Node{Name}" );

            // Unassigned
            AvfxBase.AssignedCopyPaste( Literal, Name );
            if( AvfxBase.DrawAddButton( Literal, Name ) ) return;

            // Copy/Paste
            var copy = CopyManager.Avfx;
            if( copy.IsCopying ) copy.Ints[Name] = Literal.GetValue();
            if( copy.IsPasting && copy.Ints.TryGetValue( Name, out var val ) ) {
                var newSelected = ( val == -1 || val >= Group.Items.Count ) ? null : Group.Items[val];
                copy.PasteCommand.Add( new UiNodeSelectCommand<T>( this, newSelected ) );
            }

            // Draw
            var inputSize = UiUtils.GetOffsetInputSize( FontAwesomeIcon.Share );

            ImGui.SetNextItemWidth( inputSize );
            DrawCombo();

            AvfxBase.DrawRemoveContextMenu( Literal, Name );

            // Draw go button
            ImGui.SameLine( inputSize + ImGui.GetStyle().ItemInnerSpacing.X );
            using( var font = ImRaii.PushFont( UiBuilder.IconFont ) ) {
                using var dimmed = ImRaii.PushStyle( ImGuiStyleVar.Alpha, 0.5f, Selected == null );
                if( ImGui.Button( FontAwesomeIcon.Share.ToIconString() ) ) Plugin.AvfxManager.CurrentFile.SelectItem( Selected );
            }

            UiUtils.Tooltip( "Navigate to selected node" );

            ImGui.SameLine();
            ImGui.Text( Name );
        }

        private void DrawCombo() {
            using var combo = ImRaii.Combo( "##MainCombo", GetText() );
            if( !combo ) return;

            if( ImGui.Selectable( "[NONE]", Selected == null ) ) CommandManager.Avfx.Add( new UiNodeSelectCommand<T>( this, null ) ); // "None" selector
            foreach( var item in Group.Items ) {
                if( ImGui.Selectable( item.GetText(), Selected == item ) ) CommandManager.Avfx.Add( new UiNodeSelectCommand<T>( this, item ) );
                if( ImGui.IsItemHovered() ) item.ShowTooltip();
            }
        }

        public string GetText() => Selected == null ? "[NONE]" : Selected.GetText();
    }
}
