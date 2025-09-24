# Orendes.Ximea.ManagedResolver

A tiny helper package that makes **XIMEA’s xiAPI .NET** work smoothly across platforms and target frameworks.

- ✅ Ships the correct **managed** `xiApi.NETX64.dll` for `net8.0` and `net48`
- ✅ Adds a cross-platform **unmanaged resolver** so XIMEA’s hard-coded `xiapi64.dll` name also works on Linux/macOS (maps to `libm3api.so` / `libm3api.dylib`)
- ✅ Zero code changes in your app — just install the package

> Why? XIMEA’s .NET wrapper P/Invokes `xiapi64.dll`. That name is fine on Windows, but not on Linux/macOS where the native library is named differently. This package patches that gap.

---

## Features

- **Auto-select managed DLL per TFM**  
  The package includes XIMEA’s `xiApi.NETX64.dll` under:
  - `lib/net8.0/` (for .NET 7/8/9/10 apps as applicable)
  - `lib/net48/` (for .NET Framework 4.8 apps)

  NuGet chooses the correct one during restore.

- **Runtime native library resolver**  
  Hooks the load context so calls to `xiapi64.dll` are redirected to:
  - Windows: `xiapi64.dll` (as-is)
  - Linux: `libm3api.so` (or `m3api`)
  - macOS: `libm3api.dylib` (or `m3api`)

- **No boilerplate**  
  Uses a module initializer — you don’t need to call `Init()`.

---

## Install

```bash
dotnet add package Orendes.Ximea.ManagedResolver
```

Or via NuGet Package Manager.

> The package already contains XIMEA’s managed wrapper. You **don’t** need to add a separate reference to `xiApi.NETX64.dll`.

---

## Usage

```csharp
static class Program
{
  static void Main(string[] args)
  {
    XimeaNativeBootstrap.Init();   // must run BEFORE the first xiAPI call
  }
}
```

Once the Init Code is called, just use XIMEA’s API as usual:

```csharp
using xiApi;

unsafe
{
    int num;
    var err = ximeaApi.xiGetNumberDevices(&num);
    if (err != 0) throw new InvalidOperationException($"xiGetNumberDevices error {err}");
}
```

---

## Deployment notes

### Native library location

On **Windows**, the vendor name matches, so the OS loader finds `xiapi64.dll` normally (either from PATH or app folder).

On **Linux/macOS**, make sure the native library from the XIMEA SDK can be found. The resolver tries, in order:

1. System/default search (`ldconfig` paths on Linux, DYLD on macOS)  
2. App directory (`AppContext.BaseDirectory`)  
3. An optional override directory via environment variable:
   - `XIMEA_NATIVE_PATH=/opt/ximea` (for example)

### Target frameworks

- **.NET 7/8/9** → package references `lib/net8.0/xiApi.NETX64.dll`.  
- **.NET Framework 4.8** → package references `lib/net48/xiApi.NETX64.dll`.

The right managed DLL is picked automatically by NuGet based on your project’s TFM.

---

## License

- Resolver code: MIT (this package).
- XIMEA managed/native libraries: subject to XIMEA’s licenses. Ensure you have rights to redistribute/use them in your environment.

