using Dalamud.Configuration;
using Dalamud.Logging;
using ImGuiFileDialog;
using ImGuiNET;
using OtterGui.Raii;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using VfxEditor.NodeLibrary;
using VfxEditor.Select;
using VfxEditor.Ui;

namespace VfxEditor {
    [Serializable]
    public unsafe class ManagerConfiguration {
        public List<SelectResult> RecentItems = new();
        public List<SelectResult> Favorites = new();
        public bool UseCustomWindowColor = false;
        public Vector4 TitleBg = *ImGui.GetStyleColorVec4( ImGuiCol.TitleBg );
        public Vector4 TitleBgActive = *ImGui.GetStyleColorVec4( ImGuiCol.TitleBgActive );
        public Vector4 TitleBgCollapsed = *ImGui.GetStyleColorVec4( ImGuiCol.TitleBgCollapsed );
    }

    [Serializable]
    public class Configuration : GenericDialog, IPluginConfiguration {
        public int Version { get; set; } = 0;
        public bool IsEnabled { get; set; } = true;

        public bool LogAllFiles = false;
        public bool LogDebug = false;
        public bool LogVfxDebug = false;

        public bool HideWithUI = true;
        public int SaveRecentLimit = 10;
        public bool OverlayLimit = true;
        public float OverlayRemoveDelay = 1;
        public string WriteLocation = Path.Combine( new[] {
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "XIVLauncher",
            "pluginConfigs",
            "VFXEditor",
        } );

        public bool UpdateWriteLocation = true;

        // ===== [ OBSOLETE ] =======
        public List<SelectResult> RecentSelects = new();
        public List<SelectResult> RecentSelectsTMB = new();
        public List<SelectResult> RecentSelectsPAP = new();
        public List<SelectResult> RecentSelectsScd = new();
        public List<SelectResult> FavoriteVfx = new();
        public List<SelectResult> FavoriteTmb = new();
        public List<SelectResult> FavoritePap = new();
        public List<SelectResult> FavoriteScd = new();
        // ===========================

        public Dictionary<string, ManagerConfiguration> ManagerConfigs = new();

        public bool FilepickerImagePreview = true;

        public bool AutosaveEnabled = false;
        public int AutosaveSeconds = 300;

        public bool BlockGameInputsWhenFocused = false;

        public KeybindConfiguration SaveKeybind = new();
        public KeybindConfiguration SaveAsKeybind = new();
        public KeybindConfiguration OpenKeybind = new();
        public KeybindConfiguration UpdateKeybind = new();
        public KeybindConfiguration CopyKeybind = new();
        public KeybindConfiguration PasteKeybind = new();
        public KeybindConfiguration UndoKeybind = new();
        public KeybindConfiguration RedoKeybind = new();
        public KeybindConfiguration SpawnOnSelfKeybind = new();
        public KeybindConfiguration SpawnOnGroundKeybind = new();
        public KeybindConfiguration SpawnOnTargetKeybind = new();

        public List<AvfxNodeLibraryProps> VFXNodeLibraryItems = new();

        public int MaxUndoSize = 10;
        public bool DoubleClickNavigate = true;

        public bool LoopMusic = true;
        public bool LoopSoundEffects = false;
        public float ScdVolume = 1f;
        public bool SimulateScdLoop = false;

        public bool UseDegreesForAngles = false;

        public Vector4 CurveEditorLineColor = new( 0, 0.1f, 1, 1 );
        public Vector4 CurveEditorPointColor = new( 1 );
        public Vector4 CurveEditorSelectedColor = new( 1.000f, 0.884f, 0.561f, 1f );
        public Vector4 CurveEditorPrimarySelectedColor = new( 0.984375f, 0.7265625f, 0.01176470f, 1.0f );

        public int CurveEditorLineWidth = 2;
        public int CurveEditorColorRingSize = 3;
        public int CurveEditorGrabbingDistance = 25;
        public int CurveEditorPointSize = 7;
        public int CurveEditorSelectedSize = 10;
        public int CurveEditorPrimarySelectedSize = 12;

        public Vector4 TimelineSelectedColor = new( 1f, 0.532f, 0f, 1f );
        public Vector4 TimelineBarColor = new( 0.44f, 0.457f, 0.492f, 1f );

        [NonSerialized]
        public bool WriteLocationError = false;

        public Configuration() : base( "Settings", false, 300, 200 ) { }

