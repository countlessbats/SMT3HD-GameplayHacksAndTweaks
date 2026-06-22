using System;
using System.Reflection;
using Il2Cpp;
using Il2Cpplibsdf_H;
using MelonLoader;

[assembly: MelonInfo(typeof(QuickPass.QuickPassMod), "QuickPass", "1.0.0", "local")]
[assembly: MelonGame(null, "smt3hd")]

namespace QuickPass
{
    /// <summary>
    /// QuickPass — R1 = Pass turn, L1 = Escape combat.
    /// Two-phase: frame 1 sets cursor, frame 2 calls SelectCommandListAuto.
    /// </summary>
    public class QuickPassMod : MelonMod
    {
        private int _cooldown = 0;
        private int _frameCount = 0;
        private int _pendingType = -1;   // which command type to confirm
        private int _pendingIdx = -1;    // which cursor index
        private int _pendingVal = 0;     // command value

        public override void OnInitializeMelon()
        {
            LoggerInstance.Msg("QuickPass: R1=Pass, L1=Escape.");
        }

        public override void OnUpdate()
        {
            _frameCount++;
            if (_cooldown > 0) { _cooldown--; return; }
            if (_frameCount < 300) return;

            // Phase 2: confirm pending selection
            if (_pendingType >= 0)
            {
                try
                {
                    var commData = nbCommSelProcess.GetCommSelProcessData();
                    if (commData != null)
                    {
                        var cursors = commData.nowcursor;
                        if (cursors != null)
                        {
                            commData.nowtype = (byte)_pendingType;
                            cursors[_pendingType] = (sbyte)_pendingIdx;
                            commData.drawcursor = _pendingIdx;

                            if (commData.act != null)
                                commData.act.select = (uint)_pendingVal;

                            nbCommSelProcess.SelectCommandListAuto(ref commData);
                            LoggerInstance.Msg($"[QP] Confirmed: type={_pendingType} idx={_pendingIdx} val={_pendingVal}");
                        }
                    }
                }
                catch { }
                _pendingType = -1;
                _cooldown = 30;
                return;
            }

            try
            {
                bool r1 = dds3PadManager.DDS3_PADCHECK_TRIG(SDF_PADMAP.SDF_PADMAP_R1, 0);
                bool l1 = dds3PadManager.DDS3_PADCHECK_TRIG(SDF_PADMAP.SDF_PADMAP_L1, 0);

                // Check Hyperconfig for per-button enable/disable
                if (r1) r1 = IsHyperconfigEnabled("QuickPass.R1Pass");
                if (l1) l1 = IsHyperconfigEnabled("QuickPass.L1Flee");

                if (!r1 && !l1) return;

                int commStat = nbCommSelProcess.nbGetCommSelStat();
                if (commStat == 0) return;

                var commData = nbCommSelProcess.GetCommSelProcessData();
                if (commData == null) return;

                var counts = commData.commcnt;
                var cursors = commData.nowcursor;
                var commlist = commData.commlist;
                if (counts == null || cursors == null || commlist == null) return;

                if (r1)
                {
                    // Pass = last item in type 0 — try instant confirm
                    int count = counts[0];
                    if (count <= 0) return;
                    int lastIdx = count - 1;
                    var list = commlist[0];
                    if (list == null || list.Length < count) return;

                    cursors[0] = (sbyte)lastIdx;
                    commData.nowtype = 0;
                    commData.drawcursor = lastIdx;
                    if (commData.act != null) commData.act.select = (uint)list[lastIdx];
                    nbCommSelProcess.SelectCommandListAuto(ref commData);
                    LoggerInstance.Msg($"[QP] R1 Pass instant: idx={lastIdx}");
                    _cooldown = 30;
                    return;
                }
                else if (l1)
                {
                    // Log ALL commands AND try to find Retreat (32769)
                    bool found = false;
                    for (int t = 0; t < Math.Min(counts.Length, 6); t++)
                    {
                        int count = counts[t];
                        var list = commlist[t];
                        if (list == null || count <= 0) continue;

                        string vals = "";
                        for (int i = 0; i < count && i < list.Length; i++)
                            vals += list[i] + " ";
                        LoggerInstance.Msg($"[QP] type[{t}] ({count}): {vals.Trim()}");

                        if (!found)
                        {
                            for (int i = 0; i < count && i < list.Length; i++)
                            {
                                if (list[i] == 32769)
                                {
                                    cursors[t] = (sbyte)i;
                                    commData.nowtype = (byte)t;
                                    commData.drawcursor = i;
                                    if (commData.act != null) commData.act.select = 32769;
                                    nbCommSelProcess.SelectCommandListAuto(ref commData);
                                    LoggerInstance.Msg($"[QP] L1 Retreat instant: type={t} idx={i}");
                                    found = true;
                                    _cooldown = 30;
                                }
                            }
                        }
                    }
                    if (!found)
                        LoggerInstance.Msg("[QP] L1: Retreat (32769) not found.");
                }
            }
            catch { }
        }

        /// <summary>Check Hyperconfig for a setting. Returns true if On or if Hyperconfig isn't installed.</summary>
        private static MethodInfo? _hcGetString;
        private static bool _hcResolved = false;
        private static bool IsHyperconfigEnabled(string key)
        {
            if (!_hcResolved)
            {
                _hcResolved = true;
                try
                {
                    var hcType = Type.GetType("Hyperconfig.HyperconfigStore, Hyperconfig");
                    if (hcType != null)
                        _hcGetString = hcType.GetMethod("GetString", new[] { typeof(string), typeof(string) });
                }
                catch { }
            }

            if (_hcGetString == null) return true; // no Hyperconfig = default enabled

            try
            {
                string val = (string)(_hcGetString.Invoke(null, new object[] { key, "On" }) ?? "On");
                return val.Equals("On", StringComparison.OrdinalIgnoreCase);
            }
            catch { return true; }
        }
    }
}
