using System;
using System.IO;
using System.Reflection;

namespace MoonKing
{
    internal static class KeyboardInput
    {
        private const int DownArrow = 274;
        private const int F3 = 284;
        private static bool _resolved;
        private static MethodInfo? _getKeyDown;

        internal static bool DownArrowPressed() => KeyPressed(DownArrow);
        internal static bool F3Pressed() => KeyPressed(F3);

        private static bool KeyPressed(int keyCode)
        {
            Resolve();
            if (_getKeyDown == null) return false;

            try
            {
                var parameterType = _getKeyDown.GetParameters()[0].ParameterType;
                object key = parameterType == typeof(int)
                    ? keyCode
                    : Enum.ToObject(parameterType, keyCode);
                return (bool)_getKeyDown.Invoke(null, new[] { key })!;
            }
            catch { return false; }
        }

        private static void Resolve()
        {
            if (_resolved) return;
            _resolved = true;

            try
            {
                var assemblyDir = Path.GetDirectoryName(typeof(Il2Cpp.nbCalc).Assembly.Location)!;
                Assembly.LoadFrom(Path.Combine(assemblyDir, "UnityEngine.InputLegacyModule.dll"));

                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    var inputType = assembly.GetType("UnityEngine.Input", false);
                    if (inputType == null) continue;

                    foreach (var method in inputType.GetMethods(BindingFlags.Public | BindingFlags.Static))
                    {
                        if (method.Name != "GetKeyDown" || method.GetParameters().Length != 1) continue;
                        var parameterType = method.GetParameters()[0].ParameterType;
                        if (parameterType == typeof(int) || parameterType.IsEnum)
                        {
                            _getKeyDown = method;
                            return;
                        }
                    }
                }
            }
            catch { }
        }
    }
}
