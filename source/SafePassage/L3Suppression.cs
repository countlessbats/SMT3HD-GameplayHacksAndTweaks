using System;
using System.Reflection;
using System.Runtime.InteropServices;
using Il2Cpp;
using Il2Cpplibsdf_H;
using MelonLoader;
using MelonLoader.NativeUtils;

namespace SafePassage
{
    internal static class L3Suppression
    {
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate bool d_PadCheckTrig(SDF_PADMAP id, int controller, IntPtr methodInfo);

        private static NativeHook<d_PadCheckTrig>? _hook;
        private static d_PadCheckTrig? _detour;
        private static bool _hookAttached;
        private static int _currentFrame;
        private static int _l3TriggerFrame = -1;

        internal static bool AlternateMode { get; set; }

        internal static void Init(MelonLogger.Instance log)
        {
            try
            {
                const string fieldName =
                    "NativeMethodInfoPtr_DDS3_PADCHECK_TRIG_Public_Static_Boolean_SDF_PADMAP_Int32_0";
                var field = typeof(dds3PadManager).GetField(fieldName,
                    BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
                if (field == null) throw new MissingFieldException(typeof(dds3PadManager).FullName, fieldName);

                IntPtr methodInfo = (IntPtr)field.GetValue(null)!;
                IntPtr function = Marshal.ReadIntPtr(methodInfo);
                if (function == IntPtr.Zero) throw new InvalidOperationException("DDS3_PADCHECK_TRIG pointer is null");

                _detour = HookPadCheckTrig;
                _hook = new NativeHook<d_PadCheckTrig>(
                    function, Marshal.GetFunctionPointerForDelegate(_detour));
                _hook.Attach();
                _hookAttached = true;
                log.Msg("SafePassage: L3 screenshot-UI suppression hook attached.");
            }
            catch (Exception ex)
            {
                log.Error($"SafePassage: Could not suppress vanilla L3: {ex.Message}");
            }
        }

        internal static bool PollL3Trigger(int frame)
        {
            _currentFrame = frame;
            bool directTrigger = false;
            try
            {
                directTrigger = dds3PadManager.DDS3_PADCHECK_TRIG(SDF_PADMAP.SDF_PADMAP_L3, 0);
            }
            catch { }

            return _hookAttached ? _l3TriggerFrame == frame : directTrigger;
        }

        private static bool HookPadCheckTrig(SDF_PADMAP id, int controller, IntPtr methodInfo)
        {
            bool triggered = _hook!.Trampoline(id, controller, methodInfo);
            if (!AlternateMode || controller != 0 || id != SDF_PADMAP.SDF_PADMAP_L3)
                return triggered;

            if (triggered) _l3TriggerFrame = _currentFrame;
            return false;
        }
    }
}