        public void Setup() {
            Plugin.PluginInterface.UiBuilder.DisableUserUiHide = !HideWithUI;
            FileDialogManager.ImagePreview = FilepickerImagePreview;

            // Move old configurations over to new
            ProcessOldManagerConfigs( RecentSelects, FavoriteVfx, "Vfx" );
            ProcessOldManagerConfigs( RecentSelectsTMB, FavoriteTmb, "Tmb" );
            ProcessOldManagerConfigs( RecentSelectsPAP, FavoritePap, "Pap" );
            ProcessOldManagerConfigs( RecentSelectsScd, FavoriteScd, "Scd" );

            try { Directory.CreateDirectory( WriteLocation ); }
            catch( Exception ) { WriteLocationError = true; }

            PluginLog.Log( "Write location: " + WriteLocation );
        }

        private void ProcessOldManagerConfigs( List<SelectResult> recent, List<SelectResult> favorites, string key ) {
            if( recent.Count == 0 && favorites.Count == 0 ) return;

            if( !ManagerConfigs.ContainsKey( key ) ) ManagerConfigs[key] = new();
            ManagerConfigs[key].RecentItems.AddRange( recent );
            ManagerConfigs[key].Favorites.AddRange( favorites );

            recent.Clear();
            favorites.Clear();
        }

        public void AddRecent( List<SelectResult> recentList, SelectResult result ) {
            if( result == null ) return;
            recentList.RemoveAll( x => result.CompareTo( x ) );

            recentList.Add( result );
            if( recentList.Count > SaveRecentLimit ) recentList.RemoveRange( 0, recentList.Count - SaveRecentLimit );
            Save();
        }

        public void Save() {
            Plugin.PluginInterface.SavePluginConfig( this );
            Plugin.PluginInterface.UiBuilder.DisableUserUiHide = !HideWithUI;
        }

        public override void DrawBody() {
            using var _ = ImRaii.PushId( "##Settings" );

            using var tabBar = ImRaii.TabBar( "Tabs" );
            if( !tabBar ) return;

            if( ImGui.BeginTabItem( "Configuration" ) ) {
                DrawConfiguration();
                ImGui.EndTabItem();
            }
            if( ImGui.BeginTabItem( "Keybinds" ) ) {
                DrawKeybinds();
                ImGui.EndTabItem();
            }
            if( ImGui.BeginTabItem( "Curve Editor" ) ) {
                DrawCurveEditor();
                ImGui.EndTabItem();
            }
            if( ImGui.BeginTabItem( "Timeline Editor" ) ) {
                DrawTimelineEditor();
                ImGui.EndTabItem();
            }
            if( ImGui.BeginTabItem( "File Editors" ) ) {
                DrawEditorSpecific();
                ImGui.EndTabItem();
            }
        }

        private void DrawConfiguration() {
            using var child = ImRaii.Child( "Config" );

            ImGui.SetCursorPosY( ImGui.GetCursorPosY() + 5 );
            using var indent = ImRaii.PushIndent( 5f );

            ImGui.TextDisabled( "Changes to the temp file location may require a restart to take effect" );
            if( ImGui.InputText( "Temp file location", ref WriteLocation, 255 ) ) Save();
            if( ImGui.Checkbox( "Refresh write location each update", ref UpdateWriteLocation ) ) Save();
            if( ImGui.Checkbox( "Log all files", ref LogAllFiles ) ) Save();
            if( ImGui.Checkbox( "Log debug information", ref LogDebug ) ) Save();
            if( ImGui.Checkbox( "Log Vfx debug information", ref LogVfxDebug ) ) Save();

            if( ImGui.Checkbox( "Autosave workspace", ref AutosaveEnabled ) ) Save();
            ImGui.Indent();
            if( !AutosaveEnabled ) {
                var style = ImGui.GetStyle();
                ImGui.PushStyleVar( ImGuiStyleVar.Alpha, style.Alpha * 0.5f );
            }
            ImGui.SetNextItemWidth( 120 );
            if( ImGui.InputInt( "Autosave time (seconds)", ref AutosaveSeconds ) ) Save();
            if( !AutosaveEnabled ) ImGui.PopStyleVar();
            ImGui.Unindent();

            if( ImGui.Checkbox( "Hide with UI", ref HideWithUI ) ) Save();

            if( ImGui.Checkbox( "File picker image preview", ref FilepickerImagePreview ) ) {
                FileDialogManager.ImagePreview = FilepickerImagePreview;
                Save();
            }

            ImGui.SetNextItemWidth( 135 );
            if( ImGui.InputInt( "Recent file limit", ref SaveRecentLimit ) ) {
                SaveRecentLimit = Math.Max( SaveRecentLimit, 0 );
                Save();
            }

            ImGui.SetNextItemWidth( 135 );
            if( ImGui.InputFloat( "Live overlay remove delay time", ref OverlayRemoveDelay ) ) Save();
            if( ImGui.Checkbox( "Live overlay limit by distance", ref OverlayLimit ) ) Save();

            ImGui.SetNextItemWidth( 135 );
            if( ImGui.InputInt( "Undo history size", ref MaxUndoSize ) ) Save();

            if( ImGui.Checkbox( "Double-click to navigate to items", ref DoubleClickNavigate ) ) Save();
        }

