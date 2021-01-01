Windows: [![Windows master Build Status](https://ci.appveyor.com/api/projects/status/otm2yg0os9rpn5j1?branch/master?svg=true "Windows master Build Status")](https://ci.appveyor.com/project/jmp75/dynamic-interop-dll/branch/master) Linux: [![Linux master Build Status](https://travis-ci.org/rdotnet/dynamic-interop-dll.svg?branch=master "Linux master Build Status")](https://travis-ci.org/rdotnet/dynamic-interop-dll/builds)

[![Nuget Version](https://buildstats.info/nuget/DynamicInterop)](https://www.nuget.org/packages/DynamicInterop/)

dynamic-interop-dll
===================

Facilities to load native DLLs from .NET, on Unix, Windows or MacOS.

# Purpose

This library is designed to facilitate load dynamic link libraries (called shared libraries on unix-type of platforms) from `.NET`, and the interop between the two worlds. The loading mechanism adapts to the operating system it is running on. It is an offshoot from the [R.NET](http://rdotnet.codeplex.com) project (source code now hosted [on github](https://github.com/rdotnet/rdotnet)). 


```c
MY_C_API_MARKER void Play(void* simulation, const char* variableIdentifier, double* values, TimeSeriesGeometry* geom);
```

```c#
    [DllImport("libswift.dll", EntryPoint = "Play", CallingConvention = CallingConvention.Cdecl)]
    private static extern void Play(
        [In] IntPtr modelSimulation,
        [In] string variableIdentifier,
        [In] [MarshalAs(UnmanagedType.LPArray)] double[] values,
        ref MarshaledTimeSeriesGeometry geom);
```

```c#
    public void Play(IModelSimulation modelSimulation, string variableIdentifier, MinimalTimeSeries series)
    {
        var handle = modelSimulation.DangerousGetHandle();
        var geom = new MarshaledTimeSeriesGeometry(series);
        MsDotnetNativeApi.Play(handle, variableIdentifier, series.Data, ref geom);
    }
```

```c#
    private delegate void Play_csdelegate(IntPtr simulation, string variableIdentifier, IntPtr values, IntPtr geom);
    // and somewhere in a class a field is set:
    NativeLib = new UnmanagedDll(someNativeLibFilename);

    void Play_cs(IModelSimulation simulation, string variableIdentifier, double[] values, ref MarshaledTimeSeriesGeometry geom)
    {
        IntPtr values_doublep, geom_struct;
        // here glue code here to create native arrays/structs
        NativeLib.GetFunction<Play_csdelegate>("Play")(CheckedDangerousGetHandle(simulation, "simulation"), variableIdentifier, values_doublep, geom_struct);
        // here copy args back to managed; clean up transient native resources.
    }
```

# License

This library is covered as of version 0.7.3 by the [MIT license](https://github.com/rdotnet/dynamic-interop-dll/blob/3055f27f46d1b794572bcd944eaebbd4f960b9a6/License.txt).

# Installation

You can install this library in your project(s) [as a NuGet package](https://www.nuget.org/packages/DynamicInterop)

For some Linux flavours you may need to also compile a [workaround for a "invalid caller" error](#libdl-workaround). This has been observed on some CentOS flavours. Debian, Ubuntu appear fine.

# Usage

An extract from the unit tests:

```c#
private void TestLoadKernel32()
{
    using (var dll = new UnmanagedDll("kernel32.dll"))
    {
        var beep = dll.GetFunction<Beep>();
        Assert.NotNull(beep);
        //beep(400, 1000);
        var areFileApisAnsi = dll.GetFunction<AreFileApisANSI>();
        Assert.DoesNotThrow(() => areFileApisAnsi());
    }
}

private delegate bool Beep(uint dwFreq, uint dwDuration);
private delegate bool AreFileApisANSI();
```

# Building

The command line for building and testing are largely platform neutral. you will need `dotnet` to build and optionally `cmake` to compile and run some of the unit tests, on Linux.

Follow the instructions for your platform to install `dotnet` if need be via [Download .NET](https://www.microsoft.com/net/download). Which version of nuget you have may matter too (beware older versions coming from some deb repos), see [https://docs.microsoft.com/en-us/nuget/guides/install-nuget](https://docs.microsoft.com/en-us/nuget/guides/install-nuget) to retrieve the latest nuget.exe.

```bash
dotnet restore
# ignore the restore fail for DynamicInterop.Tests/test_native_library/test_native_library.vcxproj if you get any
dotnet build --configuration Debug --no-restore
```

## testing

if on Linux, you need to compile the test native library:

```bash
cd DynamicInterop.Tests
cd test_native_library
cmake -H. -Bbuild
cmake --build build -- -j3
# cmake --build build 
cd ../..
```

On Windows:

```bat
set DynamicInteropTestLibPath=C:\src\dynamic-interop-dll\x64\Debug
```

On Linux:

```bash
export DynamicInteropTestLibPath='/home/path/to/dynamic-interop-dll/DynamicInterop.Tests/test_native_library/build'
```

then you can run unit tests:

```bash
dotnet test DynamicInterop.Tests/DynamicInterop.Tests.csproj
```

If, in the output of the unit test, you see an 'invalid caller' message from the dlerror function, read on the next section. This issue is known to happen on some Linux distros.

## libdl workaround for Linux

On Linux, DynamicInterop calls the dlopen function in "libdl" via P/Invoke to try to load the shared native library, On at least one instance of one Linux flavour (CentOS), it fails and 'dlerror' returns the message 'invalid caller'. See https://rdotnet.codeplex.com/workitem/73 for detailed information. Why this is an "invalid caller" could not be determined. While the exact cause if this failure is unclear, a thin wrapper library around libdl.so works around this issue.

You build and install this workaround with the following commands:

```bash
DYNINTEROP_BIN_DIR=~/my/path/to/DynamicInteropBinaries
cd libdlwrap
make
less sample.config # skim the comments, for information
cp sample.config $DYNINTEROP_BIN_DIR/DynamicInterop.dll.config
cp libdlwrap.so  $DYNINTEROP_BIN_DIR/
```

## Building the nuget package

*This section is primarily a reminder to the package author.*

```bash
dotnet pack DynamicInterop/DynamicInterop.csproj --configuration Release --no-build --no-restore --output nupkgs
# Or for initial testing/debugging
dotnet pack DynamicInterop/DynamicInterop.csproj --configuration Debug --no-build --no-restore --output nupkgs
```

If you have an additional nuget package repository for tests:

```cmd
cp .\DynamicInterop\nupkgs\DynamicInterop.0.9.0-beta.nupkg c:\local\nuget
```

# Related work

I did notice that [a related library](https://github.com/Boyko-Karadzhov/Dynamic-Libraries) with some overlapping features has been released just a week ago (as of when I wrote this)... While I want to explore possibilities to merge these libraries, I have pressing needs to release present library release as a stand-alone package.

# Acknowledgements

* Kosei designed and implemented the original multi-platform library loading
* evolvedmicrobe contributed the first implementation that did not require OS-specific conditional compilation.
* Daniel Collins found the workaround necessary for the library loading to work some Linux platforms, via Mono.
* jmp75 refactored, tested, merged contributions and separated from the original R.NET implementation.
