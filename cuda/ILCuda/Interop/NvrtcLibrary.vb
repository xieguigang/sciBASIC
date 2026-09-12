#Region "Microsoft.VisualBasic::bf57cddd59ecd0ab9c16190611d29901, cuda\ILCuda\Interop\NvrtcLibrary.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 217
    '    Code Lines: 162 (74.65%)
    ' Comment Lines: 27 (12.44%)
    '    - Xml Docs: 88.89%
    ' 
    '   Blank Lines: 28 (12.90%)
    '     File Size: 9.07 KB


    ' Class NvrtcLibrary
    ' 
    '     Properties: FilePath, Major, Minor, SupportsCubin, VersionKey
    '                 VersionText
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: Bind, Compile, GuessMajor, GuessMinor, TryLoad
    '               VersionNumberFromFileName
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.InteropServices
Imports System.Text

''' <summary>
''' 已经绑定好函数入口的 NVRTC 动态库实例
''' </summary>
Public NotInheritable Class NvrtcLibrary

    Private ReadOnly _handle As IntPtr
    Private ReadOnly _version As FnNvrtcVersion
    Private ReadOnly _create As FnNvrtcCreateProgram
    Private ReadOnly _destroy As FnNvrtcDestroyProgram
    Private ReadOnly _compile As FnNvrtcCompileProgram
    Private ReadOnly _logSize As FnNvrtcGetProgramLogSize
    Private ReadOnly _log As FnNvrtcGetProgramLog
    Private ReadOnly _ptxSize As FnNvrtcGetPtxSize
    Private ReadOnly _ptx As FnNvrtcGetPtx
    Private ReadOnly _cubinSize As FnNvrtcGetCubinSize
    Private ReadOnly _cubin As FnNvrtcGetCubin

    ''' <summary>动态库文件全路径</summary>
    Public ReadOnly Property FilePath As String
    ''' <summary>NVRTC 主版本号（例如 13）</summary>
    Public ReadOnly Property Major As Integer
    ''' <summary>NVRTC 次版本号（例如 0）</summary>
    Public ReadOnly Property Minor As Integer
    ''' <summary>形如 13.0 的版本文本</summary>
    Public ReadOnly Property VersionText As String

    Private Sub New(handle As IntPtr, filePath As String, major As Integer, minor As Integer)
        Me._handle = handle
        Me.FilePath = filePath
        Me.Major = major
        Me.Minor = minor
        Me.VersionText = $"{major}.{minor}"

        _version = Bind(Of FnNvrtcVersion)(handle, "nvrtcVersion", required:=False)
        _create = Bind(Of FnNvrtcCreateProgram)(handle, "nvrtcCreateProgram")
        _destroy = Bind(Of FnNvrtcDestroyProgram)(handle, "nvrtcDestroyProgram")
        _compile = Bind(Of FnNvrtcCompileProgram)(handle, "nvrtcCompileProgram")
        _logSize = Bind(Of FnNvrtcGetProgramLogSize)(handle, "nvrtcGetProgramLogSize")
        _log = Bind(Of FnNvrtcGetProgramLog)(handle, "nvrtcGetProgramLog")
        _ptxSize = Bind(Of FnNvrtcGetPtxSize)(handle, "nvrtcGetPTXSize")
        _ptx = Bind(Of FnNvrtcGetPtx)(handle, "nvrtcGetPTX")
        _cubinSize = Bind(Of FnNvrtcGetCubinSize)(handle, "nvrtcGetCUBINSize", required:=False)
        _cubin = Bind(Of FnNvrtcGetCubin)(handle, "nvrtcGetCUBIN", required:=False)

        If _version IsNot Nothing Then
            Dim ma As Integer = 0, mi As Integer = 0
            If _version(ma, mi) = nvrtcResult.NVRTC_SUCCESS AndAlso ma > 0 Then
                Me.Major = ma
                Me.Minor = mi
                Me.VersionText = $"{ma}.{mi}"
            End If
        End If
    End Sub

    ''' <summary>用于与驱动版本比较的整数键：13.0 -> 130，11.2 -> 112</summary>
    Public ReadOnly Property VersionKey As Integer
        Get
            Return Major * 10 + Minor
        End Get
    End Property

    Private Shared Function Bind(Of TDelegate As Class)(handle As IntPtr, symbol As String, Optional required As Boolean = True) As TDelegate
        Dim addr As IntPtr
        Try
            addr = NativeLibrary.GetExport(handle, symbol)
        Catch ex As EntryPointNotFoundException
            If required Then Throw
            Return Nothing
        End Try
        Return Marshal.GetDelegateForFunctionPointer(Of TDelegate)(addr)
    End Function

    ''' <summary>
    ''' 动态载入指定路径的 NVRTC 动态库
    ''' </summary>
    Public Shared Function TryLoad(filePath As String, ByRef library As NvrtcLibrary, ByRef errorMessage As String) As Boolean
        library = Nothing
        errorMessage = Nothing

        If String.IsNullOrWhiteSpace(filePath) OrElse Not IO.File.Exists(filePath) Then
            errorMessage = "文件不存在"
            Return False
        End If

        Dim handle As IntPtr = IntPtr.Zero
        Try
            ' 先把 NVRTC 所在目录加入动态库搜索路径，否则它找不到 nvrtc-builtins64_*.dll
            NativeSearchPath.AddFolder(IO.Path.GetDirectoryName(filePath))

            If Not NativeLibrary.TryLoad(filePath, handle) Then
                errorMessage = "NativeLibrary.TryLoad 失败（可能缺少依赖库或位数不匹配）"
                Return False
            End If
        Catch ex As Exception
            errorMessage = ex.Message
            Return False
        End Try

        Try
            library = New NvrtcLibrary(handle, filePath, GuessMajor(filePath), GuessMinor(filePath))
            Return True
        Catch ex As Exception
            errorMessage = ex.Message
            NativeLibrary.Free(handle)
            Return False
        End Try
    End Function

    Private Shared Function GuessMajor(path As String) As Integer
        Dim n = VersionNumberFromFileName(path)
        If n <= 0 Then Return 0
        Return n \ 10
    End Function

    Private Shared Function GuessMinor(path As String) As Integer
        Dim n = VersionNumberFromFileName(path)
        If n <= 0 Then Return 0
        Return n Mod 10
    End Function

    ''' <summary>从 nvrtc64_112_0.dll 这样的文件名中解析出 112</summary>
    Public Shared Function VersionNumberFromFileName(path As String) As Integer
        Dim name = IO.Path.GetFileNameWithoutExtension(path).ToLowerInvariant()
        Dim parts = name.Split("_"c)
        If parts.Length >= 2 Then
            Dim n As Integer
            If Integer.TryParse(parts(1), n) Then Return n
        End If
        Return 0
    End Function

    ''' <summary>是否支持直接产出 cubin（nvrtcGetCUBIN，CUDA 11.1+）</summary>
    Public ReadOnly Property SupportsCubin As Boolean
        Get
            Return _cubinSize IsNot Nothing AndAlso _cubin IsNot Nothing
        End Get
    End Property

    ''' <summary>
    ''' 把 CUDA C 源码编译为 GPU 镜像。
    ''' 当 -arch 指定的是真实架构（sm_XX）时，NVRTC 会内部调用 ptxas 直接产出
    ''' cubin（SASS）；指定虚拟架构（compute_XX）时产出 PTX，由驱动 JIT 编译。
    ''' </summary>
    ''' <param name="source">CUDA C 源码</param>
    ''' <param name="programName">编译单元名称（仅用于日志）</param>
    ''' <param name="options">编译选项，例如 -arch=sm_86 / -arch=compute_86</param>
    ''' <param name="image">编译成功时返回的镜像（cubin 或 PTX）</param>
    ''' <param name="isCubin">返回的镜像是否为 cubin</param>
    ''' <param name="log">NVRTC 编译日志</param>
    Public Function Compile(source As String, programName As String, options As String(),
                            ByRef image As Byte(), ByRef isCubin As Boolean, ByRef log As String) As nvrtcResult
        image = Nothing
        isCubin = False
        log = String.Empty

        NativeSearchPath.AddFolder(IO.Path.GetDirectoryName(FilePath))

        Dim program As IntPtr = IntPtr.Zero
        Dim result = _create(program, source, programName, 0, Nothing, Nothing)
        If result <> nvrtcResult.NVRTC_SUCCESS Then
            log = $"nvrtcCreateProgram 失败: {result}"
            Return result
        End If

        Try
            result = _compile(program, options.Length, options)

            ' 无论成功失败都取回日志，失败时用于定位问题
            Dim size As ULong = 0
            If _logSize(program, size) = nvrtcResult.NVRTC_SUCCESS AndAlso size > 0 Then
                Dim buffer As New StringBuilder(CInt(size) + 2)
                If _log(program, buffer) = nvrtcResult.NVRTC_SUCCESS Then
                    log = buffer.ToString().Trim()
                End If
            End If

            If result <> nvrtcResult.NVRTC_SUCCESS Then
                Return result
            End If

            ' 优先取 cubin（saSS），取不到再回退到 PTX
            If SupportsCubin Then
                Dim cubinSize As ULong = 0
                If _cubinSize(program, cubinSize) = nvrtcResult.NVRTC_SUCCESS AndAlso cubinSize > 0 Then
                    Dim cubin = New Byte(CInt(cubinSize) - 1) {}
                    result = _cubin(program, cubin)
                    If result <> nvrtcResult.NVRTC_SUCCESS Then Return result

                    image = cubin
                    isCubin = True
                    Return nvrtcResult.NVRTC_SUCCESS
                End If
            End If

            Dim ptxSize As ULong = 0
            result = _ptxSize(program, ptxSize)
            If result <> nvrtcResult.NVRTC_SUCCESS Then Return result
            If ptxSize = 0 Then
                Return nvrtcResult.NVRTC_ERROR_INTERNAL_ERROR
            End If

            Dim ptx = New Byte(CInt(ptxSize) - 1) {}
            result = _ptx(program, ptx)
            If result <> nvrtcResult.NVRTC_SUCCESS Then Return result

            image = ptx
            isCubin = False
            Return nvrtcResult.NVRTC_SUCCESS
        Finally
            Dim p As IntPtr = program
            _destroy(p)
        End Try
    End Function
End Class
