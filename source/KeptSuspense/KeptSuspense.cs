using System;
using System.Reflection;
using System.Runtime.InteropServices;
using Il2Cpp;
using MelonLoader;
using MelonLoader.NativeUtils;

[assembly: MelonInfo(typeof(KeptSuspense.KeptSuspenseMod), "KeptSuspense", "1.0.0", "local")]
[assembly: MelonGame(null, "smt3hd")]

namespace KeptSuspense
{
    /// <summary>
    /// KeptSuspense — Skip Continue yes/no dialog + preserve suspend data.
    ///
    /// When user selects Continue, the save info sub-screen still shows (press A).
    /// The yes/no confirmation dialog is auto-skipped — goes straight to loading.
    /// After loading, suspend data is re-saved so it persists for next time.
    /// </summary>
    public class KeptSuspenseMod : MelonMod
    {
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr d_ContinueProc(IntPtr procId, IntPtr methodInfo);
        private static NativeHook<d_ContinueProc>? _hookContinueProc;
        private static d_ContinueProc? _delContinueProc;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr d_LoadData(IntPtr methodInfo);
        private static d_LoadData? _nativeLoadData;
        private static IntPtr _loadDataMip;

        private static bool _skipDialog = true;
        private static bool _prevSuspendExists = false;
        private static bool _initialized = false;
        private static int _reSaveCountdown = 0;
        private static int _frameCount = 0;
        private static MelonLogger.Instance? _log;

        private static bool _resolved = false;
        private static PropertyInfo? _globalDataSaveData;
        private static MethodInfo? _fsSaveSave;

        public override void OnInitializeMelon()
        {
            _log = LoggerInstance;

            try
            {
                // Hook slContinueYesNoProc — the per-frame dialog processor
                _delContinueProc = Hook_ContinueYesNoProc;
                var f = typeof(slMain).GetField(
                    "NativeMethodInfoPtr_slContinueYesNoProc_Private_Static_Object_dds3ProcessID_t_0",
                    BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
                if (f != null)
                {
                    IntPtr mip = (IntPtr)f.GetValue(null)!;
                    IntPtr fp = Marshal.ReadIntPtr(mip);
                    _hookContinueProc = new NativeHook<d_ContinueProc>(fp, Marshal.GetFunctionPointerForDelegate(_delContinueProc));
                    _hookContinueProc.Attach();
                    LoggerInstance.Msg("KeptSuspense: slContinueYesNoProc hooked.");
                }

                // Resolve slLoadData for direct calling
                var f2 = typeof(slMain).GetField(
                    "NativeMethodInfoPtr_slLoadData_Private_Static_Object_0",
                    BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
                if (f2 == null)
                {
                    foreach (var ff in typeof(slMain).GetFields(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public))
                    {
                        if (ff.Name.Contains("slLoadData") && ff.Name.Contains("NativeMethodInfoPtr"))
                        { f2 = ff; break; }
                    }
                }
                if (f2 != null)
                {
                    _loadDataMip = (IntPtr)f2.GetValue(null)!;
                    IntPtr fp2 = Marshal.ReadIntPtr(_loadDataMip);
                    _nativeLoadData = Marshal.GetDelegateForFunctionPointer<d_LoadData>(fp2);
                    LoggerInstance.Msg("KeptSuspense: slLoadData resolved.");
                }
            }
            catch (Exception ex) { LoggerInstance.Error($"KeptSuspense: {ex.Message}"); }

            LoggerInstance.Msg("KeptSuspense: dialog skip + suspend preserve active.");
        }

        private static IntPtr Hook_ContinueYesNoProc(IntPtr procId, IntPtr methodInfo)
        {
            if (_skipDialog)
            {
                _skipDialog = false;
                _log?.Msg("KeptSuspense: Skipping dialog — calling slLoadData directly!");

                try
                {
                    slMain.gslSelect = 20;
                    slMain.slChooseSaveIndex = 20;
                }
                catch { }

                if (_nativeLoadData != null)
                    return _nativeLoadData(_loadDataMip);
            }

            return _hookContinueProc!.Trampoline(procId, methodInfo);
        }

        private static void Resolve()
        {
            if (_resolved) return;
            _resolved = true;
            Type? globalDataType = null, fsSaveType = null;
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                globalDataType ??= asm.GetType("Il2Cpp.GlobalData");
                fsSaveType ??= asm.GetType("Il2Cpp.FsSaveData");
            }
            _globalDataSaveData = globalDataType?.GetProperty("saveData", BindingFlags.Public | BindingFlags.Static);
            _fsSaveSave = fsSaveType?.GetMethod("Save",
                new[] { typeof(int), typeof(Il2Cppdds3GlobalWork_H.dds3GlobalWork_t), typeof(bool) });
        }

        public override void OnUpdate()
        {
            _frameCount++;
            if (_frameCount < 60) return;

            try
            {
                bool suspendNow = slMain.slSuspendDataCheck();

                if (!_initialized)
                {
                    _prevSuspendExists = suspendNow;
                    _initialized = true;
                    return;
                }

                // Suspend consumed — re-save after a short delay
                if (_prevSuspendExists && !suspendNow && _reSaveCountdown == 0)
                {
                    _log?.Msg("KeptSuspense: Suspend consumed — re-saving in 60 frames...");
                    _reSaveCountdown = 60;
                }

                if (_reSaveCountdown > 0)
                {
                    _reSaveCountdown--;
                    if (_reSaveCountdown == 0)
                    {
                        Resolve();
                        ReSaveSuspend();
                    }
                }

                // Re-arm skip when suspend reappears (back on title screen)
                if (suspendNow && !_prevSuspendExists)
                    _skipDialog = true;

                _prevSuspendExists = suspendNow;
            }
            catch { }
        }

        private void ReSaveSuspend()
        {
            try
            {
                var gameState = slMain.dds3Work;
                var fsSave = _globalDataSaveData?.GetValue(null);
                if (gameState == null || fsSave == null || _fsSaveSave == null)
                {
                    _log?.Warning("KeptSuspense: Not ready — retrying...");
                    _reSaveCountdown = 60;
                    return;
                }

                var result = (bool)_fsSaveSave.Invoke(fsSave, new object[] { 20, gameState, false })!;
                if (result)
                    _log?.Msg("KeptSuspense: Suspend RE-SAVED! Resume still available.");
                else
                    _log?.Warning("KeptSuspense: Save returned false.");
            }
            catch (Exception ex)
            {
                _log?.Error($"KeptSuspense: {ex.InnerException?.Message ?? ex.Message}");
                _reSaveCountdown = 60;
            }
        }

        public override void OnDeinitializeMelon()
        {
            _hookContinueProc?.Detach();
        }
    }
}
