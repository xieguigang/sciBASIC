' /********************************************************************************/
'  Microsoft.VisualBasic.MachineLearning.DataStorage
'  Data_science\MachineLearning\MLDataStorage\PT\PtStateDict.vb
' /********************************************************************************/

Imports System.Text
Imports Microsoft.VisualBasic.MachineLearning.TensorFlow

Namespace PT

    ''' <summary>
    ''' 权重字典中的一项：名称、张量，以及落盘时使用的元素类型与是否需要梯度。
    ''' </summary>
    Public Class PtTensorEntry

        ''' <summary>权重名称（对应 PyTorch <c>state_dict</c> 的键，例如 <c>conv.0.weight</c>）。</summary>
        Public Property Name As String

        ''' <summary>张量数值。</summary>
        Public Property Value As Tensor

        ''' <summary>落盘元素类型。</summary>
        Public Property ElementType As PtElementType

        ''' <summary>是否需要梯度（PyTorch <c>requires_grad</c>）。</summary>
        Public Property RequiresGrad As Boolean

        ''' <summary>张量形状（便于诊断，随 <see cref="Value"/> 变化）。</summary>
        Public ReadOnly Property Shape As Integer()
            Get
                Return If(Value Is Nothing, New Integer() {}, Value.Shape)
            End Get
        End Property

        Public Overrides Function ToString() As String
            Dim shape As String = String.Join(", ", Shape)

            Return $"{Name}: [{shape}] {PtDataType.DTypeName(ElementType)}"
        End Function

    End Class

    ''' <summary>
    ''' PyTorch <c>state_dict</c> 的对象视图：名称 → <see cref="Tensor"/>，并保留每个张量的
    ''' 元素类型与 <c>requires_grad</c> 标记，便于"读出 → 修改 → 原样写回"。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' 张量数值在内存中统一为双精度（框架的 <see cref="Tensor"/> 只支持双精度），只有落盘时才按
    ''' <see cref="PtTensorEntry.ElementType"/> 收窄或提升。因此对单精度权重做"读 → 写"往返是无损的。
    ''' </para>
    ''' <para>
    ''' 写出时每个张量独占一块存储，不会还原原始的存储共享 / 视图关系（数值本身无损）。
    ''' 这一限制在 <see cref="NamespaceDoc"/> 中有说明。
    ''' </para>
    ''' </remarks>
    Public Class PtStateDict

        Private ReadOnly _entries As New List(Of PtTensorEntry)()
        Private ReadOnly _index As New Dictionary(Of String, Integer)(StringComparer.Ordinal)

        ''' <summary>创建空字典。</summary>
        Public Sub New()
        End Sub

        ''' <summary>按名称获取张量。</summary>
        ''' <param name="name">权重名称</param>
        ''' <exception cref="KeyNotFoundException">名称不存在</exception>
        Default Public ReadOnly Property Item(name As String) As Tensor
            Get
                Return GetEntry(name).Value
            End Get
        End Property

        ''' <summary>权重项的数量。</summary>
        Public ReadOnly Property Count As Integer
            Get
                Return _entries.Count
            End Get
        End Property

        ''' <summary>全部权重项（保持写入 / 读取顺序）。</summary>
        Public ReadOnly Property Entries As IReadOnlyList(Of PtTensorEntry)
            Get
                Return _entries
            End Get
        End Property

        ''' <summary>全部权重名称（保持顺序）。</summary>
        Public ReadOnly Property Names As String()
            Get
                Return _entries.Select(Function(e) e.Name).ToArray()
            End Get
        End Property

        ''' <summary>名称 → 元素类型的映射。</summary>
        Public ReadOnly Property ElementTypes As IReadOnlyDictionary(Of String, PtElementType)
            Get
                Dim result As New Dictionary(Of String, PtElementType)(StringComparer.Ordinal)

                For Each entry As PtTensorEntry In _entries
                    result(entry.Name) = entry.ElementType
                Next

                Return result
            End Get
        End Property

        ''' <summary>是否包含指定名称。</summary>
        ''' <param name="name">权重名称</param>
        Public Function ContainsKey(name As String) As Boolean
            Return name IsNot Nothing AndAlso _index.ContainsKey(name)
        End Function

        ''' <summary>按名称获取权重项。</summary>
        ''' <param name="name">权重名称</param>
        ''' <exception cref="KeyNotFoundException">名称不存在</exception>
        Public Function GetEntry(name As String) As PtTensorEntry
            Dim position As Integer

            If name IsNot Nothing AndAlso _index.TryGetValue(name, position) Then
                Return _entries(position)
            End If

            Throw New KeyNotFoundException($"权重字典中不存在名称 '{name}'")
        End Function

        ''' <summary>尝试按名称获取权重项。</summary>
        ''' <param name="name">权重名称</param>
        ''' <param name="entry">输出参数：命中的权重项</param>
        Public Function TryGetEntry(name As String, ByRef entry As PtTensorEntry) As Boolean
            Dim position As Integer

            If name IsNot Nothing AndAlso _index.TryGetValue(name, position) Then
                entry = _entries(position)
                Return True
            End If

            entry = Nothing
            Return False
        End Function

        ''' <summary>按名称获取张量；名称不存在时抛出异常。</summary>
        ''' <param name="name">权重名称</param>
        Public Function GetTensor(name As String) As Tensor
            Return GetEntry(name).Value
        End Function

        ''' <summary>尝试按名称获取张量。</summary>
        ''' <param name="name">权重名称</param>
        ''' <param name="value">输出参数：命中的张量</param>
        Public Function TryGetTensor(name As String, ByRef value As Tensor) As Boolean
            Dim entry As PtTensorEntry

            If TryGetEntry(name, entry) Then
                value = entry.Value
                Return True
            Else
                value = Nothing
                Return False
            End If
        End Function

        ''' <summary>加入一项权重；同名项会被覆盖。</summary>
        ''' <param name="name">权重名称</param>
        ''' <param name="value">张量</param>
        ''' <param name="elementType">落盘元素类型</param>
        ''' <param name="requiresGrad">是否需要梯度</param>
        Public Sub Add(name As String, value As Tensor, elementType As PtElementType,
                       Optional requiresGrad As Boolean = False)
            If String.IsNullOrEmpty(name) Then
                Throw New ArgumentException("权重名称不能为空", NameOf(name))
            End If

            If value Is Nothing Then
                Throw New ArgumentNullException(NameOf(value), $"权重 '{name}' 的张量不能为空")
            End If

            Dim entry As New PtTensorEntry With {
                .Name = name,
                .Value = value,
                .ElementType = elementType,
                .RequiresGrad = requiresGrad
            }

            Dim position As Integer

            If _index.TryGetValue(name, position) Then
                _entries(position) = entry
            Else
                _index(name) = _entries.Count
                _entries.Add(entry)
            End If
        End Sub

        ''' <summary>移除一项权重。</summary>
        ''' <param name="name">权重名称</param>
        Public Function Remove(name As String) As Boolean
            If Not ContainsKey(name) Then
                Return False
            End If

            Call _entries.RemoveAll(Function(e) String.Equals(e.Name, name, StringComparison.Ordinal))
            Call _index.Clear()

            For i As Integer = 0 To _entries.Count - 1
                _index(_entries(i).Name) = i
            Next

            Return True
        End Function

        ''' <summary>转换为"名称 → 张量"的普通字典。</summary>
        Public Function AsTensors() As Dictionary(Of String, Tensor)
            Dim result As New Dictionary(Of String, Tensor)(StringComparer.Ordinal)

            For Each entry As PtTensorEntry In _entries
                result(entry.Name) = entry.Value
            Next

            Return result
        End Function

        ''' <summary>
        ''' 由"名称 → 张量"的字典构造权重字典（同一元素类型）。
        ''' </summary>
        ''' <param name="tensors">名称 → 张量</param>
        ''' <param name="elementType">落盘元素类型</param>
        Public Shared Function FromTensors(tensors As IDictionary(Of String, Tensor),
                                           Optional elementType As PtElementType = PtElementType.Float32) As PtStateDict
            If tensors Is Nothing Then
                Throw New ArgumentNullException(NameOf(tensors))
            End If

            Dim result As New PtStateDict()

            For Each name As String In tensors.Keys
                Call result.Add(name, tensors(name), elementType)
            Next

            Return result
        End Function

        ''' <summary>
        ''' 保存为 PyTorch <c>.pt</c> 文件（每项按各自的原始元素类型写出）。
        ''' </summary>
        ''' <param name="path">目标文件路径</param>
        Public Sub Save(path As String)
            Call PtFile.Save(path, Me)
        End Sub

        ''' <summary>
        ''' 生成结构摘要（按元素类型分组计数，并列出张量总数与数值总数）。
        ''' </summary>
        Public Function Summary() As String
            Dim sb As New StringBuilder()
            Dim groups As New Dictionary(Of PtElementType, Integer)()
            Dim total As Long = 0

            For Each entry As PtTensorEntry In _entries
                Dim count As Integer

                If Not groups.TryGetValue(entry.ElementType, count) Then
                    count = 0
                End If

                groups(entry.ElementType) = count + 1

                If entry.Value IsNot Nothing Then
                    total += entry.Value.Length
                End If
            Next

            Call sb.AppendLine($"{_entries.Count} tensors, {total} values")

            For Each type As PtElementType In groups.Keys.OrderBy(Function(t) CInt(t))
                Call sb.AppendLine($"  {PtDataType.DTypeName(type)}: {groups(type)} tensors")
            Next

            Return sb.ToString()
        End Function

        Public Overrides Function ToString() As String
            Return $"PtStateDict[{_entries.Count}]"
        End Function

    End Class

End Namespace
