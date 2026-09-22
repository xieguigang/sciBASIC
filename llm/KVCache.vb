#Region "Microsoft.VisualBasic::cbb97aa8764e5dbbe5db9d2e4ca9e181, llm\KVCache.vb"

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

    '   Total Lines: 194
    '    Code Lines: 98 (50.52%)
    ' Comment Lines: 62 (31.96%)
    '    - Xml Docs: 66.13%
    ' 
    '   Blank Lines: 34 (17.53%)
    '     File Size: 8.12 KB


    ' Class KVCache
    ' 
    '     Properties: Capacity, CapacityBytes, HeadDim, HeadStride, Keys
    '                 KeysRaw, Length, NkvHeads, PeakLength, UsedBytes
    '                 Values, ValuesRaw
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    '     Function: PrefixView, ToString
    ' 
    '     Sub: Append, Clear, Reset
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' ---------------------------------------------------------------------------
' KVCache —— 自回归解码的 K/V 缓存
'
' 朴素实现的代价：生成第 t 个 token 时，要把全部 t 个位置的 K、V 重新算一遍，
' 累计复杂度 O(t²)。而"已生成 token 的 K、V 在后续步骤中不再改变"这一观察，
' 让这部分计算可以被完整复用 —— 每步只需计算新 token 的 Q、K、V 并追加到缓存，
' 单步开销降为 O(t)（注意力段），代价是缓存占用随序列长度线性增长。
'
' 内存布局的选择（很重要）：
'
'     位置优先  [maxSeq, nKvHeads, headDim]
'
' 之所以按"位置"而不是按"头"优先，是因为增量解码永远只访问前缀 [0, Length)：
' 位置优先时这个前缀在内存里是连续的一段，可以直接用零拷贝视图
' <c>Tensor.Wrap(data, Length, nKvHeads, headDim)</c> 暴露出去；
' 若按"头"优先，同一位置的数据被 head 维隔开，前缀就不再连续，每次都要重排。
'
' 追加（Append）也因同样的原因退化成一次整块 Array.Copy。
' ---------------------------------------------------------------------------

Imports Microsoft.VisualBasic.MachineLearning.TensorFlow

