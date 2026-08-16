using System;
using System.Reflection;
using Il2Cpp;
using Il2Cpplibsdf_H;
using MelonLoader;

[assembly: MelonInfo(typeof(SafePassage.SafePassageMod), "SafePassage", "1.0.1", "local")]
[assembly: MelonGame(null, "smt3hd")]

namespace SafePassage
{
    public class SafePassageMod : MelonMod
    {
        private bool _encountersDisabled = false;
        private int _cooldown = 0;
        private int _frameCount = 0;

        // On-screen popup
        private static object? _popupGO;
        private static object? _popupText;
        private static MethodInfo? _setText, _goSetActive;
        private static int _popupTimer = 0;
        private static bool _popupCreated = false;

        public override void OnInitializeMelon()
        {
            LoggerInstance.Msg("SafePassage: D-pad Up or Up Arrow toggles random encounters.");
        }

        public override void OnUpdate()
        {
            _frameCount++;

            // Popup dismiss — runs every frame regardless of cooldown
            if (_popupTimer > 0)
            {
                _popupTimer--;
                if (_popupTimer == 0)
                    try { _goSetActive?.Invoke(_popupGO, new object[] { false }); } catch { }
            }

            if (_cooldown > 0) { _cooldown--; return; }
            if (_frameCount < 300) return;

            // Keep NoEncountCnt high every frame when disabled
            if (_encountersDisabled)
            {
                try
                {
                    var gw = fldGlobal.fldGb;
                    if (gw != null && gw.NoEncountCnt < 999000)
                        gw.NoEncountCnt = 999999;
                }
                catch { }
            }

            try
            {
                bool dPadUp = dds3PadManager.DDS3_PADCHECK_TRIG(SDF_PADMAP.SDF_PADMAP_U, 0);
                if (dPadUp)
                {
                    byte analogY = dds3PadManager.GetPadAnalog(0, 0, 1, 0);
                    dPadUp = analogY >= 116 && analogY <= 140;
                }
                if (!dPadUp && !KeyboardInput.UpArrowPressed()) return;

                // Block in combat
                try { if (nbMainProcess.nbGetMainProcessData() != null) return; } catch { }

                // Block in camp menu
                try { if (cmpInit.cmpChkCampProcess() != 0) return; } catch { }

                // Block when field process is shut down (terminal, shop, etc.)
                try { if (fldProcess.fldProcShutDownFlg == 0) return; } catch { }

                _encountersDisabled = !_encountersDisabled;

                if (_encountersDisabled)
                {
                    fldCommand.fldCommand_ENCOUNT_OFF();
                    var gw = fldGlobal.fldGb;
                    if (gw != null) gw.NoEncountCnt = 999999;
                    try { SoundManager.PlaySE(1); } catch { }
                    LoggerInstance.Msg("SafePassage: Encounters OFF");
                    ShowPopup("Encounters OFF");
                }
                else
                {
                    fldCommand.fldCommand_ENCOUNT_ON();
                    var gw = fldGlobal.fldGb;
                    if (gw != null) gw.NoEncountCnt = 0;
                    try { SoundManager.PlaySE(0); } catch { }
                    LoggerInstance.Msg("SafePassage: Encounters ON");
                    ShowPopup("Encounters ON");
                }

                _cooldown = 15;
            }
            catch { }
        }

        private void ShowPopup(string message)
        {
            if (!_popupCreated) CreatePopup();
            if (_popupText == null || _setText == null) return;
            try
            {
                _setText.Invoke(_popupText, new object[] { message });
                _goSetActive?.Invoke(_popupGO, new object[] { true });
                _popupTimer = 90; // ~1.5s at 60fps
            }
            catch { }
        }

