namespace XimeaCameraDriverAndResolver
{

    using System;
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;

    public static class XimeaNativeBootstrap
    {
        private static bool _initialized = false;

        public static void Init()
        {
            if (_initialized) return;
        #if NETCOREAPP
            NativeLibrary.SetDllImportResolver(typeof(xiApi.PRM_TYPE).Assembly, Resolve);
        #endif
            _initialized = true;
        }

        #if NETCOREAPP
        private static IntPtr Resolve(string libraryName, Assembly asm, DllImportSearchPath? paths)
        {
            // They hard-coded "xiapi64.dll" in [DllImport]. Intercept that (and the 32-bit name)
            if (!string.Equals(libraryName, "xiapi64.dll", StringComparison.OrdinalIgnoreCase)) return IntPtr.Zero; // let normal resolution handle other libs

            // Candidate names by OS
            string[] candidates =
                OperatingSystem.IsWindows()
                    ? new[]
                    {
                        "xiapi64.dll", "xiapi32.dll", "xiapi.dll"
                    } // try both; some vendors ship a neutral name too
                    : OperatingSystem.IsLinux()
                        ? new[]
                        {
                            "libm3api.so", "m3api", "libxiapi.so"
                        } // common Linux patterns
                        : OperatingSystem.IsMacOS()
                            ? new[]
                            {
                                "libm3api.dylib", "m3api", "libxiapi.dylib"
                            }
                            : Array.Empty<string>();

            // 1) Try default loader search for each candidate
            foreach (var name in candidates)
                if (NativeLibrary.TryLoad(name, asm, paths, out var handle))
                    return handle;

            // 2) Try next to the app (self-contained deployments)
            foreach (var name in candidates)
            {
                var local = Path.Combine(AppContext.BaseDirectory, name);
                if (File.Exists(local))
                    return NativeLibrary.Load(local);
            }

            // 3) Last resort: try a known install path you control (env var, config, etc.)
            var env = Environment.GetEnvironmentVariable("XIMEA_NATIVE_PATH");
            if (!string.IsNullOrEmpty(env))
            {
                foreach (var name in candidates)
                {
                    var p = Path.Combine(env, name);
                    if (File.Exists(p))
                        return NativeLibrary.Load(p);
                }
            }

            return IntPtr.Zero; // fall back; P/Invoke will throw if still unresolved
        }
        #endif
    }
}
