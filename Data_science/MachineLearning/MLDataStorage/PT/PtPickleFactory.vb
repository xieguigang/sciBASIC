' /********************************************************************************/
'  Microsoft.VisualBasic.MachineLearning.DataStorage
'  Data_science\MachineLearning\MLDataStorage\PT\PtPickleFactory.vb
' /********************************************************************************/

Imports System.IO
Imports System.Linq
Imports Microsoft.VisualBasic.MachineLearning.TensorFlow
Imports PyPickle = Microsoft.VisualBasic.Data.IO.Pickle

Namespace PT

    ''' <summary>
    ''' <see cref="MinimalPickleUnpickler"/> 的 PyTorch 扩展工厂：把 <c>data.pkl</c> 中的
    ''' <c>torch._utils._rebuild_tensor_v2(...)</c> 调用还原为 <see cref="Tensor"/>。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' PyTorch 的归档把张量写成"先 pickle 出存储引用（持久化 ID），再由
    ''' <c>_rebuild_tensor_v2(storage, storage_offset, size, stride, requires_grad, backward_hooks)</c>
    ''' 重新组装"的形式。持久化 ID 在 pickle 协议中由 <c>BINPERSID</c> 承载，反序列化器只是把它原样
    ''' 压回栈上，因此这个元组会作为第一个参数进入本工厂。
    ''' </para>
    ''' <para>
    ''' 持久化 ID 元组形如 <c>("storage", torch.float32, "0", "cpu", 4000)</c>。其中的 dtype 元素
    ''' 在 pickle 流里是 <c>GLOBAL</c> 记号，反序列化器内部用 <c>Friend</c> 类型
    ''' <c>Pickle.PythonGlobalRef</c> 承载，本工程看不到该类型，因此这里统一用文本
    ''' （<c>"torch.float32"</c> / <c>"torch.FloatStorage"</c>）识别 dtype。
    ''' </para>
    ''' <para>
    ''' 存储块按 <see cref="PtStorage.Key"/> 缓存：同一个存储可能被多个张量引用（视图 / 权重绑定），
    ''' 缓存保证只解码一次。
    ''' </para>
    ''' </remarks>
    Public Class PtPickleFactory
        Implements PyPickle.IPickleObjectFactory

        Private ReadOnly _readRecord As Func(Of String, Byte())
        Private ReadOnly _storages As New Dictionary(Of String, PtStorage)(StringComparer.Ordinal)
        Private ReadOnly _metadata As New Dictionary(Of Tensor, PtTensorEntry)(New TensorReferenceComparer())
        Private _tensorCount As Integer

        ''' <summary>创建工厂。</summary>
        ''' <param name="readRecord">
        ''' 存储键 → 原始字节的委托；由归档容器提供（读取 zip 条目 <c>data/&lt;key&gt;</c>）。
        ''' </param>
        Public Sub New(readRecord As Func(Of String, Byte()))
            If readRecord Is Nothing Then
                Throw New ArgumentNullException(NameOf(readRecord))
            End If

            _readRecord = readRecord
        End Sub

        ''' <summary>已经解析出的存储块（键 → 存储）。</summary>
        Public ReadOnly Property Storages As IReadOnlyDictionary(Of String, PtStorage)
            Get
                Return _storages
            End Get
        End Property

        ''' <summary>已经重建的张量个数。</summary>
        Public ReadOnly Property TensorCount As Integer
            Get
                Return _tensorCount
            End Get
        End Property

        ''' <summary>
        ''' 查询某个张量在归档中记录的元素类型与 <c>requires_grad</c> 标记。
        ''' </summary>
        ''' <param name="tensor">由本工厂重建的张量</param>
        ''' <param name="elementType">输出参数：元素类型</param>
        ''' <param name="requiresGrad">输出参数：是否需要梯度</param>
        ''' <returns>是否为本工厂重建的张量</returns>
        Public Function TryGetMetadata(tensor As Tensor, ByRef elementType As PtElementType,
                                       ByRef requiresGrad As Boolean) As Boolean
            Dim entry As PtTensorEntry

            If tensor IsNot Nothing AndAlso _metadata.TryGetValue(tensor, entry) Then
                elementType = entry.ElementType
                requiresGrad = entry.RequiresGrad
                Return True
            Else
                elementType = PtElementType.Float32
                requiresGrad = False
                Return False
            End If
        End Function

        ''' <summary>记录（或更新）张量的元信息。</summary>
        Private Sub SetMetadata(tensor As Tensor, elementType As PtElementType, requiresGrad As Boolean)
            _metadata(tensor) = New PtTensorEntry With {
                .Value = tensor,
                .ElementType = elementType,
                .RequiresGrad = requiresGrad
            }
        End Sub

        ''' <summary>
        ''' 识别 PyTorch 的全局符号并构造对应对象。
        ''' </summary>
        ''' <param name="moduleName">模块名</param>
        ''' <param name="className">类 / 函数名</param>
        ''' <param name="args">构造参数</param>
        ''' <returns>张量或形状元组；非 PyTorch 符号返回 <c>Nothing</c></returns>
        Public Function Construct(moduleName As String, className As String, args As Object()) As Object Implements PyPickle.IPickleObjectFactory.Construct
            If String.IsNullOrEmpty(className) Then
                Return Nothing
            End If

            Dim fullName As String = moduleName & "." & className

            Select Case fullName
                Case "torch._utils._rebuild_tensor_v2"
                    Return RebuildTensor(args, requiresGradIndex:=4)

                Case "torch._utils._rebuild_tensor"
                    Return RebuildTensor(args, requiresGradIndex:=-1)

                Case "torch._utils._rebuild_parameter", "torch._utils._rebuild_parameter_with_state"
                    Return RebuildParameter(args)

                Case "torch.Size"
                    ' torch.Size 就是元组，直接透传，后续按元组解析
                    If args IsNot Nothing AndAlso args.Length > 0 Then
                        Return args(0)
                    Else
                        Return New PyPickle.PythonTuple(Array.Empty(Of Object)())
                    End If

                Case Else
                    Return Nothing
            End Select
        End Function

        ''' <summary>
        ''' BUILD 状态回填：PyTorch 的张量不使用 <c>__setstate__</c>，这里始终交回默认实现。
        ''' </summary>
        Public Function ApplyState(ByRef obj As Object, state As Object) As Boolean Implements PyPickle.IPickleObjectFactory.ApplyState
            Return False
        End Function