        private void CreatePopup()
        {
            _popupCreated = true;
            try
            {
                // Load TextRenderingModule for font
                try
                {
                    var asmDir = System.IO.Path.GetDirectoryName(typeof(nbCalc).Assembly.Location)!;
                    Assembly.LoadFrom(System.IO.Path.Combine(asmDir, "UnityEngine.TextRenderingModule.dll"));
                }
                catch { }

                // Find types
                Type? goType = null, canvasType = null, rtType = null, imgType = null, textType = null;
                foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
                {
                    try
                    {
                        foreach (var t in a.GetTypes())
                        {
                            if (t.Namespace == null || t.Namespace.Contains("Melon")) continue;
                            if (t.Name == "GameObject" && goType == null) goType = t;
                            if (t.Name == "Canvas" && t.Namespace.Contains("UnityEngine") && canvasType == null) canvasType = t;
                            if (t.Name == "RectTransform" && rtType == null) rtType = t;
                            if (t.Name == "Image" && t.Namespace.Contains("UI") && imgType == null) imgType = t;
                            if (t.Name == "TextMeshProUGUI" && textType == null) textType = t;
                        }
                    }
                    catch { }
                }
                // Fallback to UI.Text
                if (textType == null)
                    foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
                    {
                        try { foreach (var t in a.GetTypes()) if (t.Name == "Text" && t.Namespace != null && t.Namespace.Contains("UI") && !t.Namespace.Contains("Melon")) { textType = t; break; } } catch { }
                        if (textType != null) break;
                    }

                bool useTMP = textType?.Name == "TextMeshProUGUI";
                if (goType == null || canvasType == null || textType == null) return;

                var goCtor = goType.GetConstructor(new[] { typeof(string) })!;
                var addComp = goType.GetMethod("AddComponent", Type.EmptyTypes)!;
                var getComp = goType.GetMethod("GetComponent", Type.EmptyTypes)!;
                var transProp = goType.GetProperty("transform")!;
                _goSetActive = goType.GetMethod("SetActive", new[] { typeof(bool) });

                // Canvas
                _popupGO = goCtor.Invoke(new object[] { "SafePassagePopup" });
                var canvas = addComp.MakeGenericMethod(canvasType).Invoke(_popupGO, null);
                canvasType.GetProperty("renderMode")!.SetValue(canvas,
                    Enum.ToObject(canvasType.GetProperty("renderMode")!.PropertyType, 0));
                canvasType.GetProperty("sortingOrder")?.SetValue(canvas, 9998);

                // DDOL
                try
                {
                    foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
                    {
                        var oT = a.GetType("UnityEngine.Object", false); if (oT == null) continue;
                        foreach (var m in oT.GetMethods(BindingFlags.Public | BindingFlags.Static))
                            if (m.Name == "DontDestroyOnLoad") { m.Invoke(null, new[] { _popupGO }); goto done; }
                    }
                    done:;
                }
                catch { }

                var parentTr = transProp.GetValue(_popupGO);

                // Background
                if (imgType != null)
                {
                    var bgGO = goCtor.Invoke(new object[] { "PopBG" });
                    var bgTr = transProp.GetValue(bgGO)!;
                    bgTr.GetType().GetMethod("SetParent", new[] { parentTr!.GetType(), typeof(bool) })
                        ?.Invoke(bgTr, new[] { parentTr, (object)false });
                    addComp.MakeGenericMethod(imgType).Invoke(bgGO, null);
                    var bgImg = getComp.MakeGenericMethod(imgType).Invoke(bgGO, null);
                    SetColor(bgImg, 0.0f, 0.0f, 0.05f, 0.75f);
                    if (rtType != null) SetRT(getComp.MakeGenericMethod(rtType).Invoke(bgGO, null), 220f, 36f);
                }

                // Text
                var txtGO = goCtor.Invoke(new object[] { "PopTxt" });
                var txtTr = transProp.GetValue(txtGO)!;
                txtTr.GetType().GetMethod("SetParent", new[] { parentTr!.GetType(), typeof(bool) })
                    ?.Invoke(txtTr, new[] { parentTr, (object)false });
                _popupText = addComp.MakeGenericMethod(textType).Invoke(txtGO, null);
                _setText = textType.GetProperty("text")?.GetSetMethod();

                if (useTMP)
                {
                    textType.GetProperty("fontSize")?.SetValue(_popupText, 18f);
                    textType.GetProperty("richText")?.SetValue(_popupText, true);
                    textType.GetProperty("enableWordWrapping")?.SetValue(_popupText, false);
                    var alP = textType.GetProperty("alignment");
                    if (alP != null) alP.SetValue(_popupText, Enum.ToObject(alP.PropertyType, 514)); // Center
                    // TMP default font
                    Type? tmpS = null;
                    foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
                    { try { foreach (var t in a.GetTypes()) if (t.Name == "TMP_Settings") { tmpS = t; break; } } catch { } if (tmpS != null) break; }
                    var font = tmpS?.GetProperty("defaultFontAsset", BindingFlags.Public | BindingFlags.Static)?.GetValue(null);
                    if (font != null) textType.GetProperty("font")?.SetValue(_popupText, font);
                }
                else
                {
                    textType.GetProperty("fontSize")?.SetValue(_popupText, 18);
                    textType.GetProperty("supportRichText")?.SetValue(_popupText, true);
                    var alP = textType.GetProperty("alignment");
                    if (alP != null) alP.SetValue(_popupText, Enum.ToObject(alP.PropertyType, 4));
                    textType.GetProperty("horizontalOverflow")?.SetValue(_popupText,
                        Enum.ToObject(textType.GetProperty("horizontalOverflow")!.PropertyType, 1));
                    textType.GetProperty("verticalOverflow")?.SetValue(_popupText,
                        Enum.ToObject(textType.GetProperty("verticalOverflow")!.PropertyType, 1));
                    // Arial
                    LoadArial(textType, _popupText);
                }

                SetColor(_popupText, 1f, 1f, 1f, 1f);
                if (rtType != null) SetRT(getComp.MakeGenericMethod(rtType).Invoke(txtGO, null), 220f, 36f);

                _goSetActive?.Invoke(_popupGO, new object[] { false });
            }
            catch (Exception ex)
            {
                LoggerInstance.Error($"SafePassage popup: {ex.InnerException?.Message ?? ex.Message}");
            }
        }

