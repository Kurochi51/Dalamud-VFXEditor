using Dalamud.Plugin.Ipc;
using System;
using System.Collections.Generic;

namespace VfxEditor.Interop {
    public class PenumbraIpc : IDisposable {
        public bool PenumbraEnabled { get; private set; }

        private readonly ICallGateSubscriber<(int Breaking, int Features)> ApiVersionsSubscriber;
        private readonly ICallGateSubscriber<string> GetModDirectorySubscriber;
        private readonly ICallGateSubscriber<IList<(string, string)>> GetModsSubscriber;

        private readonly ICallGateSubscriber<object> InitializedSubscriber;
        private readonly ICallGateSubscriber<object> DisposedSubscriber;

        public PenumbraIpc() {
            ApiVersionsSubscriber = Plugin.PluginInterface.GetIpcSubscriber<(int, int)>( "Penumbra.ApiVersions" );
            InitializedSubscriber = Plugin.PluginInterface.GetIpcSubscriber<object>( "Penumbra.Initialized" );
            DisposedSubscriber = Plugin.PluginInterface.GetIpcSubscriber<object>( "Penumbra.Disposed" );

            GetModDirectorySubscriber = Plugin.PluginInterface.GetIpcSubscriber<string>( "Penumbra.GetModDirectory" );
            GetModsSubscriber = Plugin.PluginInterface.GetIpcSubscriber<IList<(string, string)>>( "Penumbra.GetMods" );

            InitializedSubscriber.Subscribe( EnablePenumbra );
            DisposedSubscriber.Subscribe( DisablePenumbra );

            if( !Plugin.PluginInterface.PluginNames.Contains( "Penumbra" ) ) return;
            try {
                ApiVersionsSubscriber.InvokeFunc();
            }
            catch( Exception ) {
                return;
            }

            PenumbraEnabled = true;
        }

        public List<string> GetMods() {
            var ret = new List<string>();
            try {
                var mods = GetModsSubscriber.InvokeFunc();
                foreach( var m in mods ) {
                    var (modPath, nodName) = m;
                    ret.Add( modPath );
                }
            }
            catch( Exception ) { }
            return ret;
        }

        public string GetModDirectory() {
            try {
                return GetModDirectorySubscriber.InvokeFunc();

            }
            catch( Exception ) {
                return null;
            }
        }

        private void EnablePenumbra() {
            PenumbraEnabled = true;
        }

        private void DisablePenumbra() {
            PenumbraEnabled = false;
        }

        public void Dispose() {
            InitializedSubscriber.Unsubscribe( EnablePenumbra );
            DisposedSubscriber.Unsubscribe( DisablePenumbra );
        }
    }
}
