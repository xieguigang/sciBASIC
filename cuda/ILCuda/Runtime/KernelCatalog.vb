' ------------------------------------------------------------------------
' 内核注册表
'
' 只保存"元数据"（内核名 / 来源 / 默认 block / 共享内存需求），
' 不参与 cuLaunchKernel 热路径，因此这里做字符串查找是安全的。
'
' 框架自带的内核在首次访问时自动预置；外部（demo / 业务方）注册进来的
' 内核也可以登记自己的默认启动参数。
' ------------------------------------------------------------------------

Namespace Runtime

    ''' <summary>一个内核的描述信息</summary>
    Public Class KernelInfo
        ''' <summary>与 .cu 中 extern "C" 名称一致的内核函数名</summary>
        Public Property Name As String
        ''' <summary>来源的 .cu 文件（例如 basic.cu / metrics.cu）</summary>
        Public Property Source As String
        ''' <summary>建议的每 block 线程数</summary>
        Public Property DefaultBlock As Integer = 256
        ''' <summary>建议的每 block 共享内存字节数（0 表示不需要动态共享内存）</summary>
        Public Property SharedMemory As Integer = 0
        ''' <summary>一句说明</summary>
        Public Property Description As String

        Public Sub New()
        End Sub

        Public Sub New(name As String, source As String, Optional defaultBlock As Integer = 256,
                       Optional sharedMemory As Integer = 0, Optional description As String = Nothing)
            Me.Name = name
            Me.Source = source
            Me.DefaultBlock = defaultBlock
            Me.SharedMemory = sharedMemory
            Me.Description = description
        End Sub

        Public Overrides Function ToString() As String
            Dim block = If(DefaultBlock > 0, $"block={DefaultBlock}", "block=?")
            Dim smem = If(SharedMemory > 0, $", smem={SharedMemory}B", "")

            Return $"{Name} ({Source}, {block}{smem})"
        End Function
    End Class

    ''' <summary>内核元数据注册表</summary>
    Public Module KernelCatalog

        Private ReadOnly _sync As New Object()
        Private ReadOnly _items As New Dictionary(Of String, KernelInfo)(StringComparer.Ordinal)
        Private _initialized As Boolean

        Private Sub EnsureBuiltin()
            If _initialized Then Return

            SyncLock _sync
                If _initialized Then Return

                AddCore(New KernelInfo(KernelNames.VecAdd, "basic.cu", 256, 0, "c = a + b"))
                AddCore(New KernelInfo(KernelNames.Saxpy, "basic.cu", 256, 0, "out = alpha * x + y"))

                AddCore(New KernelInfo(KernelNames.ReduceSum, "reduce.cu", 256, 1024, "归约第一阶段：求和"))
                AddCore(New KernelInfo(KernelNames.ReduceMax, "reduce.cu", 256, 1024, "归约第一阶段：最大值"))
                AddCore(New KernelInfo(KernelNames.ReduceMin, "reduce.cu", 256, 1024, "归约第一阶段：最小值"))
                AddCore(New KernelInfo(KernelNames.ReduceFinalSum, "reduce.cu", 256, 1024, "归约收尾：求和"))
                AddCore(New KernelInfo(KernelNames.ReduceFinalMax, "reduce.cu", 256, 1024, "归约收尾：最大值"))
                AddCore(New KernelInfo(KernelNames.ReduceFinalMin, "reduce.cu", 256, 1024, "归约收尾：最小值"))

                For Each name In New String() {
                    KernelNames.EwAdd, KernelNames.EwSub, KernelNames.EwMul, KernelNames.EwDiv,
                    KernelNames.EwScale, KernelNames.EwAxpy, KernelNames.EwRelu, KernelNames.EwExp,
                    KernelNames.EwLog, KernelNames.EwSqrt, KernelNames.EwAbs
                }
                    AddCore(New KernelInfo(name, "elementwise.cu", 256, 0, "逐元素算子"))
                Next

                AddCore(New KernelInfo(KernelNames.Gemm, "blas.cu", 256, 0, "16x16 分块矩阵乘（block 为 16x16）"))
                AddCore(New KernelInfo(KernelNames.Gemv, "blas.cu", 256, 1024, "矩阵-向量乘（每 block 一行）"))

                _initialized = True
            End SyncLock
        End Sub

        ''' <summary>登记（或覆盖）一个内核的描述信息</summary>
        Public Sub Add(info As KernelInfo)
            If info Is Nothing Then Throw New ArgumentNullException(NameOf(info))
            If String.IsNullOrWhiteSpace(info.Name) Then
                Throw New ArgumentException("内核名不能为空", NameOf(info))
            End If

            EnsureBuiltin()

            SyncLock _sync
                AddCore(info)
            End SyncLock
        End Sub

        ''' <summary>不做 EnsureBuiltin 的直接写入，仅供初始化路径使用</summary>
        Private Sub AddCore(info As KernelInfo)
            _items(info.Name) = info
        End Sub

        ''' <summary>查询内核描述，未登记时返回 False</summary>
        Public Function TryGet(name As String, ByRef info As KernelInfo) As Boolean
            info = Nothing
            If String.IsNullOrWhiteSpace(name) Then Return False

            EnsureBuiltin()

            SyncLock _sync
                Return _items.TryGetValue(name, info)
            End SyncLock
        End Function

        ''' <summary>是否已登记该内核</summary>
        Public Function Contains(name As String) As Boolean
            Dim info As KernelInfo = Nothing
            Return TryGet(name, info)
        End Function

        ''' <summary>
        ''' 取得内核描述；未登记时抛出列出全部已知内核的异常，便于排查名字写错的问题。
        ''' </summary>
        Public Function Require(name As String) As KernelInfo
            Dim info As KernelInfo = Nothing

            If TryGet(name, info) Then Return info

            Dim known = String.Join(", ", All().Select(Function(k) k.Name))
            Throw New KeyNotFoundException(
                $"内核 '{name}' 未在 KernelCatalog 中登记。已知内核：{known}")
        End Function

        ''' <summary>全部已登记内核的快照（按名称排序）</summary>
        Public Function All() As IReadOnlyList(Of KernelInfo)
            EnsureBuiltin()

            SyncLock _sync
                Return _items.Values.OrderBy(Function(k) k.Name, StringComparer.Ordinal).ToList()
            End SyncLock
        End Function

        ''' <summary>默认启动参数（内核未登记时退回 256 线程 / 无共享内存）</summary>
        Public Function DefaultBlockOf(name As String) As Integer
            Dim info As KernelInfo = Nothing
            If TryGet(name, info) AndAlso info.DefaultBlock > 0 Then Return info.DefaultBlock
            Return 256
        End Function

        ''' <summary>默认共享内存需求（字节）</summary>
        Public Function DefaultSharedMemoryOf(name As String) As Integer
            Dim info As KernelInfo = Nothing
            If TryGet(name, info) Then Return System.Math.Max(0, info.SharedMemory)
            Return 0
        End Function
    End Module
End Namespace
