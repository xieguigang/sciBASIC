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
        Public ReadOnly Property Kind As DmlTensorKind

        ''' <summary>主机端数据（FP32）。</summary>
        Public Property Data As Single()

        ''' <summary>显存切片（编译后有效）。</summary>
        Public Property Allocation As DmlAllocation

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
            Me.StagingDirty = True
        End Sub

        ''' <summary>取得主机端数据（输出张量在执行后可用）。</summary>
        Public Function GetData() As Single()
            Return Data
        End Function

        Public Overrides Function ToString() As String
            Return $"{Kind} {Name}: {Tensor}"
        End Function

    End Class

End Namespace
