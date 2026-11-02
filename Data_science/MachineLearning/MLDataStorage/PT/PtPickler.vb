' /********************************************************************************/
'  Microsoft.VisualBasic.MachineLearning.DataStorage
'  Data_science\MachineLearning\MLDataStorage\PT\PtPickler.vb
' /********************************************************************************/

Imports System.Text

Namespace PT

    ''' <summary>
    ''' 生成 PyTorch <c>data.pkl</c> 的最小 pickle 写入器（协议 2）。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' 为什么不用 <c>Pickle.MinimalPicklePickler</c>：PyTorch 的归档必须把张量数值表达为"持久化 ID"
    ''' （<c>BINPERSID</c>），而通用写入器只支持常规 .NET 类型，无法表达"随机访问的外部存储块"。
    ''' 因此这里按 PyTorch 的语义直接产出有限的一组操作码：
    ''' <c>PROTO / GLOBAL / BINUNICODE / BININT* / MARK / TUPLE1-3 / TUPLE / REDUCE / BINPERSID / SETITEMS / STOP</c>。
    ''' </para>
    ''' <para>
    ''' 输出的对象图与 <c>torch.save(state_dict)</c> 一致：
    ''' <code>
    ''' collections.OrderedDict(
    '''     "name" -> torch._utils._rebuild_tensor_v2(
    '''         ("storage", torch.float32, "0", "cpu", numel),
    '''         storage_offset, size, stride, requires_grad, collections.OrderedDict()
    '''     )
    ''' )
    ''' </code>
    ''' 其中存储引用由 <c>BINPERSID</c> 承载，PyTorch 读取时通过 <c>persistent_load</c> 从归档里取回
    ''' 对应的 <c>data/&lt;key&gt;</c> 记录。
    ''' </para>
    ''' <para>
    ''' 为减少体积与复杂度，这里**不写 memo**（pickle 允许），因此文件略大于 <c>torch.save</c> 的产物，
    ''' 但语义完全等价。
    ''' </para>
    ''' </remarks>
    Friend Class PtPickler

        ' ===== pickle 协议 2 操作码 =====
        Private Const PROTO As Byte = &H80
        Private Const [STOP] As Byte = &H2E
        Private Const MARK As Byte = &H28
        Private Const [GLOBAL] As Byte = &H63
        Private Const REDUCE As Byte = &H52
        Private Const BINPERSID As Byte = &H51
        Private Const EMPTY_TUPLE As Byte = &H29
        Private Const TUPLE As Byte = &H74
        Private Const TUPLE1 As Byte = &H85
        Private Const TUPLE2 As Byte = &H86
        Private Const TUPLE3 As Byte = &H87
        Private Const NEWTRUE As Byte = &H88
        Private Const NEWFALSE As Byte = &H89
        Private Const BININT As Byte = &H4A
        Private Const BININT1 As Byte = &H4B
        Private Const BININT2 As Byte = &H4D
        Private Const BINUNICODE As Byte = &H58
        Private Const SETITEMS As Byte = &H75

        Private ReadOnly _output As New List(Of Byte)(1024)

        ''' <summary>
        ''' 是否使用旧式 storage 类名（<c>torch.FloatStorage</c>）代替新式 dtype 记号（<c>torch.float32</c>）。
        ''' </summary>
        ''' <remarks>
        ''' 新式记号需要 PyTorch 1.6 及以上；旧式类名能被 1.x 全系列与 2.x 识别，但在 2.x 上会触发
        ''' <c>TypedStorage</c> 的弃用警告。默认写新式记号。
        ''' </remarks>
        Public Property LegacyStorageNames As Boolean = False

        ''' <summary>
        ''' 把权重字典序列化为 <c>data.pkl</c> 的字节流。
        ''' </summary>
        ''' <param name="state">权重字典</param>
        ''' <param name="storageKeys">每项对应的存储键（与 <see cref="PtStateDict.Entries"/> 顺序一致）</param>
        Public Shared Function Pickle(state As PtStateDict, storageKeys As String(),
                                       Optional legacyStorageNames As Boolean = False) As Byte()
            If state Is Nothing Then
                Throw New ArgumentNullException(NameOf(state))
            End If

            If storageKeys Is Nothing OrElse storageKeys.Length <> state.Count Then
                Throw New ArgumentException("存储键数量必须与权重项数量一致", NameOf(storageKeys))
            End If

            Dim pickler As New PtPickler With {
                .LegacyStorageNames = legacyStorageNames
            }

            Return pickler.Serialize(state, storageKeys)
        End Function

        Private Function Serialize(state As PtStateDict, storageKeys As String()) As Byte()
            Call _output.Clear()

            ' 协议头
            Call _output.Add(PROTO)
            Call _output.Add(2)

            ' collections.OrderedDict() —— 与 torch.save(state_dict) 的根对象一致
            Call WriteGlobal("collections", "OrderedDict")
            Call _output.Add(EMPTY_TUPLE)
            Call _output.Add(REDUCE)

            Call _output.Add(MARK)

            For i As Integer = 0 To state.Count - 1
                Dim entry As PtTensorEntry = state.Entries(i)

                Call WriteUnicode(entry.Name)
                Call WriteTensor(entry, storageKeys(i))
            Next

            Call _output.Add(SETITEMS)
            Call _output.Add([STOP])

            Return _output.ToArray()
        End Function

        ''' <summary>
        ''' 写出单个张量：<c>torch._utils._rebuild_tensor_v2(pid, offset, size, stride, requires_grad, hooks)</c>。
        ''' </summary>
        Private Sub WriteTensor(entry As PtTensorEntry, storageKey As String)
            Dim shape As Integer() = entry.Shape
            Dim values As Double() = entry.Value.Data

            Call WriteGlobal("torch._utils", "_rebuild_tensor_v2")

            Call _output.Add(MARK)

            ' ---- 第 1 个参数：存储持久化 ID ----
            Call _output.Add(MARK)
            Call WriteUnicode("storage")

            If LegacyStorageNames Then
                Call WriteTypeGlobal(PtDataType.StorageName(entry.ElementType))
            Else
                Call WriteTypeGlobal(PtDataType.DTypeName(entry.ElementType))
            End If

            Call WriteUnicode(storageKey)
            Call WriteUnicode("cpu")
            Call WriteInteger(values.Length)
            Call _output.Add(TUPLE)

            Call _output.Add(BINPERSID)

            ' ---- storage_offset ----
            Call WriteInteger(0)

            ' ---- size ----
            Call WriteIntegerTuple(shape)

            ' ---- stride（行主序连续布局）----
            Call WriteIntegerTuple(ContiguousStride(shape))

            ' ---- requires_grad ----
            Call _output.Add(If(entry.RequiresGrad, NEWTRUE, NEWFALSE))

            ' ---- backward_hooks ----
            Call WriteGlobal("collections", "OrderedDict")
            Call _output.Add(EMPTY_TUPLE)
            Call _output.Add(REDUCE)

            ' 关闭参数元组并调用构造函数
            Call _output.Add(TUPLE)
            Call _output.Add(REDUCE)
        End Sub

        ''' <summary>
        ''' 计算行主序连续布局的步长。
        ''' </summary>
        ''' <param name="shape">形状</param>
        Friend Shared Function ContiguousStride(shape As Integer()) As Integer()
            Dim strides As Integer() = New Integer(shape.Length - 1) {}
            Dim distance As Integer = 1

            For d As Integer = shape.Length - 1 To 0 Step -1
                strides(d) = distance
                distance *= shape(d)
            Next

            Return strides
        End Function

        ''' <summary>
        ''' 写出 <c>torch.&lt;type&gt;</c> 形式的全局引用（模块固定为 <c>torch</c>）。
        ''' </summary>
        Private Sub WriteTypeGlobal(token As String)
            Dim dot As Integer = token.LastIndexOf("."c)

            If dot > 0 Then
                Call WriteGlobal(token.Substring(0, dot), token.Substring(dot + 1))
            Else
                Call WriteGlobal("torch", token)
            End If
        End Sub

        ''' <summary>写出 GLOBAL 操作码：<c>c&lt;module&gt;\n&lt;name&gt;\n</c>。</summary>
        Private Sub WriteGlobal(moduleName As String, name As String)
            Call _output.Add([GLOBAL])
            Call _output.AddRange(Encoding.ASCII.GetBytes(moduleName & vbLf))
            Call _output.AddRange(Encoding.ASCII.GetBytes(name & vbLf))
        End Sub

        ''' <summary>写出 BINUNICODE 操作码：<c>X&lt;len:int32le&gt;&lt;utf8&gt;</c>。</summary>
        Private Sub WriteUnicode(value As String)
            Dim bytes As Byte() = Encoding.UTF8.GetBytes(If(value, ""))

            Call _output.Add(BINUNICODE)
            Call _output.AddRange(BitConverter.GetBytes(bytes.Length))
            Call _output.AddRange(bytes)
        End Sub

        ''' <summary>写出整数（自动选择 BININT1 / BININT2 / BININT）。</summary>
        Private Sub WriteInteger(value As Long)
            If value >= 0 AndAlso value <= &HFF Then
                Call _output.Add(BININT1)
                Call _output.Add(CByte(value))
            ElseIf value >= 0 AndAlso value <= &HFFFF Then
                Call _output.Add(BININT2)
                Call _output.AddRange(BitConverter.GetBytes(CUShort(value)))
            Else
                Call _output.Add(BININT)
                Call _output.AddRange(BitConverter.GetBytes(CInt(value)))
            End If
        End Sub

        ''' <summary>写出整数元组（按元素个数选择 TUPLE1/2/3 或 MARK+TUPLE）。</summary>
        Private Sub WriteIntegerTuple(values As Integer())
            Select Case values.Length
                Case 0
                    Call _output.Add(EMPTY_TUPLE)
                Case 1
                    Call WriteInteger(values(0))
                    Call _output.Add(TUPLE1)
                Case 2
                    Call WriteInteger(values(0))
                    Call WriteInteger(values(1))
                    Call _output.Add(TUPLE2)
                Case 3
                    Call WriteInteger(values(0))
                    Call WriteInteger(values(1))
                    Call WriteInteger(values(2))
                    Call _output.Add(TUPLE3)
                Case Else
                    Call _output.Add(MARK)

                    For Each value As Integer In values
                        Call WriteInteger(value)
                    Next

                    Call _output.Add(TUPLE)
            End Select
        End Sub

    End Class

End Namespace
