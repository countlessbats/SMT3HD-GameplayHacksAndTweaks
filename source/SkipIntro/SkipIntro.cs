using System;
using System.Reflection;
using System.Runtime.InteropServices;
using Il2Cpp;
using MelonLoader;
using MelonLoader.NativeUtils;

[assembly: MelonInfo(typeof(SkipIntro.SkipIntroMod), "SkipIntro", "1.0.0", "local")]
[assembly: MelonGame(null, "smt3hd")]

namespace SkipIntro
{
    /// <summary>
    /// SkipIntro v17 — Hook dds3TitleInit and change its mode parameter
    /// to skip the intro sequence entirely. The game calls dds3TitleInit(0)
    /// for a full intro — we change it to mode 1 (or 2, 3...) to see
    /// if a higher mode skips directly to the menu.
    /// </summary>
    public class SkipIntroMod : MelonMod
    {
        // dds3TitleInit(Int32 mode) — static void
        // Native: void fn(int mode, IntPtr methodInfo)
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void d_TitleInit(int mode, IntPtr methodInfo);

        private static NativeHook<d_TitleInit>? _hook;
        private static d_TitleInit? _del;
        private static bool _firstCall = true;

        public override void OnInitializeMelon()
        {
            try
            {
                _del = Hook_TitleInit;

                var field = typeof(dds3TitleMain).GetField(
                    "NativeMethodInfoPtr_dds3TitleInit_Public_Static_Void_Int32_0",
                    BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);

                if (field == null) { LoggerInstance.Error("SkipIntro: field not found"); return; }

                IntPtr mip = (IntPtr)field.GetValue(null)!;
                IntPtr fp = Marshal.ReadIntPtr(mip);
                _hook = new NativeHook<d_TitleInit>(fp, Marshal.GetFunctionPointerForDelegate(_del));
                _hook.Attach();

                LoggerInstance.Msg("SkipIntro v17: dds3TitleInit hooked.");
            }
            catch (Exception ex) { LoggerInstance.Error($"SkipIntro: {ex}"); }
        }

        private static void Hook_TitleInit(int mode, IntPtr methodInfo)
        {
            if (_firstCall)
            {
                _firstCall = false;
                MelonLogger.Msg($"SkipIntro: dds3TitleInit called with mode={mode}, changing to mode=1");
                _hook!.Trampoline(1, methodInfo);
            }
            else
            {
                MelonLogger.Msg($"SkipIntro: dds3TitleInit subsequent call mode={mode}");
                _hook!.Trampoline(mode, methodInfo);
            }
        }

        public override void OnDeinitializeMelon()
        {
            _hook?.Detach();
        }
    }
}