        private void DrawKeybinds() {
            if( ImGui.Checkbox( "Block game inputs when VFXEditor is focused##Settings", ref BlockGameInputsWhenFocused ) ) Save();

            using var child = ImRaii.Child( "Keybinds", new Vector2( -1 ), true );

            if( SaveKeybind.Draw( "Save" ) ) Save();
            if( SaveAsKeybind.Draw( "Save as" ) ) Save();
            if( OpenKeybind.Draw( "Open" ) ) Save();
            if( CopyKeybind.Draw( "Copy" ) ) Save();
            if( PasteKeybind.Draw( "Paste" ) ) Save();
            if( UndoKeybind.Draw( "Undo" ) ) Save();
            if( RedoKeybind.Draw( "Redo " ) ) Save();
            if( UpdateKeybind.Draw( "Update" ) ) Save();
            if( SpawnOnSelfKeybind.Draw( "Spawn on self (Vfx only)" ) ) Save();
            if( SpawnOnGroundKeybind.Draw( "Spawn on ground (Vfx only)" ) ) Save();
            if( SpawnOnTargetKeybind.Draw( "Spawn on target (Vfx only)" ) ) Save();
        }

        private void DrawCurveEditor() {
            using var child = ImRaii.Child( "CurveEditor" );

            ImGui.SetCursorPosY( ImGui.GetCursorPosY() + 5 );
            using var indent = ImRaii.PushIndent( 5f );

            if( ImGui.ColorEdit4( "Line color", ref CurveEditorLineColor ) ) Save();
            if( ImGui.ColorEdit4( "Point color", ref CurveEditorPointColor ) ) Save();
            if( ImGui.ColorEdit4( "Primary selected color", ref CurveEditorPrimarySelectedColor ) ) Save();
            if( ImGui.ColorEdit4( "Selected color", ref CurveEditorSelectedColor ) ) Save();

            if( ImGui.InputInt( "Line width", ref CurveEditorLineWidth ) ) Save();
            if( ImGui.InputInt( "Color ring width", ref CurveEditorColorRingSize ) ) Save();
            if( ImGui.InputInt( "Point size", ref CurveEditorPointSize ) ) Save();
            if( ImGui.InputInt( "Primary selected size", ref CurveEditorPrimarySelectedSize ) ) Save();
            if( ImGui.InputInt( "Selected size", ref CurveEditorSelectedSize ) ) Save();
            if( ImGui.InputInt( "Grab distance", ref CurveEditorGrabbingDistance ) ) Save();
        }

        private void DrawTimelineEditor() {
            using var child = ImRaii.Child( "TimelineEditor" );

            ImGui.SetCursorPosY( ImGui.GetCursorPosY() + 5 );
            using var indent = ImRaii.PushIndent( 5f );

            if( ImGui.ColorEdit4( $"Selected color", ref TimelineSelectedColor ) ) Save();
            if( ImGui.ColorEdit4( $"Bar color", ref TimelineBarColor ) ) Save();
        }

        private void DrawEditorSpecific() {
            using var child = ImRaii.Child( "EditorSpecific" );
            ImGui.SetCursorPosY( ImGui.GetCursorPosY() + 5 );

            foreach( var config in ManagerConfigs ) {
                using var _ = ImRaii.PushId( config.Key );

                if( ImGui.CollapsingHeader( config.Key ) ) {
                    using var indent = ImRaii.PushIndent( 5f );

                    ImGui.Checkbox( "Use custom window color", ref config.Value.UseCustomWindowColor );
                    if( config.Value.UseCustomWindowColor ) {
                        if( ImGui.ColorEdit4( "Background", ref config.Value.TitleBg ) ) Save();
                        if( ImGui.ColorEdit4( "Active", ref config.Value.TitleBgActive ) ) Save();
                        if( ImGui.ColorEdit4( "Collapsed", ref config.Value.TitleBgCollapsed ) ) Save();
                    }
                }
            }
        }
    }
}