        private void SetRT(object? rt, float w, float h)
        {
            if (rt == null) return;
            var sdP = rt.GetType().GetProperty("sizeDelta");
            var v2C = sdP?.PropertyType.GetConstructor(new[] { typeof(float), typeof(float) });
            if (v2C == null) return;
            rt.GetType().GetProperty("anchorMin")?.SetValue(rt, v2C.Invoke(new object[] { 1f, 1f }));
            rt.GetType().GetProperty("anchorMax")?.SetValue(rt, v2C.Invoke(new object[] { 1f, 1f }));
            rt.GetType().GetProperty("pivot")?.SetValue(rt, v2C.Invoke(new object[] { 1f, 1f }));
            sdP!.SetValue(rt, v2C.Invoke(new object[] { w, h }));
            rt.GetType().GetProperty("anchoredPosition")?.SetValue(rt, v2C.Invoke(new object[] { -10f, -10f }));
        }

        private void SetColor(object? c, float r, float g, float b, float a)
        {
            if (c == null) return;
            try
            {
                var cp = c.GetType().GetProperty("color");
                var cc = cp?.PropertyType.GetConstructor(new[] { typeof(float), typeof(float), typeof(float), typeof(float) });
                cp?.SetValue(c, cc?.Invoke(new object[] { r, g, b, a }));
            }
            catch { }
        }

        private void LoadArial(Type textType, object? text)
        {
            if (text == null) return;
            try
            {
                Type? resT = null, fontT = null;
                foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
                {
                    resT ??= a.GetType("UnityEngine.Resources", false);
                    try { foreach (var t in a.GetTypes()) if (t.Name == "Font" && t.Namespace != null && t.Namespace.Contains("UnityEngine") && !t.Namespace.Contains("Melon")) { fontT = t; break; } } catch { }
                }
                if (resT == null || fontT == null) return;
                MethodInfo? gbr = null;
                foreach (var m in resT.GetMethods(BindingFlags.Public | BindingFlags.Static))
                    if (m.Name == "GetBuiltinResource" && !m.IsGenericMethod && m.GetParameters().Length == 2) gbr = m;
                if (gbr == null) return;
                Type? il2cppTypeUtil = null;
                foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
                    il2cppTypeUtil ??= a.GetType("Il2CppInterop.Runtime.Il2CppType", false);
                var fromM = il2cppTypeUtil?.GetMethod("From", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(Type) }, null);
                var il2ft = fromM?.Invoke(null, new object[] { fontT });
                var result = gbr.Invoke(null, new[] { il2ft, (object)"Arial.ttf" });
                if (result != null)
                {
                    if (!fontT.IsAssignableFrom(result.GetType()))
                    {
                        var ptrP = result.GetType().GetProperty("Pointer");
                        var bt = result.GetType();
                        while (ptrP == null && bt != null) { ptrP = bt.GetProperty("Pointer", BindingFlags.Public | BindingFlags.Instance); bt = bt.BaseType; }
                        if (ptrP != null) { var ptr = (IntPtr)ptrP.GetValue(result)!; if (ptr != IntPtr.Zero) result = fontT.GetConstructor(new[] { typeof(IntPtr) })?.Invoke(new object[] { ptr }); }
                    }
                    textType.GetProperty("font")?.SetValue(text, result);
                }
            }
            catch { }
        }
    }
}
