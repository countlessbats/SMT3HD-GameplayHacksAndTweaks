using Il2Cpp;
using Il2Cpplibsdf_H;
using MelonLoader;

[assembly: MelonInfo(typeof(MoonKing.MoonKingMod), "MoonKing", "1.0.1", "local")]
[assembly: MelonGame(null, "smt3hd")]

namespace MoonKing
{
    public class MoonKingMod : MelonMod
    {
        private bool _fullMoon = true;
        private int _cooldown = 0;
        private int _frameCount = 0;

        public override void OnInitializeMelon()
        {
            LoggerInstance.Msg("MoonKing: D-pad Down or Down Arrow toggles Full/New Moon.");
        }

        public override void OnUpdate()
        {
            _frameCount++;

            if (_cooldown > 0) { _cooldown--; return; }
            if (_frameCount < 300) return;

            try
            {
                bool dPadDown = dds3PadManager.DDS3_PADCHECK_TRIG(SDF_PADMAP.SDF_PADMAP_D, 0);
                if (dPadDown)
                {
                    byte analogY = dds3PadManager.GetPadAnalog(0, 0, 1, 0);
                    dPadDown = analogY >= 116 && analogY <= 140;
                }
                if (!dPadDown && !KeyboardInput.DownArrowPressed()) return;

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