#Region "张量重建"

        ''' <summary>
        ''' 由 <c>_rebuild_tensor_v2</c> / <c>_rebuild_tensor</c> 的参数重建张量。
        ''' </summary>
        ''' <param name="args">参数数组</param>
        ''' <param name="requiresGradIndex">requires_grad 所在位置；-1 表示该重载没有此参数</param>
        Private Function RebuildTensor(args As Object(), requiresGradIndex As Integer) As Tensor
            If args Is Nothing OrElse args.Length < 4 Then
                Throw New InvalidDataException(
                    $"torch._utils._rebuild_tensor 的参数个数不足：期望至少 4 个，实际 {If(args Is Nothing, 0, args.Length)} 个")
            End If

            Dim storage As PtStorage = ResolveStorage(args(0))
            Dim offset As Long = ToInteger(args(1), "storage_offset")
            Dim shape As Integer() = ToInt32Array(args(2), "size")
            Dim strides As Integer() = ToInt32Array(args(3), "stride")

            Dim requiresGrad As Boolean = False

            If requiresGradIndex >= 0 AndAlso args.Length > requiresGradIndex AndAlso TypeOf args(requiresGradIndex) Is Boolean Then
                requiresGrad = CBool(args(requiresGradIndex))
            End If

            Dim data As Double() = Materialize(storage, offset, shape, strides)
            Dim tensor As New Tensor(data, NormalizeShape(shape)) With {
                .IsVariable = requiresGrad
            }

            _tensorCount += 1

            Call SetMetadata(tensor, storage.ElementType, requiresGrad)

            Return tensor
        End Function

        ''' <summary>
        ''' 包装 <c>torch.nn.Parameter</c>：参数本身就是张量，这里把 <c>requires_grad</c> 传递下去。
        ''' </summary>
        ''' <param name="args">参数数组</param>
        Private Function RebuildParameter(args As Object()) As Tensor
            If args Is Nothing OrElse args.Length = 0 Then
                Throw New InvalidDataException("torch._utils._rebuild_parameter 缺少张量参数")
            End If

            Dim tensor As Tensor = TryCast(args(0), Tensor)

            If tensor Is Nothing Then
                Throw New InvalidDataException(
                    $"torch._utils._rebuild_parameter 的第一个参数不是张量（实际为 {args(0)?.GetType().FullName}）")
            End If

            Dim elementType As PtElementType = PtElementType.Float32
            Dim requiresGrad As Boolean = tensor.IsVariable

            Call TryGetMetadata(tensor, elementType, requiresGrad)

            If args.Length > 1 AndAlso TypeOf args(1) Is Boolean Then
                requiresGrad = CBool(args(1))
            End If

            tensor.IsVariable = requiresGrad

            Call SetMetadata(tensor, elementType, requiresGrad)

            Return tensor
        End Function

        ''' <summary>
        ''' 解析持久化 ID 元组并取回对应的存储块。
        ''' </summary>
        ''' <param name="persistentId">持久化 ID（<see cref="PythonTuple"/> 或 <c>Object()</c>）</param>
        Private Function ResolveStorage(persistentId As Object) As PtStorage
            Dim items As Object() = ToItems(persistentId)

            If items Is Nothing OrElse items.Length < 5 Then
                Throw New InvalidDataException(
                    $"无法解析 PyTorch 存储持久化 ID：期望 ('storage', dtype, key, location, numel) 五项，实际为 {Describe(persistentId)}")
            End If

            Dim kind As String = ToToken(items(0))

            If Not String.Equals(kind, "storage", StringComparison.Ordinal) Then
                Throw New InvalidDataException($"不支持的 PyTorch 持久化 ID 类型：'{kind}'（仅支持 'storage'）")
            End If

            Dim typeToken As String = ToToken(items(1))
            Dim key As String = ToToken(items(2))
            Dim location As String = ToToken(items(3))
            Dim declaredCount As Long = ToInteger(items(4), "numel")
            Dim elementType As PtElementType = PtDataType.Parse(typeToken)

            Dim storage As PtStorage

            If _storages.TryGetValue(key, storage) Then
                ' 同一块存储被多个张量引用时直接复用（视图 / 权重绑定）
                Return storage
            End If

            Dim bytes As Byte() = _readRecord(key)

            If bytes Is Nothing Then
                Throw New InvalidDataException($"归档中缺少存储条目 'data/{key}'（{PtDataType.DTypeName(elementType)}）")
            End If

            storage = PtStorage.FromBytes(bytes, key, elementType, declaredCount, location)
            _storages(key) = storage

            Return storage
        End Function

        ''' <summary>
        ''' 按偏移 / 形状 / 步长把存储块物化为行主序的双精度数组。
        ''' </summary>
        ''' <param name="storage">存储块</param>
        ''' <param name="offset">起始元素偏移</param>
        ''' <param name="shape">形状</param>
        ''' <param name="strides">步长（元素单位）</param>
        Private Function Materialize(storage As PtStorage, offset As Long,
                                     shape As Integer(), strides As Integer()) As Double()
            Dim values As Double() = storage.ToArray()

            ' 标量：PyTorch 用空形状表示 0 阶张量，框架的张量至少要有一维
            If shape.Length = 0 Then
                If offset < 0 OrElse offset >= values.Length Then
                    Throw OutOfRange(storage, offset, "标量张量")
                End If

                Return New Double() {values(offset)}
            End If

            If strides.Length <> shape.Length Then
                Throw New InvalidDataException(
                    $"张量形状 [{String.Join(", ", shape)}] 与步长 [{String.Join(", ", strides)}] 的维数不一致" &
                    $"，存储块 '{storage.Key}'")
            End If

            Dim total As Integer = TotalSize(shape, storage)
            Dim result As Double() = New Double(total - 1) {}

            ' 连续张量（步长等于行主序步长）走 Array.Copy 快路径
            If IsContiguous(shape, strides) Then
                If offset < 0 OrElse offset + total > values.Length Then
                    Throw OutOfRange(storage, offset, $"形状 [{String.Join(", ", shape)}] 共 {total} 个元素")
                End If

                Call Array.Copy(values, CInt(offset), result, 0, total)

                Return result
            End If

            ' 非连续视图（转置 / 切片）：按多维索引 × 步长取值
            Dim indices As Integer() = New Integer(shape.Length - 1) {}

            For i As Integer = 0 To total - 1
                Dim position As Long = offset

                For d As Integer = 0 To indices.Length - 1
                    position += CLng(indices(d)) * strides(d)
                Next

                If position < 0 OrElse position >= values.Length Then
                    Throw New InvalidDataException(
                        $"张量索引越过存储块边界：index={position}, count={values.Length}, offset={offset}, " &
                        $"shape=[{String.Join(", ", shape)}], stride=[{String.Join(", ", strides)}], storage='{storage.Key}'")
                End If

                result(i) = values(CInt(position))

                ' 递增多维索引（最后一维变化最快）
                For d As Integer = indices.Length - 1 To 0 Step -1
                    indices(d) += 1

                    If indices(d) < shape(d) Then
                        Exit For
                    End If

                    indices(d) = 0
                Next
            Next

            Return result
        End Function

        Private Function OutOfRange(storage As PtStorage, offset As Long, description As String) As InvalidDataException
            Return New InvalidDataException(
                $"张量的起始偏移越过存储块边界：offset={offset}, count={storage.Count}, " &
                $"storage='{storage.Key}', {description}")
        End Function

        ''' <summary>
        ''' 形状各维乘积，附带容量校验。
        ''' </summary>
        Private Function TotalSize(shape As Integer(), storage As PtStorage) As Integer
            Dim total As Long = 1

            For Each size As Integer In shape
                If size < 0 Then
                    Throw New InvalidDataException(
                        $"张量形状中存在非法维度 {size}（storage='{storage.Key}'）")
                End If

                total *= size

                If total > Integer.MaxValue Then
                    Throw New NotSupportedException(
                        $"张量形状 [{String.Join(", ", shape)}] 共 {total} 个元素，超过单个数组的容量上限 " &
                        $"{Integer.MaxValue}（storage='{storage.Key}'）")
                End If
            Next

            Return CInt(total)
        End Function

        ''' <summary>
        ''' 判断步长是否为行主序连续布局。
        ''' </summary>
        Private Function IsContiguous(shape As Integer(), strides As Integer()) As Boolean
            Dim expected As Integer = 1

            For d As Integer = shape.Length - 1 To 0 Step -1
                If shape(d) > 1 AndAlso strides(d) <> expected Then
                    Return False
                End If

                expected *= shape(d)
            Next

            Return True
        End Function

        ''' <summary>
        ''' 框架的张量要求至少一维：0 阶（标量）形状统一按 <c>{1}</c> 处理。
        ''' </summary>
        Private Shared Function NormalizeShape(shape As Integer()) As Integer()
            If shape Is Nothing OrElse shape.Length = 0 Then
                Return New Integer() {1}
            End If

            Return shape
        End Function

#End Region

#Region "参数解析辅助"

        ''' <summary>把元组形态的参数展开为对象数组。</summary>
        Private Shared Function ToItems(raw As Object) As Object()
            Dim [tuple] As PyPickle.PythonTuple = TryCast(raw, PyPickle.PythonTuple)

            If [tuple] IsNot Nothing Then
                Return [tuple].Items
            End If

            Dim buffer As Object() = TryCast(raw, Object())

            If buffer IsNot Nothing Then
                Return buffer
            End If

            Dim list As IList = TryCast(raw, IList)

            If list IsNot Nothing Then
                Dim result(list.Count - 1) As Object

                Call list.CopyTo(result, 0)

                Return result
            End If

            Return Nothing
        End Function

        ''' <summary>把元组形态的整数参数展开为 <see cref="Integer"/> 数组。</summary>
        ''' <param name="raw">pickle 中的元组（<see cref="PyPickle.PythonTuple"/>、<c>Object()</c> 或 <c>IList</c>）</param>
        ''' <param name="label">参数名（用于错误信息）</param>
        Private Shared Function ToInt32Array(raw As Object, label As String) As Integer()
            Dim items As Object() = ToItems(raw)

            If items Is Nothing Then
                ' 单个整数（0 阶张量的 size 偶尔被写成标量）
                If TypeOf raw Is Long OrElse TypeOf raw Is Integer Then
                    Return New Integer() {CInt(ToInteger(raw, label))}
                End If

                Throw New InvalidDataException($"PyTorch 张量参数 {label} 缺失或不是元组")
            End If

            Dim result As Integer() = New Integer(items.Length - 1) {}

            For i As Integer = 0 To items.Length - 1
                result(i) = CInt(ToInteger(items(i), label))
            Next

            Return result
        End Function

        ''' <summary>把 pickle 标量转换为整数（支持整数、BigInteger 与浮点）。</summary>
        Private Shared Function ToInteger(raw As Object, label As String) As Long
            If raw Is Nothing Then
                Throw New InvalidDataException($"PyTorch 张量参数 {label} 为空")
            End If

            Select Case True
                Case TypeOf raw Is Long
                    Return CLng(raw)
                Case TypeOf raw Is Integer
                    Return CLng(raw)
                Case TypeOf raw Is Short
                    Return CLng(raw)
                Case TypeOf raw Is Byte
                    Return CLng(raw)
                Case TypeOf raw Is Double
                    Return CLng(raw)
                Case TypeOf raw Is Single
                    Return CLng(raw)
                Case TypeOf raw Is System.Numerics.BigInteger
                    Return CLng(DirectCast(raw, System.Numerics.BigInteger))
                Case Else
                    Dim text As String = ToToken(raw)
                    Dim value As Long

                    If Long.TryParse(text, Globalization.NumberStyles.Integer, Globalization.CultureInfo.InvariantCulture, value) Then
                        Return value
                    End If

                    Throw New InvalidDataException($"PyTorch 张量参数 {label} 无法解析为整数：'{text}'")
            End Select
        End Function

        ''' <summary>
        ''' 取对象的名字文本：优先 <see cref="PythonObject.FullName"/>，其余（含内部的全局引用）用
        ''' <see cref="Object.ToString"/>。
        ''' </summary>
        Private Shared Function ToToken(raw As Object) As String
            If raw Is Nothing Then
                Return "None"
            End If

            Dim pyObject As PyPickle.PythonObject = TryCast(raw, PyPickle.PythonObject)

            If pyObject IsNot Nothing Then
                Return pyObject.FullName
            End If

            ' 注意：不能用 CStr(obj)，PythonGlobalRef 之类没有实现 IConvertible 的类型会抛 InvalidCastException
            Return raw.ToString()
        End Function

        ''' <summary>把参数对象渲染成可读文本（仅用于异常信息）。</summary>
        Private Shared Function Describe(raw As Object) As String
            Dim items As Object() = ToItems(raw)

            If items Is Nothing Then
                Return $"'{ToToken(raw)}'"
            End If

            Return "(" & String.Join(", ", items.Select(Function(o) $"'{ToToken(o)}'")) & ")"
        End Function

#End Region

#Region "引用相等比较器"

        ''' <summary>
        ''' 按引用比较张量：元信息表以张量实例为键，若用值语义比较，两个数值相同的不同张量会被误判为同一项。
        ''' </summary>
        Private NotInheritable Class TensorReferenceComparer
            Implements IEqualityComparer(Of Tensor)

            Public Overloads Function Equals(x As Tensor, y As Tensor) As Boolean Implements IEqualityComparer(Of Tensor).Equals
                Return Object.ReferenceEquals(x, y)
            End Function

            Public Overloads Function GetHashCode(obj As Tensor) As Integer Implements IEqualityComparer(Of Tensor).GetHashCode
                Return System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj)
            End Function

        End Class

#End Region

    End Class

End Namespace
