' ------------------------------------------------------------------------
' 计算图里的一个张量：形状 + 主机端数据 + 显存切片
' ------------------------------------------------------------------------

Imports Microsoft.VisualBasic.Computing.DirectML.Interop

Namespace Runtime

    ''' <summary>张量在计算图中的角色。</summary>
    Public Enum DmlTensorKind As Integer

        ''' <summary>图的输入（每次执行前由调用方写入）。</summary>
        Input = 0

        ''' <summary>图的输出（每次执行后回读）。</summary>
        Output = 1

        ''' <summary>中间结果（只在显存里流动）。</summary>
        Intermediate = 2

        ''' <summary>常量（权重），编译期上传一次。</summary>
        Constant = 3

    End Enum

    ''' <summary>
    ''' 计算图中的一个命名张量。
    ''' </summary>
    Public NotInheritable Class DmlGraphTensor

        Friend Sub New(name As String, tensor As DmlTensor, kind As DmlTensorKind)
            Me.Name = name
            Me.Tensor = tensor
            Me.Kind = kind
        End Sub

        ''' <summary>张量名称（通常是 ONNX 里的值名）。</summary>
        Public ReadOnly Property Name As String

        ''' <summary>形状描述。</summary>
        Public ReadOnly Property Tensor As DmlTensor

        ''' <summary>图角色。</summary>
        Public Property Kind As DmlTensorKind

        ''' <summary>主机端数据（FP32）。</summary>
        Public Property Data As Single()

        ''' <summary>主机端原始字节（非 FP32 类型时使用）。</summary>
        Public Property RawData As Byte()

        ''' <summary>显存切片（编译后有效）。</summary>
        Public Property Allocation As DmlAllocation

        ''' <summary>
        ''' 共享显存的宿主张量：非空时本张量与宿主使用同一段显存，
        ''' 但可以用不同的形状/步长描述它（用于转置等零拷贝视图）。
        ''' </summary>
        Public Property AllocationOwner As DmlGraphTensor

        ''' <summary>在上传堆中的偏移。</summary>
        Public Property UploadOffset As ULong

        ''' <summary>是否需要在下次执行时上传。</summary>
        Public Property StagingDirty As Boolean

        ''' <summary>在回读堆中的偏移。</summary>
        Public Property ReadbackOffset As ULong = ULong.MaxValue

        ''' <summary>
        ''' 写入主机端数据（下一次 <see cref="DmlGraph.Execute"/> 时会上传到显存）。
        ''' </summary>
        Public Sub SetData(values As Single())
            If values Is Nothing Then
                Throw New ArgumentNullException(NameOf(values))
            End If

            If CULng(values.Length) <> Tensor.ElementCount Then
                Throw New ArgumentException(
                    $"张量 ""{Name}"" 需要 {Tensor.ElementCount} 个元素，但提供了 {values.Length} 个", NameOf(values))
            End If

            Me.Data = values
            Me.RawData = Nothing
            Me.StagingDirty = True
        End Sub

        ''' <summary>
        ''' 写入原始字节（用于 INT32 / UINT8 / BOOL 张量）。
        ''' </summary>
        Public Sub SetRawData(bytes As Byte())
            If bytes Is Nothing Then
                Throw New ArgumentNullException(NameOf(bytes))
            End If

            If CULng(bytes.Length) <> Tensor.TotalSizeInBytes Then
                Throw New ArgumentException(
                    $"张量 ""{Name}"" 需要 {Tensor.TotalSizeInBytes} 字节，但提供了 {bytes.Length} 字节", NameOf(bytes))
            End If

            Me.RawData = bytes
            Me.Data = Nothing
            Me.StagingDirty = True
        End Sub

        ''' <summary>取得主机端数据（输出张量在执行后可用）。</summary>
        Public Function GetData() As Single()
            Return Data
        End Function

        ''' <summary>
        ''' 隐式转成算子工厂需要的 <see cref="DmlTensor"/> 形状描述。
        ''' </summary>
        Public Shared Widening Operator CType(value As DmlGraphTensor) As DmlTensor
            If value Is Nothing Then
                Return Nothing
            End If

            Return value.Tensor
        End Operator

        Public Overrides Function ToString() As String
            Return $"{Kind} {Name}: {Tensor}"
        End Function

    End Class

End Namespace
