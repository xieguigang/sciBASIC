#Region "Microsoft.VisualBasic::e981bf4300a1b85633bb9012b35b827b, Data_science\MachineLearning\DeepLearning\CNN\data\DataBlock.vb"

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

    '   Total Lines: 558
    '    Code Lines: 254 (45.52%)
    ' Comment Lines: 231 (41.40%)
    '    - Xml Docs: 83.98%
    ' 
    '   Blank Lines: 73 (13.08%)
    '     File Size: 24.51 KB


    '     Class DataBlock
    ' 
    '         Properties: Depth, Grad, Grad2D, Grad4D, Gradients
    '                     SX, SY, TensorShape4D, trace, Value
    '                     Value2D, Value4D, Weights
    ' 
    '         Constructor: (+4 Overloads) Sub New
    ' 
    '         Function: clearGradient, clone, cloneAndZero, (+2 Overloads) getGradient, (+2 Overloads) getWeight
    '                   ToString
    ' 
    '         Sub: addFrom, addFromScaled, (+2 Overloads) addGradient, (+3 Overloads) addImageData, addWeight
    '              MarkGradientModified, MarkHostModified, MarkValueModified, mulGradient, (+3 Overloads) setGradient
    '              SetGradients, SetValues, (+2 Overloads) setWeight, subGradient
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Runtime.Serialization
Imports Microsoft.VisualBasic.Language.Java
Imports Microsoft.VisualBasic.Math
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports std = System.Math
Imports Tensor = Microsoft.VisualBasic.MachineLearning.TensorFlow.Tensor

