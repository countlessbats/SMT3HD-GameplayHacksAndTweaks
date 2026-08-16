using Il2Cpp;
using Il2Cpplibsdf_H;
using MelonLoader;

[assembly: MelonInfo(typeof(MoonKing.MoonKingMod), "MoonKing", "1.0.2", "local")]
[assembly: MelonGame(null, "smt3hd")]

namespace MoonKing
{
    public class MoonKingMod : MelonMod
    {
        private bool _fullMoon = true;
        private int _cooldown = 0;
        private int _frameCount = 0;
        private bool _alternateBindings = false;

        public override void OnInitializeMelon()
        {
            LoggerInstance.Msg("MoonKing: D-pad Down or Down Arrow toggles moon phase. F3 switches D-pad Down to R3.");
        }

        public override void OnUpdate()
        {
            _frameCount++;

            if (KeyboardInput.F3Pressed())
            {
                _alternateBindings = !_alternateBindings;
                LoggerInstance.Msg($"MoonKing: controller binding is now {(_alternateBindings ? "R3" : "D-pad Down")}; Down Arrow remains active.");
            }

            if (_cooldown > 0) { _cooldown--; return; }
            if (_frameCount < 300) return;

            try
            {
                bool controllerTrigger;
                if (_alternateBindings)
                {
                    controllerTrigger = dds3PadManager.DDS3_PADCHECK_TRIG(SDF_PADMAP.SDF_PADMAP_R3, 0);
                }
                else
                {
                    controllerTrigger = dds3PadManager.DDS3_PADCHECK_TRIG(SDF_PADMAP.SDF_PADMAP_D, 0);
                    if (controllerTrigger)
                    {
                        byte analogY = dds3PadManager.GetPadAnalog(0, 0, 1, 0);
                        controllerTrigger = analogY >= 116 && analogY <= 140;
                    }
                }
                if (!controllerTrigger && !KeyboardInput.DownArrowPressed()) return;

                // Block in combat
                try { if (nbMainProcess.nbGetMainProcessData() != null) return; } catch { }

                // Block in camp menu
                try { if (cmpInit.cmpChkCampProcess() != 0) return; } catch { }

                // Block when field process is shut down (terminal, shop, etc.)
                // fldProcShutDownFlg=1 means field is active, 0 means UI took over
                try { if (fldProcess.fldProcShutDownFlg == 0) return; } catch { }

                _fullMoon = !_fullMoon;
                evtMoon.evtSetAgeOfMoon(_fullMoon ? 8 : 0);
                try { SoundManager.PlaySE(_fullMoon ? 1 : 0); } catch { }
                LoggerInstance.Msg($"MoonKing: {(_fullMoon ? "Full Moon" : "New Moon")}");
                _cooldown = 15;
            }
            catch { }
        }
    }
}