''' <summary>
''' 单层的 K/V 缓存：预分配 <c>[maxSeq, nKvHeads, headDim]</c>，按位置顺序追加。
''' </summary>
''' <remarks>
''' 本类只负责"存"，不负责"算"；注意力层直接读写 <see cref="KeysRaw"/> /
''' <see cref="ValuesRaw"/>，从而避免在每步解码中产生临时张量。
''' </remarks>
Public Class KVCache

    Private ReadOnly _keys As Double()
    Private ReadOnly _values As Double()
    Private _length As Integer
    Private _peakLength As Integer

    ''' <summary>缓存容量（可容纳的最大位置数）。</summary>
    Public ReadOnly Property Capacity As Integer

    ''' <summary>K/V 头的个数。小于注意力头数时即为 GQA / MQA。</summary>
    Public ReadOnly Property NkvHeads As Integer

    ''' <summary>单个头的维度。</summary>
    Public ReadOnly Property HeadDim As Integer

    ''' <summary>当前已缓存的位置数，同时也是下一个待写入 token 的绝对位置。</summary>
    Public ReadOnly Property Length As Integer
        Get
            Return _length
        End Get
    End Property

    ''' <summary>历史最高水位（用于观察"没有缓存时"的等价开销与缓存占用峰值）。</summary>
    Public ReadOnly Property PeakLength As Integer
        Get
            Return _peakLength
        End Get
    End Property

    ''' <summary>单个位置的 K/V 元素总数（= <c>nKvHeads * headDim</c>）。</summary>
    Public ReadOnly Property HeadStride As Integer
        Get
            Return NkvHeads * HeadDim
        End Get
    End Property

    ''' <summary>当前缓存占用的字节数（K 与 V 合计）。</summary>
    Public ReadOnly Property UsedBytes As Long
        Get
            Return _length * HeadStride * 2L * 8L
        End Get
    End Property

    ''' <summary>按容量预分配的字节数。</summary>
    Public ReadOnly Property CapacityBytes As Long
        Get
            Return Capacity * HeadStride * 2L * 8L
        End Get
    End Property

    ''' <summary>K 的底层缓冲区（布局 <c>[maxSeq, nKvHeads, headDim]</c>）。</summary>
    Public ReadOnly Property KeysRaw As Double()
        Get
            Return _keys
        End Get
    End Property

    ''' <summary>V 的底层缓冲区（布局与 <see cref="KeysRaw"/> 相同）。</summary>
    Public ReadOnly Property ValuesRaw As Double()
        Get
            Return _values
        End Get
    End Property

    ''' <summary>
    ''' 已缓存 K 的零拷贝只读视图，形状 <c>[Length, nKvHeads, headDim]</c>。
    ''' </summary>
    ''' <remarks>
    ''' 因为缓冲区按位置优先且只能顺序追加，前 <c>Length</c> 个位置恰好占据
    ''' 底层数组的前 <c>Length * HeadStride</c> 个元素，所以可以安全地零拷贝包装。
    ''' 长度会被向上取整到元素个数（例如 <c>Length=0</c> 时退化为长度 1 的空视图）。
    ''' </remarks>
    Public ReadOnly Property Keys As Tensor
        Get
            Return PrefixView(_keys, _length)
        End Get
    End Property

    ''' <summary>已缓存 V 的零拷贝只读视图，形状同 <see cref="Keys"/>。</summary>
    Public ReadOnly Property Values As Tensor
        Get
            Return PrefixView(_values, _length)
        End Get
    End Property

    Private Function PrefixView(buffer As Double(), length As Integer) As Tensor
        If length <= 0 Then
            ' 空缓存：Tensor.Wrap 要求数据长度与形状乘积严格相等，
            ' 长度为 0 时无法表达，这里退化为一个真正的空张量而不是零拷贝视图。
            Return New Tensor(0, NkvHeads, HeadDim)
        End If

        Return Tensor.Wrap(buffer, length, NkvHeads, HeadDim)
    End Function

    ''' <param name="maxSeq">可容纳的最大位置数</param>
    ''' <param name="nKvHeads">K/V 头个数（等于注意力头数时为标准 MHA，小于则为 GQA / MQA）</param>
    ''' <param name="headDim">单头维度</param>
    Public Sub New(maxSeq As Integer, nKvHeads As Integer, headDim As Integer)
        If maxSeq <= 0 Then Throw New ArgumentException($"KVCache 容量必须为正数，实际 {maxSeq}")
        If nKvHeads <= 0 Then Throw New ArgumentException($"KVCache 的 nKvHeads 必须为正数，实际 {nKvHeads}")
        If headDim <= 0 Then Throw New ArgumentException($"KVCache 的 headDim 必须为正数，实际 {headDim}")

        Me.Capacity = maxSeq
        Me.NkvHeads = nKvHeads
        Me.HeadDim = headDim

        Dim total = maxSeq * nKvHeads * headDim

        _keys = New Double(total - 1) {}
        _values = New Double(total - 1) {}
    End Sub

    ''' <summary>
    ''' 追加 <paramref name="count"/> 个位置的 K/V。
    ''' </summary>
    ''' <param name="keys">K 的数据源，来源布局 <c>[*, nKvHeads, headDim]</c></param>
    ''' <param name="keysOffset">K 数据源中起始元素的偏移</param>
    ''' <param name="values">V 的数据源，布局与 <paramref name="keys"/> 相同</param>
    ''' <param name="valuesOffset">V 数据源中起始元素的偏移</param>
    ''' <param name="count">追加的位置数</param>
    Public Sub Append(keys As Double(), keysOffset As Integer,
                      values As Double(), valuesOffset As Integer,
                      count As Integer)

        If count <= 0 Then Return

        If _length + count > Capacity Then
            Throw New InvalidOperationException(
                $"KV Cache 已满：容量 {Capacity}，已用 {_length}，本次追加 {count}")
        End If

        Dim bytes = count * HeadStride
        Dim dstOffset = _length * HeadStride

        Call Array.Copy(keys, keysOffset, _keys, dstOffset, bytes)
        Call Array.Copy(values, valuesOffset, _values, dstOffset, bytes)

        _length += count

        If _length > _peakLength Then _peakLength = _length
    End Sub

    ''' <summary>把缓存长度归零（复用同一块缓冲区开始新一轮会话）。</summary>
    Public Sub Reset()
        _length = 0
    End Sub

    ''' <summary>释放底层缓冲区引用（用于让 GC 尽早回收大数组）。</summary>
    Public Sub Clear()
        _length = 0
        Array.Clear(_keys, 0, _keys.Length)
        Array.Clear(_values, 0, _values.Length)
    End Sub

    ''' <summary>Returns a short summary of the cache occupancy and size.</summary>
    ''' <returns>A text that reports the used positions, the head layout and the memory footprint.</returns>
    Public Overrides Function ToString() As String
        Return $"[KV {_length}/{Capacity} pos, {NkvHeads} kv-heads x {HeadDim}] {UsedBytes / 1024.0 / 1024.0:F2} MB"
    End Function

End Class



