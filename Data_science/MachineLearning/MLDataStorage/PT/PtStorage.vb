' /********************************************************************************/
'  Microsoft.VisualBasic.MachineLearning.DataStorage
'  Data_science\MachineLearning\MLDataStorage\PT\PtStorage.vb
' /********************************************************************************/

Imports System.IO

Namespace PT

    ''' <summary>
    ''' PyTorch 归档中的一块存储（storage）：一段连续的原始字节，按 <see cref="ElementType"/> 解释。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' 存储块是 `.pt` 归档里真正存放数值的地方（zip 条目 <c>data/&lt;key&gt;</c>）。
    ''' pickle 流中的张量只保存"引用 + 偏移 + 形状 + 步长"，多个张量可以共享同一块存储
    ''' （典型例子是转置 / 切片视图，或权重绑定），因此读取端必须按 <see cref="Key"/> 缓存。
    ''' </para>
    ''' <para>
    ''' pickle 中声明的元素数（<see cref="DeclaredCount"/>）与存储块实际字节数理论上应当一致
    ''' （<c>字节数 = numel × itemsize</c>）；个别文件存在字段不一致，此时以**实际字节数**为准
    ''' 重新推导元素数，避免整文件读取失败。
    ''' </para>
    ''' </remarks>
    Public Class PtStorage

        Private _values As Double()

        ''' <summary>存储键：pickle 持久化 ID 中的第 3 个元素，对应 zip 条目 <c>data/&lt;key&gt;</c>。</summary>
        Public Property Key As String

        ''' <summary>元素类型。</summary>
        Public Property ElementType As PtElementType

        ''' <summary>
        ''' pickle 中记录的位置标记（<c>cpu</c> / <c>cuda:0</c> 等）。
        ''' </summary>
        ''' <remarks>
        ''' 仅用于诊断：新式 zip 归档的数值字节总是写在 zip 内，与原始设备无关。
        ''' </remarks>
        Public Property Location As String

        ''' <summary>pickle 中声明的元素个数。</summary>
        Public Property DeclaredCount As Long

        ''' <summary>存储块的原始字节。</summary>
        Public Property RawBytes As Byte()

        ''' <summary>
        ''' 依据实际字节数推导出的元素个数（字节数 ÷ 单个元素字节数）。
        ''' </summary>
        Public ReadOnly Property Count As Integer
            Get
                If RawBytes Is Nothing Then
                    Return 0
                End If

                Return RawBytes.Length \ PtDataType.ElementSize(ElementType)
            End Get
        End Property

        ''' <summary>
        ''' 声明的元素数与实际字节数是否一致。
        ''' </summary>
        Public ReadOnly Property HasCountMismatch As Boolean
            Get
                Return DeclaredCount <> Count
            End Get
        End Property

        ''' <summary>
        ''' 把存储块解码为双精度数组（同一实例只解码一次）。
        ''' </summary>
        Public Function ToArray() As Double()
            If _values Is Nothing Then
                _values = PtDataType.Decode(RawBytes, 0, Count, ElementType)
            End If

            Return _values
        End Function

        ''' <summary>
        ''' 由双精度数组构造存储块。
        ''' </summary>
        ''' <param name="values">数值</param>
        ''' <param name="key">存储键</param>
        ''' <param name="type">元素类型</param>
        Public Shared Function FromArray(values As Double(), key As String, type As PtElementType) As PtStorage
            If values Is Nothing Then
                Throw New ArgumentNullException(NameOf(values))
            End If

            Dim bytes As Byte() = PtDataType.Encode(values, type)

            Return New PtStorage With {
                .Key = key,
                .ElementType = type,
                .Location = "cpu",
                .DeclaredCount = values.Length,
                .RawBytes = bytes
            }
        End Function

        ''' <summary>
        ''' 从原始字节构造存储块。
        ''' </summary>
        ''' <param name="bytes">原始字节</param>
        ''' <param name="key">存储键</param>
        ''' <param name="type">元素类型</param>
        ''' <param name="declaredCount">pickle 中声明的元素数</param>
        ''' <param name="location">位置标记</param>
        Public Shared Function FromBytes(bytes As Byte(), key As String, type As PtElementType,
                                         Optional declaredCount As Long = -1,
                                         Optional location As String = "cpu") As PtStorage
            Dim result As New PtStorage With {
                .Key = key,
                .ElementType = type,
                .Location = location,
                .RawBytes = bytes
            }

            Dim size As Integer = PtDataType.ElementSize(type)

            If bytes.Length Mod size <> 0 Then
                Throw New InvalidDataException(
                    $"存储块 '{key}' 的字节数 {bytes.Length} 不是单个元素字节数 {size}（{PtDataType.DTypeName(type)}）的整数倍")
            End If

            result.DeclaredCount = If(declaredCount >= 0, declaredCount, bytes.Length \ size)

            Return result
        End Function

        Public Overrides Function ToString() As String
            Return $"storage[{Key}] {PtDataType.DTypeName(ElementType)} x {Count} @ {Location}"
        End Function

    End Class

End Namespace
