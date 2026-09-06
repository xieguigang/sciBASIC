' ------------------------------------------------------------------------
' NVRTC（CUDA 运行时编译库）的动态绑定封装
'
' 为什么不使用固定的 DllImport：
'   NVRTC 的动态库文件名随 CUDA 工具包版本变化（nvrtc64_130_0.dll、
'   nvrtc64_120_0.dll、nvrtc64_112_0.dll ...），必须先用 NativeLibrary
'   载入实际存在的文件，再用 GetProcAddress + Marshal 取得委托，
'   才能在同一个进程内尝试多个版本的 NVRTC。
' ------------------------------------------------------------------------

Imports System.Runtime.InteropServices
Imports System.Text

Public Module NvrtcApi

    ' --------- NVRTC 函数签名（均为 __cdecl，字符串按 ANSI 封送） ---------

    <UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet:=CharSet.Ansi)>
    Public Delegate Function FnNvrtcVersion(ByRef major As Integer, ByRef minor As Integer) As nvrtcResult

    <UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet:=CharSet.Ansi)>
    Public Delegate Function FnNvrtcCreateProgram(ByRef program As IntPtr,
                                                  source As String,
                                                  name As String,
                                                  numHeaders As Integer,
                                                  headers As IntPtr(),
                                                  includeNames As IntPtr()) As nvrtcResult

    <UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet:=CharSet.Ansi)>
    Public Delegate Function FnNvrtcDestroyProgram(ByRef program As IntPtr) As nvrtcResult

    <UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet:=CharSet.Ansi)>
    Public Delegate Function FnNvrtcCompileProgram(program As IntPtr, numOptions As Integer, options As String()) As nvrtcResult

    <UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet:=CharSet.Ansi)>
    Public Delegate Function FnNvrtcGetProgramLogSize(program As IntPtr, ByRef logSize As ULong) As nvrtcResult

    <UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet:=CharSet.Ansi)>
    Public Delegate Function FnNvrtcGetProgramLog(program As IntPtr, log As StringBuilder) As nvrtcResult

    <UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet:=CharSet.Ansi)>
    Public Delegate Function FnNvrtcGetPtxSize(program As IntPtr, ByRef ptxSize As ULong) As nvrtcResult

    <UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet:=CharSet.Ansi)>
    Public Delegate Function FnNvrtcGetPtx(program As IntPtr, ptx As Byte()) As nvrtcResult

    <UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet:=CharSet.Ansi)>
    Public Delegate Function FnNvrtcGetCubinSize(program As IntPtr, ByRef cubinSize As ULong) As nvrtcResult

    <UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet:=CharSet.Ansi)>
    Public Delegate Function FnNvrtcGetCubin(program As IntPtr, cubin As Byte()) As nvrtcResult

    ''' <summary>
    ''' NVRTC 在编译时会通过 LoadLibrary 载入同目录下的 nvrtc-builtins64_*.dll，
    ''' 而默认的动态库搜索路径并不包含"NVRTC 动态库自身所在目录"，
    ''' 因此这里需要先把该目录加入搜索路径（Windows: SetDllDirectory）。
    ''' </summary>
    Friend Class NativeSearchPath

        <DllImport("kernel32.dll", CharSet:=CharSet.Auto, SetLastError:=True)>
        Private Shared Function SetDllDirectory(folder As String) As Boolean
        End Function

        Public Shared Sub AddFolder(folder As String)
            If String.IsNullOrWhiteSpace(folder) Then Return
            If Not RuntimeInformation.IsOSPlatform(OSPlatform.Windows) Then Return

            Try
                SetDllDirectory(folder)
            Catch
            End Try
        End Sub
    End Class

End Module
