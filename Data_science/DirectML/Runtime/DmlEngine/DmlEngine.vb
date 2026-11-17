' ------------------------------------------------------------------------
' 引擎组装入口（对位 ILCuda 的 CudaEngine）
' ------------------------------------------------------------------------

Imports Microsoft.VisualBasic.Computing.DirectML.Interop

Namespace Runtime

    ''' <summary>
    ''' DirectML 计算引擎：一个适配器、一个设备、一条执行上下文。
    ''' </summary>
    ''' <remarks>
    ''' 引擎本身不持有显存；显存由 <see cref="DmlGraph"/> 在编译期一次性布局，
    ''' 因为 DirectML 的绑定表在编译后就是稳定的。
    ''' </remarks>
    Public NotInheritable Class DmlEngine
        Implements IDisposable

        Private _disposed As Boolean

        ''' <summary>用已经创建好的设备构造引擎。</summary>
        Public Sub New(device As DmlDevice, Optional options As EngineOptions = Nothing)
            If device Is Nothing Then
                Throw New ArgumentNullException(NameOf(device))
            End If

            Me.Device = device
            Me.Options = If(options, New EngineOptions())
            Me.Context = New DmlExecutionContext(device)
        End Sub

        ''' <summary>设备。</summary>
        Public ReadOnly Property Device As DmlDevice

        ''' <summary>执行上下文。</summary>
        Public ReadOnly Property Context As DmlExecutionContext

        ''' <summary>组装选项。</summary>
        Public ReadOnly Property Options As EngineOptions

        ''' <summary>创建一个新的计算图。</summary>
        Public Function CreateGraph() As DmlGraph
            Return New DmlGraph(Me)
        End Function

        ''' <summary>查询显存信息。</summary>
        Public Function GetMemoryInfo() As DmlMemoryInfo
            Dim info As New DmlMemoryInfo With {
                .DedicatedVideoMemory = Device.Adapter.DedicatedVideoMemory
            }

            Dim budget As ULong

            If Device.Adapter.TryGetBudget(budget) Then
                info.Budget = budget
            End If

            Return info
        End Function

        ''' <summary>
        ''' 尝试组装引擎；失败时不抛异常，而是返回 Nothing 并写入 <paramref name="options"/>.ErrorMessage。
        ''' </summary>
        Public Shared Function TryCreate(Optional options As EngineOptions = Nothing) As DmlEngine
            If options Is Nothing Then
                options = New EngineOptions()
            End If

            Try
                Return Create(options)
            Catch ex As Exception
                options.ErrorMessage = ex.Message

                If options.Candidates Is Nothing Then
                    options.Candidates = New List(Of DmlAdapter)()
                End If

                Return Nothing
            End Try
        End Function

        ''' <summary>
        ''' 组装引擎；失败时抛出异常。
        ''' </summary>
        Public Shared Function Create(Optional options As EngineOptions = Nothing) As DmlEngine
            If options Is Nothing Then
                options = New EngineOptions()
            End If

            If Not String.IsNullOrEmpty(options.DirectMLDirectory) Then
                DmlRuntime.ExtraSearchDirectories = New String() {options.DirectMLDirectory}
            End If

            If Not DmlRuntime.Probe() Then
                Throw New DmlException(-2147467259, "DirectML runtime", DmlRuntime.LastError)
            End If

            Dim adapter As DmlAdapter = options.Adapter
            Dim candidates As List(Of DmlAdapter) = Nothing

            If adapter Is Nothing Then
                candidates = DmlAdapter.Enumerate()
                options.Candidates = candidates

                If candidates.Count = 0 Then
                    Throw New DmlException(-2147467259, "DmlAdapter.Enumerate", "系统中没有可用于 D3D12/DirectML 的适配器")

                    Return Nothing
                End If

                Dim index As Integer = System.Math.Max(0, System.Math.Min(options.AdapterIndex, candidates.Count - 1))

                adapter = candidates(index)
            End If

            Dim device As DmlDevice = DmlDevice.Create(adapter, options.EnableDebugLayer, options.MinimumFeatureLevel)
            Dim engine As New DmlEngine(device, options)

            Return engine
        End Function

        Public Sub Dispose() Implements IDisposable.Dispose
            If _disposed Then
                Return
            End If

            _disposed = True

            If Context IsNot Nothing Then
                Context.Dispose()
            End If

            If Device IsNot Nothing Then
                Device.Dispose()
            End If

            If Options IsNot Nothing AndAlso Options.Candidates IsNot Nothing Then
                For Each adapter As DmlAdapter In Options.Candidates
                    adapter.Dispose()
                Next
            End If
        End Sub

        Public Overrides Function ToString() As String
            Return $"DML feature level {Device.FeatureLevelText} engine on {Device.Adapter.Description}"
        End Function

    End Class

End Namespace