Namespace CNN.data

    ''' <summary>
    ''' Holding all the data handled by the network. So a layer will receive
    ''' this class and return a similar block as a output that will be used
    ''' by the next layer in the chain.
    ''' 
    ''' @author Daniel Persson (mailto.woden@gmail.com)
    ''' </summary>
    Public Class DataBlock

        ''' <summary>Gets the width of the block.</summary>
        Public ReadOnly Property SX As Integer
        ''' <summary>Gets the height of the block.</summary>
        Public ReadOnly Property SY As Integer
        ''' <summary>Gets the number of channels (depth) of the block.</summary>
        Public ReadOnly Property Depth As Integer

        ''' <summary>Optional diagnostic label that identifies where this block came from.</summary>
        Public Property trace As String

        ''' <summary>
        ''' the multiple class classify probability weight
        ''' (or the prediction result) 
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>get <see cref="w"/></remarks>
        Public ReadOnly Property Weights As Double()
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return w
            End Get
        End Property

        ''' <summary>Gets the gradient vector that corresponds one to one with <see cref="Weights"/>.</summary>
        ''' <remarks>Returns the backing field <see cref="dw"/>.</remarks>
        Public ReadOnly Property Gradients As Double()
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return dw
            End Get
        End Property

        ''' <summary>
        ''' backend of <see cref="Weights"/>
        ''' </summary>
        Friend w As Double()
        ''' <summary>
        ''' backend of <see cref="Gradients"/>
        ''' </summary>
        Friend dw As Double()

        ' ------------------------------------------------------------------
        ' 张量视图
        '
        ' 本类对外仍然是"扁平 Double() + (SX,SY,Depth) 索引"的老接口（24 个引用文件
        ' 的调用点因此完全不需要改动），但真正参与计算的数据载体是 TensorFlow 的
        ' Tensor：下面两个属性把 w / dw 零拷贝包装成张量视图，各层的计算实现即可
        ' 直接调用 Tensor / Math / nn 的算子，从而由 Tensor.computeKernel 统一派发到
        ' CPU(SIMD) 或 CUDA 后端。
        '
        ' 采用"按需包装 + 引用比对"而不是把 Tensor 作为持久字段，有两个好处：
        '   1) 任何对 w / dw 的整数组替换（例如 mulGradient、序列化后的字段回填）
        '      都会被自动察觉并重新包装，不存在"别名失效导致写入丢失"的隐患；
        '   2) 反射式序列化仍然只看到 w / dw 两个 Double() 字段，
        '      磁盘格式与旧模型完全兼容。
        ' ------------------------------------------------------------------

        <IgnoreDataMember> Private _value As Tensor
        <IgnoreDataMember> Private _grad As Tensor
        <IgnoreDataMember> Private _value4D As Tensor
        <IgnoreDataMember> Private _grad4D As Tensor
        <IgnoreDataMember> Private _value2D As Tensor
        <IgnoreDataMember> Private _grad2D As Tensor

        ''' <summary>
        ''' 值数据的张量视图（零拷贝，与 <see cref="Weights"/> 共享同一份底层存储）。
        ''' 形状为 (SY, SX, Depth)，正好对应索引公式 (SX*y + x)*Depth + depth。
        ''' </summary>
        Friend ReadOnly Property Value As Tensor
            Get
                If w Is Nothing Then Return Nothing

                If _value Is Nothing OrElse Not Object.ReferenceEquals(_value.Data, w) Then
                    _value = Tensor.Wrap(w, _SY, _SX, _Depth)
                End If

                Return _value
            End Get
        End Property

        ''' <summary>
        ''' 梯度数据的张量视图（零拷贝，与 <see cref="Gradients"/> 共享同一份底层存储）。
        ''' </summary>
        Friend ReadOnly Property Grad As Tensor
            Get
                If dw Is Nothing Then Return Nothing

                If _grad Is Nothing OrElse Not Object.ReferenceEquals(_grad.Data, dw) Then
                    _grad = Tensor.Wrap(dw, _SY, _SX, _Depth)
                End If

                Return _grad
            End Get
        End Property

        ''' <summary>把本块当作单样本图像时的四维形状 (N=1, H=SY, W=SX, C=Depth)</summary>
        Friend ReadOnly Property TensorShape4D As Integer()
            Get
                Return New Integer() {1, _SY, _SX, _Depth}
            End Get
        End Property

        ' ------------------------------------------------------------------
        ' 缓存张量视图
        '
        ' 各层需要的张量形状不止一种：卷积/池化要 (1,H,W,C) 的四维视图、全连接要 (n,1) 的二维视图、
        ' 激活函数要三维视图。这些视图必须**由本块缓存并持有**，不能在各层里用 Tensor.Wrap(...) 现场构造，
        ' 原因见下方 MarkValueModified 的说明：显存缓存的键是"(主机数组, 张量版本号)"，
        ' 而现场构造的新包装对象版本号是独立起算的，无法从本块继承，因此一旦主机数组被就地改写，
        ' 那些现场构造的视图就会命中旧的显存副本 —— 这是一个静默的数值错误。
        ' ------------------------------------------------------------------

        ''' <summary>值数据的四维视图 (1, SY, SX, Depth)：供卷积/池化算子使用</summary>
        Friend ReadOnly Property Value4D As Tensor
            Get
                If w Is Nothing Then Return Nothing

                If _value4D Is Nothing OrElse Not Object.ReferenceEquals(_value4D.Data, w) Then
                    _value4D = Tensor.Wrap(w, 1, _SY, _SX, _Depth)
                End If

                Return _value4D
            End Get
        End Property

        ''' <summary>梯度数据的四维视图 (1, SY, SX, Depth)：作为卷积/池化的 gradOutput</summary>
        Friend ReadOnly Property Grad4D As Tensor
            Get
                If dw Is Nothing Then Return Nothing

                If _grad4D Is Nothing OrElse Not Object.ReferenceEquals(_grad4D.Data, dw) Then
                    _grad4D = Tensor.Wrap(dw, 1, _SY, _SX, _Depth)
                End If

                Return _grad4D
            End Get
        End Property

        ''' <summary>值数据的二维列向量视图 (n, 1)：供矩阵乘使用</summary>
        Friend ReadOnly Property Value2D As Tensor
            Get
                If w Is Nothing Then Return Nothing

                If _value2D Is Nothing OrElse Not Object.ReferenceEquals(_value2D.Data, w) Then
                    _value2D = Tensor.Wrap(w, w.Length, 1)
                End If

                Return _value2D
            End Get
        End Property

        ''' <summary>梯度数据的二维列向量视图 (n, 1)：作为矩阵乘的 gradOutput</summary>
        Friend ReadOnly Property Grad2D As Tensor
            Get
                If dw Is Nothing Then Return Nothing

                If _grad2D Is Nothing OrElse Not Object.ReferenceEquals(_grad2D.Data, dw) Then
                    _grad2D = Tensor.Wrap(dw, dw.Length, 1)
                End If

                Return _grad2D
            End Get
        End Property

        ' ------------------------------------------------------------------
        ' 设备端缓存一致性
        '
        ' <see cref="Value"/> / <see cref="Grad"/> 是 <see cref="w"/> / <see cref="dw"/> 的**零拷贝**张量视图，
        ' 而本类型的大量写入（addImageData / setWeight / addGradient / clearGradient ...）都是
        ' 绕过 Tensor 索引器直接改写底层数组的。CUDA 后端会把主机数组缓存到显存，
        ' 缓存键是"(数组引用, 张量版本号)"，版本号只会在通过 Tensor 索引器写入、
        ' 或者调用 Tensor.InvalidateAllDeviceCaches() 时才变化。
        '
        ' 因此：**任何绕过 Tensor 索引器的就地写入，都必须紧跟一次 MarkXxxModified()**，
        ' 否则 GPU 侧会一直复用旧数据，造成静默的数值错误（实测表现为训练结果与 CPU 不一致）。
        ' ------------------------------------------------------------------

        ''' <summary>把整块数据写入 <see cref="w"/>，并声明值数据的设备端缓存已失效</summary>
        Friend Sub SetValues(data As Double())
            Array.ConstrainedCopy(data, Scan0, w, Scan0, w.Length)
            Call MarkValueModified()
        End Sub

        ''' <summary>把整块数据写入 <see cref="dw"/>，并声明梯度数据的设备端缓存已失效</summary>
        Friend Sub SetGradients(data As Double())
            Array.ConstrainedCopy(data, Scan0, dw, Scan0, dw.Length)
            Call MarkGradientModified()
        End Sub

        ''' <summary>声明值数据的设备端缓存已失效（就地改写 <see cref="w"/> 之后必须调用）</summary>
        ''' <remarks>
        ''' 需要把本块持有的**全部**视图都标记一遍：三维的 <see cref="Value"/>、四维的 <see cref="Value4D"/>、
        ''' 二维的 <see cref="Value2D"/>。它们各自持有独立的版本号，少标一个就会出现
        ''' "某个算子读到旧值"的静默错误。
        ''' 由于视图都由本块缓存，这里已经不需要再调用全局的 <c>Tensor.InvalidateAllDeviceCaches</c>，
        ''' 失效粒度从"所有张量"收敛到"本块"。
        ''' </remarks>
        Friend Sub MarkValueModified()
            Call MarkHostModified(Value)
            Call MarkHostModified(_value4D)
            Call MarkHostModified(_value2D)
        End Sub

        ''' <summary>声明梯度数据的设备端缓存已失效（就地改写 <see cref="dw"/> 之后必须调用）</summary>
        Friend Sub MarkGradientModified()
            Call MarkHostModified(Grad)
            Call MarkHostModified(_grad4D)
            Call MarkHostModified(_grad2D)
        End Sub

        ''' <summary>对已创建的视图逐个推进版本号（未创建的视图本来就没有显存副本，无需处理）</summary>
        Private Shared Sub MarkHostModified(t As Tensor)
            If t IsNot Nothing Then Call t.MarkHostModified()
        End Sub

        ''' <summary>Creates an empty block, used by the deserializer.</summary>
        Sub New()
        End Sub

        ''' <summary>
        ''' Creates a block of the given size with randomly initialized weights.
        ''' </summary>
        ''' <param name="sx">Width.</param>
        ''' <param name="sy">Height.</param>
        ''' <param name="depth">Number of channels.</param>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub New(sx As Integer, sy As Integer, depth As Integer)
            Me.New(sx, sy, depth, -1.0)
        End Sub

        ''' <summary>
        ''' Creates a block from a <see cref="Dimension"/> and a weight initialization value.
        ''' </summary>
        ''' <param name="dims">The block dimensions.</param>
        ''' <param name="depth">Number of channels.</param>
        ''' <param name="c">The constant used to fill the weights, or <c>-1</c> for random initialization.</param>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Sub New(dims As Dimension, depth As Integer, c As Double)
            Call Me.New(dims.x, dims.y, depth, c)
        End Sub

        ''' <summary>
        ''' Creates a block of the given size.
        ''' </summary>
        ''' <param name="sx">Width.</param>
        ''' <param name="sy">Height.</param>
        ''' <param name="depth">Number of channels.</param>
        ''' <param name="c">
        ''' Constant used to initialize the weight vector; when it equals <c>-1</c> the weights are randomly initialized
        ''' instead.
        ''' </param>
        Public Sub New(sx As Integer, sy As Integer, depth As Integer, c As Double)
            Dim n = sx * sy * depth

            _SX = sx
            _SY = sy
            _Depth = depth

            dw = New Double(n - 1) {}

            If c <> -1.0 Then
                w = c.Replicate(n).ToArray
            Else
                w = Vector.rand(-1, 1, size:=n) * std.Sqrt(1.0 / n)
            End If
        End Sub

        ''' <summary>Returns a short diagnostic description of the block.</summary>
        ''' <returns>A text that reports the shape, the trace label and the first few weight values.</returns>
        Public Overrides Function ToString() As String
            Dim sb As String = $"shape(w:{SX}, h:{SY}, channels_depth:{Depth})[{Weights.Length}]"
            Dim njs As String = w.Take(13).JoinBy(", ")

            If trace.StringEmpty Then
                Return sb & $" [from_unknown] [{njs}...]"
            Else
                Return $"{sb} [{trace}] [{njs}...]"
            End If
        End Function

        ''' <summary>
        ''' Prepares the input by copying the pixels into the weight vector and normalizing them into <c>[-0.5, 0.5]</c>.
        ''' </summary>
        ''' <param name="imgData">The raw byte pixel data.</param>
        ''' <param name="maxvalue">The maximum possible channel value, normally 255.</param>
        Public Overridable Sub addImageData(imgData As Byte(), maxvalue As Byte)
            Dim max As Double = maxvalue

            For i As Integer = 0 To imgData.Length - 1
                w(i) = imgData(i) / max - 0.5 ' normalize image pixels to [-0.5, 0.5]
            Next

            Call MarkValueModified()
        End Sub

        ''' <summary>
        ''' Prepares the input by copying the pixels into the weight vector and normalizing them into <c>[-0.5, 0.5]</c>.
        ''' </summary>
        ''' <param name="imgData">The raw pixel data.</param>
        ''' <param name="maxvalue">The maximum possible channel value.</param>
        Public Overridable Sub addImageData(imgData As Double(), maxvalue As Double)
            For i As Integer = 0 To imgData.Length - 1
                w(i) = imgData(i) / maxvalue - 0.5 ' normalize image pixels to [-0.5, 0.5]
            Next

            Call MarkValueModified()
        End Sub

        ''' <summary>
        ''' Prepares the input by copying the pixels into the weight vector and normalizing them into <c>[-0.5, 0.5]</c>.
        ''' </summary>
        ''' <param name="imgData">The raw integer pixel data.</param>
        ''' <param name="maxvalue">The maximum possible channel value.</param>
        Public Overridable Sub addImageData(imgData As Integer(), maxvalue As Integer)
            Dim max As Double = maxvalue

            For i As Integer = 0 To imgData.Length - 1
                w(i) = imgData(i) / max - 0.5 ' normalize image pixels to [-0.5, 0.5]
            Next

            Call MarkValueModified()
        End Sub

        ''' <summary>
        ''' set <see cref="w"/>
        ''' </summary>
        ''' <param name="ix"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overridable Function getWeight(ix As Integer) As Double
            Return w(ix)
        End Function

        ''' <summary>
        ''' Gets the weight at the given three dimensional coordinate.
        ''' </summary>
        ''' <param name="x">The column index.</param>
        ''' <param name="y">The row index.</param>
        ''' <param name="depth">The channel index.</param>
        ''' <returns>The weight value.</returns>
        Public Overridable Function getWeight(x As Integer, y As Integer, depth As Integer) As Double
            Dim ix = (_SX * y + x) * _Depth + depth
            Return w(ix)
        End Function

        ''' <summary>
        ''' get <see cref="w"/>
        ''' </summary>
        ''' <param name="ix"></param>
        ''' <param name="val"></param>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overridable Sub setWeight(ix As Integer, val As Double)
            w(ix) = val
            Call MarkValueModified()
        End Sub

        ''' <summary>
        ''' Sets the weight at the given three dimensional coordinate.
        ''' </summary>
        ''' <param name="x">The column index.</param>
        ''' <param name="y">The row index.</param>
        ''' <param name="depth">The channel index.</param>
        ''' <param name="val">The value to store.</param>
        Public Overridable Sub setWeight(x As Integer, y As Integer, depth As Integer, val As Double)
            Dim ix = (_SX * y + x) * _Depth + depth
            setWeight(ix, val)
        End Sub

        ''' <summary>
        ''' Adds a value to the weight at the given three dimensional coordinate.
        ''' </summary>
        ''' <param name="x">The column index.</param>
        ''' <param name="y">The row index.</param>
        ''' <param name="depth">The channel index.</param>
        ''' <param name="val">The value to add.</param>
        Public Overridable Sub addWeight(x As Integer, y As Integer, depth As Integer, val As Double)
            Dim ix = (_SX * y + x) * _Depth + depth
            w(ix) += val
            Call MarkValueModified()
        End Sub

        ''' <summary>
        ''' Gets the gradient at the given three dimensional coordinate.
        ''' </summary>
        ''' <param name="x">The column index.</param>
        ''' <param name="y">The row index.</param>
        ''' <param name="depth">The channel index.</param>
        ''' <returns>The gradient value.</returns>
        Public Overridable Function getGradient(x As Integer, y As Integer, depth As Integer) As Double
            Dim ix = (_SX * y + x) * _Depth + depth
            Return getGradient(ix)
        End Function

        ''' <summary>
        ''' get <see cref="Gradients"/>
        ''' </summary>
        ''' <param name="ix"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' get element value from <see cref="dw"/>
        ''' </remarks>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overridable Function getGradient(ix As Integer) As Double
            Return dw(ix)
        End Function

        ''' <summary>
        ''' Sets the gradient at the given three dimensional coordinate.
        ''' </summary>
        ''' <param name="x">The column index.</param>
        ''' <param name="y">The row index.</param>
        ''' <param name="depth">The channel index.</param>
        ''' <param name="val">The value to store.</param>
        Public Overridable Sub setGradient(x As Integer, y As Integer, depth As Integer, val As Double)
            Dim ix = (_SX * y + x) * _Depth + depth
            setGradient(ix, val)
        End Sub

        ''' <summary>
        ''' set element value to <see cref="Gradients"/>
        ''' </summary>
        ''' <param name="ix"></param>
        ''' <param name="val"></param>
        ''' <remarks>
        ''' set value to <see cref="dw"/>
        ''' </remarks>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overridable Sub setGradient(ix As Integer, val As Double)
            dw(ix) = val
            Call MarkGradientModified()
        End Sub

        ''' <summary>
        ''' Replaces the whole gradient vector with the given values.
        ''' </summary>
        ''' <param name="val">The new gradient values.</param>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub setGradient(val As Double())
            Array.ConstrainedCopy(val, Scan0, dw, Scan0, dw.Length)
            Call MarkGradientModified()
        End Sub

        ''' <summary>
        ''' Adds a value to the gradient at the given three dimensional coordinate.
        ''' </summary>
        ''' <param name="x">The column index.</param>
        ''' <param name="y">The row index.</param>
        ''' <param name="depth">The channel index.</param>
        ''' <param name="val">The value to add.</param>
        Public Overridable Sub addGradient(x As Integer, y As Integer, depth As Integer, val As Double)
            Dim ix = (_SX * y + x) * _Depth + depth
            addGradient(ix, val)
        End Sub

        ''' <summary>
        ''' Adds a value to the gradient at the given flat index.
        ''' </summary>
        ''' <param name="ix">The flat index into the gradient vector.</param>
        ''' <param name="val">The value to add.</param>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overridable Sub addGradient(ix As Integer, val As Double)
            dw(ix) += val
            Call MarkGradientModified()
        End Sub

        ''' <summary>
        ''' Subtracts a value from the gradient at the given flat index.
        ''' </summary>
        ''' <param name="ix">The flat index into the gradient vector.</param>
        ''' <param name="val">The value to subtract.</param>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overridable Sub subGradient(ix As Integer, val As Double)
            dw(ix) -= val
            Call MarkGradientModified()
        End Sub

        ''' <summary>
        ''' <paramref name="val"/> * <see cref="Gradients"/>
        ''' </summary>
        ''' <param name="val"></param>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overridable Sub mulGradient(val As Double)
            dw = SIMD.Multiply.f64_scalar_op_multiply_f64(val, dw)
        End Sub

        ''' <summary>
        ''' Creates a new block with the same shape and trace but with all weights zeroed.
        ''' </summary>
        ''' <returns>A zero filled copy of this block.</returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overridable Function cloneAndZero() As DataBlock
            Return New DataBlock(_SX, _SY, _Depth, 0.0) With {.trace = trace}
        End Function

        ''' <summary>
        ''' make a copy of <see cref="w"/> or <see cref="Weights"/>
        ''' </summary>
        ''' <returns></returns>
        Public Overridable Function clone() As DataBlock
            Dim db As New DataBlock(_SX, _SY, _Depth, 0.0) With {.trace = trace}

            For i As Integer = 0 To w.Length - 1
                db.w(i) = w(i)
            Next

            Return db
        End Function

        ''' <summary>
        ''' just set <see cref="dw"/> vector to zero
        ''' </summary>
        Public Function clearGradient() As DataBlock
            Call dw.fill(0)
            Call MarkGradientModified()
            Return Me
        End Function

        ''' <summary>
        ''' Copies the weights of another block into this one.
        ''' </summary>
        ''' <param name="db">The source block; it must have the same length.</param>
        Public Overridable Sub addFrom(db As DataBlock)
            For i As Integer = 0 To w.Length - 1
                w(i) = db.w(i)
            Next
        End Sub

        ''' <summary>
        ''' Copies the weights of another block into this one, scaled by a constant factor.
        ''' </summary>
        ''' <param name="db">The source block; it must have the same length.</param>
        ''' <param name="a">The scale factor applied to every source weight.</param>
        Public Overridable Sub addFromScaled(db As DataBlock, a As Double)
            For i As Integer = 0 To w.Length - 1
                w(i) = db.w(i) * a
            Next
        End Sub
    End Class
End Namespace
