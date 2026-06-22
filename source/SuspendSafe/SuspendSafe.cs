using System;
using System.Reflection;
using Il2Cpp;
using Il2Cpplibsdf_H;
using MelonLoader;

[assembly: MelonInfo(typeof(SuspendSafe.SuspendSafeMod), "SuspendSafe", "1.0.0", "local")]
[assembly: MelonGame(null, "smt3hd")]

namespace SuspendSafe
{
    /// <summary>
    /// SuspendSafe — Press R3 to create a suspend (quicksave) at your exact position.
    /// No menu interruption, no title screen kick. Just save and keep playing.
    /// Resume from the title screen like a normal suspend save.
    /// </summary>
    public class SuspendSafeMod : MelonMod
    {
        private static int _cooldown = 0;
        private static int _frameCount = 0;
        private static MelonLogger.Instance? _log;

        private static bool _resolved = false;
        private static PropertyInfo? _globalDataSaveData;
        private static MethodInfo? _fsSaveSave;

        public override void OnInitializeMelon()
        {
            _log = LoggerInstance;
            LoggerInstance.Msg("SuspendSafe: Press R3 to quicksave. Resume from title screen.");
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

            _globalDataSaveData = globalDataType?.GetProperty("saveData",
                BindingFlags.Public | BindingFlags.Static);
            _fsSaveSave = fsSaveType?.GetMethod("Save",
                new[] { typeof(int), typeof(Il2Cppdds3GlobalWork_H.dds3GlobalWork_t), typeof(bool) });

            _log?.Msg($"SuspendSafe ready: {_globalDataSaveData != null && _fsSaveSave != null}");
        }

        public override void OnUpdate()
        {
            _frameCount++;
            if (_cooldown > 0) { _cooldown--; return; }
            if (_frameCount < 300) return;

            try
            {
                if (!dds3PadManager.DDS3_PADCHECK_TRIG(SDF_PADMAP.SDF_PADMAP_R3, 0)) return;

                if (dds3SequenceList.CheckNewBattle() != 0 ||
                    dds3SequenceList.CheckTitle() != 0)
                {
                    _cooldown = 30;
                    return;
                }

                Resolve();
                Quicksave();
                _cooldown = 180;
            }
            catch { }
        }

        private void Quicksave()
        {
            try
            {
                var gameState = slMain.dds3Work;
                var fsSave = _globalDataSaveData?.GetValue(null);

                if (gameState == null || fsSave == null || _fsSaveSave == null)
                {
                    _log?.Warning("SuspendSafe: Save system not available.");
                    return;
                }

                var result = (bool)_fsSaveSave.Invoke(fsSave, new object[] { 20, gameState, false })!;

                if (result)
                    _log?.Msg("SuspendSafe: Quicksave created! Resume available on title screen.");
                else
                    _log?.Warning("SuspendSafe: Save returned false.");
            }
            catch (Exception ex)
            {
                _log?.Error($"SuspendSafe: {ex.InnerException?.Message ?? ex.Message}");
            }
        }
    }
}
