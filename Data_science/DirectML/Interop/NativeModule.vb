' ------------------------------------------------------------------------
' 原生动态库的探测与导出绑定
'
' DirectML.dll 的来源在不同机器上差异很大：
'   * Windows 10 1903 之后由系统通过 DirectX 更新下发（System32）；
'   * 也可以从 Microsoft.AI.DirectML 的分发目录旁加载到程序目录；
'   * 有些环境通过环境变量显式指定。
' 因此这里按"程序目录 → 指定目录 → System32 → 默认搜索路径"的顺序探测，
' 并把每一次尝试记录下来交给诊断层，而不是直接抛 DllNotFoundException。
' ------------------------------------------------------------------------

Imports System.IO
Imports System.Reflection
Imports System.Runtime.InteropServices

Namespace Interop

    ''' <summary>一次原生库加载尝试的记录。</summary>
    Public NotInheritable Class NativeProbeAttempt

        ''' <summary>尝试的路径或模块名。</summary>
        Public ReadOnly Property Path As String

        ''' <summary>是否成功。</summary>
        Public ReadOnly Property Succeeded As Boolean

        ''' <summary>失败原因。</summary>
        Public ReadOnly Property ErrorMessage As String

        Public Sub New(path As String, succeeded As Boolean, Optional errorMessage As String = Nothing)
            Me.Path = path
            Me.Succeeded = succeeded
            Me.ErrorMessage = errorMessage
        End Sub

        Public Overrides Function ToString() As String
            If Succeeded Then
                Return $"[OK]   {Path}"
            End If

            Return $"[FAIL] {Path}：{ErrorMessage}"
        End Function

    End Class

    ''' <summary>
    ''' 一个已加载的原生模块：持有模块句柄并提供导出函数的绑定。
    ''' </summary>
    Public NotInheritable Class NativeModule
        Implements IDisposable

        Private ReadOnly _handle As IntPtr
        Private ReadOnly _name As String
        Private ReadOnly _path As String
        Private ReadOnly _exports As New Dictionary(Of String, IntPtr)(StringComparer.Ordinal)
        Private _disposed As Boolean

        Private Sub New(handle As IntPtr, name As String, path As String)
            Me._handle = handle
            Me._name = name
            Me._path = path
        End Sub

        ''' <summary>模块名（例如 <c>DirectML.dll</c>）。</summary>
        Public ReadOnly Property Name As String
            Get
                Return _name
            End Get
        End Property

        ''' <summary>实际加载的路径。</summary>
        Public ReadOnly Property FilePath As String
            Get
                Return _path
            End Get
        End Property

        ''' <summary>模块句柄。</summary>
        Public ReadOnly Property Handle As IntPtr
            Get
                Return _handle
            End Get
        End Property

        ''' <summary>
        ''' 探测并加载一个原生模块。
        ''' </summary>
        ''' <param name="moduleName">模块文件名，例如 <c>DirectML.dll</c>。</param>
        ''' <param name="attempts">尝试记录（会追加）。</param>
        ''' <param name="extraDirectories">额外的优先搜索目录。</param>
        ''' <returns>加载成功的模块；全部失败时返回 Nothing。</returns>
        Public Shared Function TryLoad(moduleName As String,
                                      attempts As IList(Of NativeProbeAttempt),
                                      Optional extraDirectories As String() = Nothing) As NativeModule

            If attempts Is Nothing Then
                Throw New ArgumentNullException(NameOf(attempts))
            End If

            Dim candidates As New List(Of String)

            Call candidates.Add(Path.Combine(AppContext.BaseDirectory, moduleName))

            If extraDirectories IsNot Nothing Then
                For Each dir As String In extraDirectories
                    If Not String.IsNullOrEmpty(dir) Then
                        Call candidates.Add(Path.Combine(dir, moduleName))
                    End If
                Next
            End If

            Dim system32 As String = Environment.GetFolderPath(Environment.SpecialFolder.System)

            If Not String.IsNullOrEmpty(system32) Then
                Call candidates.Add(Path.Combine(system32, moduleName))
            End If

            Call candidates.Add(moduleName)

            Dim seen As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)

            For Each candidate As String In candidates
                If Not seen.Add(candidate) Then
                    Continue For
                End If

                Dim handle As IntPtr

                If NativeLibrary.TryLoad(candidate, handle) Then
                    Call attempts.Add(New NativeProbeAttempt(candidate, True))

                    Return New NativeModule(handle, moduleName, candidate)
                End If

                Call attempts.Add(New NativeProbeAttempt(candidate, False, "NativeLibrary.TryLoad 失败"))
            Next

            Return Nothing
        End Function

        ''' <summary>
        ''' 取得导出函数的指针。
        ''' </summary>
        ''' <param name="entryPoint">导出名。</param>
        ''' <returns>函数指针；不存在时返回 <see cref="IntPtr.Zero"/>。</returns>
        Public Function TryGetExport(entryPoint As String) As IntPtr
            If _disposed Then
                Throw New ObjectDisposedException(NameOf(NativeModule))
            End If

            Dim ptr As IntPtr

            If _exports.TryGetValue(entryPoint, ptr) Then
                Return ptr
            End If

            If Not NativeLibrary.TryGetExport(_handle, entryPoint, ptr) Then
                ptr = IntPtr.Zero
            End If

            _exports(entryPoint) = ptr

            Return ptr
        End Function

        ''' <summary>
        ''' 取得导出函数并绑定为委托。
        ''' </summary>
        ''' <typeparam name="TDelegate">委托类型。</typeparam>
        ''' <param name="entryPoint">导出名。</param>
        ''' <param name="[optional]">导出缺失时是否允许返回 Nothing。</param>
        ''' <returns>委托实例。</returns>
        Public Function GetDelegate(Of TDelegate As Class)(entryPoint As String, Optional [optional] As Boolean = False) As TDelegate
            Dim ptr As IntPtr = TryGetExport(entryPoint)

            If ptr = IntPtr.Zero Then
                If [optional] Then
                    Return Nothing
                End If

                Throw New EntryPointNotFoundException($"模块 {_name} 中找不到导出函数 {entryPoint}")
            End If

            Dim created As System.Delegate = Marshal.GetDelegateForFunctionPointer(ptr, GetType(TDelegate))

            Return DirectCast(CObj(created), TDelegate)
        End Function

        Public Sub Dispose() Implements IDisposable.Dispose
            If _disposed Then
                Return
            End If

            _disposed = True
            ' 进程退出前不主动 Free：D3D12/DirectML 的 COM 对象可能在释放阶段仍然
            ' 引用模块内的代码，提前卸载会导致释放路径崩溃。
        End Sub

        Public Overrides Function ToString() As String
            Return $"{_name} -> {_path}"
        End Function

    End Class

End Namespace
