using Dalamud.Hooking;
using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace VfxEditor.Interop {
    public unsafe partial class ResourceLoader : IDisposable {
        public bool IsEnabled { get; private set; } = false;

        public void Init() {
            var scanner = Plugin.SigScanner;

            var readFileAddress = scanner.ScanText( Constants.ReadFileSig );

            ReadSqpackHook = Hook<ReadSqpackPrototype>.FromAddress( scanner.ScanText( Constants.ReadSqpackSig ), ReadSqpackHandler );
            GetResourceSyncHook = Hook<GetResourceSyncPrototype>.FromAddress( scanner.ScanText( Constants.GetResourceSyncSig ), GetResourceSyncHandler );
            GetResourceAsyncHook = Hook<GetResourceAsyncPrototype>.FromAddress( scanner.ScanText( Constants.GetResourceAsyncSig ), GetResourceAsyncHandler );

            ReadFile = Marshal.GetDelegateForFunctionPointer<ReadFilePrototype>( readFileAddress );

            var staticVfxCreateAddress = scanner.ScanText( Constants.StaticVfxCreateSig );
            var staticVfxRemoveAddress = scanner.ScanText( Constants.StaticVfxRemoveSig );
            var actorVfxCreateAddress = scanner.ScanText( Constants.ActorVfxCreateSig );
            var actorVfxRemoveAddresTemp = scanner.ScanText( Constants.ActorVfxRemoveSig ) + 7;
            var actorVfxRemoveAddress = Marshal.ReadIntPtr( actorVfxRemoveAddresTemp + Marshal.ReadInt32( actorVfxRemoveAddresTemp ) + 4 );

            ActorVfxCreate = Marshal.GetDelegateForFunctionPointer<ActorVfxCreateDelegate>( actorVfxCreateAddress );
            ActorVfxRemove = Marshal.GetDelegateForFunctionPointer<ActorVfxRemoveDelegate>( actorVfxRemoveAddress );
            StaticVfxRemove = Marshal.GetDelegateForFunctionPointer<StaticVfxRemoveDelegate>( staticVfxRemoveAddress );
            StaticVfxRun = Marshal.GetDelegateForFunctionPointer<StaticVfxRunDelegate>( scanner.ScanText( Constants.StaticVfxRunSig ) );
            StaticVfxCreate = Marshal.GetDelegateForFunctionPointer<StaticVfxCreateDelegate>( staticVfxCreateAddress );

            StaticVfxCreateHook = Hook<StaticVfxCreateHookDelegate>.FromAddress( staticVfxCreateAddress, StaticVfxNewHandler );
            StaticVfxRemoveHook = Hook<StaticVfxRemoveHookDelegate>.FromAddress( staticVfxRemoveAddress, StaticVfxRemoveHandler );
            ActorVfxCreateHook = Hook<ActorVfxCreateHookDelegate>.FromAddress( actorVfxCreateAddress, ActorVfxNewHandler );
            ActorVfxRemoveHook = Hook<ActorVfxRemoveHookDelegate>.FromAddress( actorVfxRemoveAddress, ActorVfxRemoveHandler );

            GetMatrixSingleton = Marshal.GetDelegateForFunctionPointer<GetMatrixSingletonDelegate>( scanner.ScanText( Constants.GetMatrixSig ) );
            GetFileManager = Marshal.GetDelegateForFunctionPointer<GetFileManagerDelegate>( scanner.ScanText( Constants.GetFileManagerSig ) );
            GetFileManagerAlt = Marshal.GetDelegateForFunctionPointer<GetFileManagerDelegate>( scanner.ScanText( Constants.GetFileManager2Sig ) );
            DecRef = Marshal.GetDelegateForFunctionPointer<DecRefDelegate>( scanner.ScanText( Constants.DecRefSig ) );
            RequestFile = Marshal.GetDelegateForFunctionPointer<RequestFileDelegate>( scanner.ScanText( Constants.RequestFileSig ) );

            CheckFileStateHook = Hook<CheckFileStatePrototype>.FromAddress( scanner.ScanText( Constants.CheckFileStateSig ), CheckFileStateDetour );
            LoadTexFileLocal = Marshal.GetDelegateForFunctionPointer<LoadTexFileLocalDelegate>( scanner.ScanText( Constants.LoadTexFileLocalSig ) );
            LoadTexFileExternHook = Hook<LoadTexFileExternPrototype>.FromAddress( scanner.ScanText( Constants.LoadTexFileExternSig ), LoadTexFileExternDetour );
        }

        public void Enable() {
            if( IsEnabled ) return;
            ReadSqpackHook.Enable();
            GetResourceSyncHook.Enable();
            GetResourceAsyncHook.Enable();

            StaticVfxCreateHook.Enable();
            StaticVfxRemoveHook.Enable();
            ActorVfxCreateHook.Enable();
            ActorVfxRemoveHook.Enable();

            CheckFileStateHook.Enable();
            LoadTexFileExternHook.Enable();
            PathResolved += AddCrc;

            IsEnabled = true;
        }

        public void Dispose() {
            if( IsEnabled ) Disable();
        }

        public void Disable() {
            if( !IsEnabled ) return;

            PathResolved -= AddCrc;

            ReadSqpackHook.Disable();
            GetResourceSyncHook.Disable();
            GetResourceAsyncHook.Disable();
            StaticVfxCreateHook.Disable();
            StaticVfxRemoveHook.Disable();
            ActorVfxCreateHook.Disable();
            ActorVfxRemoveHook.Disable();
            CheckFileStateHook.Disable();
            LoadTexFileExternHook.Disable();

            Thread.Sleep( 500 );

            ReadSqpackHook.Dispose();
            GetResourceSyncHook.Dispose();
            GetResourceAsyncHook.Dispose();
            StaticVfxCreateHook.Dispose();
            StaticVfxRemoveHook.Dispose();
            ActorVfxCreateHook.Dispose();
            ActorVfxRemoveHook.Dispose();
            CheckFileStateHook.Dispose();
            LoadTexFileExternHook.Dispose();

            ReadSqpackHook = null;
            GetResourceSyncHook = null;
            GetResourceAsyncHook = null;
            StaticVfxCreateHook = null;
            StaticVfxRemoveHook = null;
            ActorVfxCreateHook = null;
            ActorVfxRemoveHook = null;
            CheckFileStateHook = null;
            LoadTexFileExternHook = null;

            IsEnabled = false;
        }
    }
}