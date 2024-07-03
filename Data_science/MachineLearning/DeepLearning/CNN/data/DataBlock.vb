#Region "Microsoft.VisualBasic::bd1afc73c078b993d0bc414191f68dc1, Data_science\MachineLearning\DeepLearning\CNN\data\DataBlock.vb"

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

    '   Total Lines: 284
    '    Code Lines: 157 (55.28%)
    ' Comment Lines: 85 (29.93%)
    '    - Xml Docs: 96.47%
    ' 
    '   Blank Lines: 42 (14.79%)
    '     File Size: 10.01 KB


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
Imports Microsoft.VisualBasic.Language.Java
Imports Microsoft.VisualBasic.Math
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports std = System.Math

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
        End Sub

        Public Overridable Sub setWeight(x As Integer, y As Integer, depth As Integer, val As Double)
            Dim ix = (_SX * y + x) * _Depth + depth
            setWeight(ix, val)
        End Sub

        Public Overridable Sub addWeight(x As Integer, y As Integer, depth As Integer, val As Double)
            Dim ix = (_SX * y + x) * _Depth + depth
            w(ix) += val
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
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub setGradient(val As Double())
            Array.ConstrainedCopy(val, Scan0, dw, Scan0, dw.Length)
        End Sub

        Public Overridable Sub addGradient(x As Integer, y As Integer, depth As Integer, val As Double)
            Dim ix = (_SX * y + x) * _Depth + depth
            addGradient(ix, val)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overridable Sub addGradient(ix As Integer, val As Double)
            dw(ix) += val
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overridable Sub subGradient(ix As Integer, val As Double)
            dw(ix) -= val
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
