#Region "Microsoft.VisualBasic::cd786f444aa1a1a78435a1eeb188526d, Data_science\MachineLearning\DeepLearning\CNN\data\DataBlock.vb"

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

    '   Total Lines: 283
    '    Code Lines: 156 (55.12%)
    ' Comment Lines: 85 (30.04%)
    '    - Xml Docs: 96.47%
    ' 
    '   Blank Lines: 42 (14.84%)
    '     File Size: 9.95 KB


    '     Class DataBlock
    ' 
    '         Properties: Depth, Gradients, SX, SY, trace
    '                     Weights
    ' 
    '         Constructor: (+4 Overloads) Sub New
    ' 
    '         Function: clearGradient, clone, cloneAndZero, (+2 Overloads) getGradient, (+2 Overloads) getWeight
    '                   ToString
    ' 
    '         Sub: addFrom, addFromScaled, (+2 Overloads) addGradient, (+3 Overloads) addImageData, addWeight
    '              mulGradient, (+3 Overloads) setGradient, (+2 Overloads) setWeight, subGradient
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

        Public ReadOnly Property SX As Integer
        Public ReadOnly Property SY As Integer
        Public ReadOnly Property Depth As Integer

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

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>get <see cref="dw"/></remarks>
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
        Friend Sub MarkValueModified()
            Dim t = Value

            If t IsNot Nothing Then Call t.MarkHostModified()
        End Sub

        ''' <summary>声明梯度数据的设备端缓存已失效（就地改写 <see cref="dw"/> 之后必须调用）</summary>
        Friend Sub MarkGradientModified()
            Dim t = Grad

            If t IsNot Nothing Then Call t.MarkHostModified()
        End Sub

        Sub New()
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub New(sx As Integer, sy As Integer, depth As Integer)
            Me.New(sx, sy, depth, -1.0)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Sub New(dims As Dimension, depth As Integer, c As Double)
            Call Me.New(dims.x, dims.y, depth, c)
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="sx"></param>
        ''' <param name="sy"></param>
        ''' <param name="depth"></param>
        ''' <param name="c">use for initialize the weight vector: 
        ''' weight vector will be filled with the value of this parameter.
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
        ''' prepare the input: get pixels and normalize them
        ''' </summary>
        ''' <param name="imgData"></param>
        ''' <param name="maxvalue"></param>
        Public Overridable Sub addImageData(imgData As Byte(), maxvalue As Byte)
            Dim max As Double = maxvalue

            For i As Integer = 0 To imgData.Length - 1
                w(i) = imgData(i) / max - 0.5 ' normalize image pixels to [-0.5, 0.5]
            Next

            Call MarkValueModified()
        End Sub

        ''' <summary>
        ''' prepare the input: get pixels and normalize them
        ''' </summary>
        ''' <param name="imgData"></param>
        ''' <param name="maxvalue"></param>
        Public Overridable Sub addImageData(imgData As Double(), maxvalue As Double)
            For i As Integer = 0 To imgData.Length - 1
                w(i) = imgData(i) / maxvalue - 0.5 ' normalize image pixels to [-0.5, 0.5]
            Next

            Call MarkValueModified()
        End Sub

        ''' <summary>
        ''' prepare the input: get pixels and normalize them
        ''' </summary>
        ''' <param name="imgData"></param>
        ''' <param name="maxvalue"></param>
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

        Public Overridable Sub setWeight(x As Integer, y As Integer, depth As Integer, val As Double)
            Dim ix = (_SX * y + x) * _Depth + depth
            setWeight(ix, val)
        End Sub

        Public Overridable Sub addWeight(x As Integer, y As Integer, depth As Integer, val As Double)
            Dim ix = (_SX * y + x) * _Depth + depth
            w(ix) += val
            Call MarkValueModified()
        End Sub

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

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub setGradient(val As Double())
            Array.ConstrainedCopy(val, Scan0, dw, Scan0, dw.Length)
            Call MarkGradientModified()
        End Sub

        Public Overridable Sub addGradient(x As Integer, y As Integer, depth As Integer, val As Double)
            Dim ix = (_SX * y + x) * _Depth + depth
            addGradient(ix, val)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overridable Sub addGradient(ix As Integer, val As Double)
            dw(ix) += val
            Call MarkGradientModified()
        End Sub

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

        Public Overridable Sub addFrom(db As DataBlock)
            For i As Integer = 0 To w.Length - 1
                w(i) = db.w(i)
            Next
        End Sub

        Public Overridable Sub addFromScaled(db As DataBlock, a As Double)
            For i As Integer = 0 To w.Length - 1
                w(i) = db.w(i) * a
            Next
        End Sub
    End Class
End Namespace
