using Il2Cpp;
using Il2Cpplibsdf_H;
using MelonLoader;

namespace SafePassage
{
    internal static class L3Suppression
    {
        private static int _restoreUiFrames;

        internal static bool AlternateMode { get; set; }

        internal static void Init(MelonLogger.Instance log)
        {
            log.Msg("SafePassage: L3 screenshot-UI suppression uses state restoration (no native input hook).");
        }

        internal static void Tick()
        {
            if (_restoreUiFrames <= 0) return;
            RestoreUi();
            _restoreUiFrames--;
        }

        internal static bool PollL3Trigger()
        {
            bool triggered = false;
            try
            {
                triggered = dds3PadManager.DDS3_PADCHECK_TRIG(SDF_PADMAP.SDF_PADMAP_L3, 0);
            }
            catch { }

            if (triggered && AlternateMode)
            {
                // Vanilla also uses L3 to toggle the screenshot/UI-hidden state. Restore
                // visibility over several frames so this consumed press remains exclusive
                // to SafePassage regardless of update ordering.
                _restoreUiFrames = 4;
                RestoreUi();
            }

            return triggered;
        }

        private static void RestoreUi()
        {
            try
            {
                dds3KernelMain.UIDispFlag = true;
                dds3KernelMain.UIDispFlag_OLD = true;
            }
            catch { }
        }
    }
}
