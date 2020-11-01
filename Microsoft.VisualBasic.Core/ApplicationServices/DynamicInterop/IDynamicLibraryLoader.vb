Namespace ApplicationServices.DynamicInterop

    ''' <summary>
    ''' An interface definition to hide the platform specific aspects of library loading
    ''' </summary>
    ''' <remarks>There are probably projects 'out there' doing this already, but nothing 
    ''' is obvious from a quick search. Re-consider again if you need more features.</remarks>
    Public Interface IDynamicLibraryLoader
        ''' <summary>
        ''' Load a native library (DLL on windows, shared libraries on Linux and MacOS)
        ''' </summary>
        ''' <param name="filename">The file name (short file name) of the library to load, e.g. R.dll on Windows</param>
        ''' <returns></returns>
        Function LoadLibrary(ByVal filename As String) As IntPtr

        ''' <summary>
        ''' Gets the last error message from the native API used to load the library.
        ''' </summary>
        ''' <returns></returns>
        Function GetLastError() As String

        ''' <summary>
        ''' Unloads a library
        ''' </summary>
        ''' <param name="handle">The pointer to the entry point of the library</param>
        ''' <returns></returns>
        Function FreeLibrary(ByVal handle As IntPtr) As Boolean

        ''' <summary>
        ''' Gets a pointer to the address of a native function in the specified loaded library
        ''' </summary>
        ''' <param name="hModule">Handle of the module(library)</param>
        ''' <param name="lpProcName">The name of the function sought</param>
        ''' <returns>Handle to the native function</returns>
        Function GetFunctionAddress(ByVal hModule As IntPtr, ByVal lpProcName As String) As IntPtr
    End Interface
End Namespace
