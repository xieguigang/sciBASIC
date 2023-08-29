Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language.Java
Imports randf = Microsoft.VisualBasic.Math.RandomExtensions
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

        Public Overridable ReadOnly Property SX As Integer
        Public Overridable ReadOnly Property SY As Integer
        Public Overridable ReadOnly Property Depth As Integer

        ''' <summary>
        ''' the multiple class classify probability weight
        ''' (or the prediction result) 
        ''' </summary>
        ''' <returns></returns>
        Public Overridable ReadOnly Property Weights As Double()
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return w
            End Get
        End Property

        Public Overridable ReadOnly Property Gradients As Double()
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return dw
            End Get
        End Property

        Dim w As Double()
        Dim dw As Double()

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub New(sx As Integer, sy As Integer, depth As Integer)
            Me.New(sx, sy, depth, -1.0)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Sub New(dims As Dimension, depth As Integer, c As Double)
            Call Me.New(dims.x, dims.y, depth, c)
        End Sub

        Public Sub New(sx As Integer, sy As Integer, depth As Integer, c As Double)
            Dim n = sx * sy * depth

            _SX = sx
            _SY = sy
            _Depth = depth

            w = New Double(n - 1) {}
            dw = New Double(n - 1) {}

            If c <> -1.0 Then
                w.fill(c)
            Else
                Dim scale = std.Sqrt(1.0 / n)

                For i = 0 To n - 1
                    w(i) = randf.NextDouble() * scale
                Next
            End If
            dw.fill(0)
        End Sub

        ''' <summary>
        ''' prepare the input: get pixels and normalize them
        ''' </summary>
        ''' <param name="imgData"></param>
        ''' <param name="maxvalue"></param>
        Public Overridable Sub addImageData(imgData As Byte(), maxvalue As Byte)
            Dim max As Double = maxvalue

            For i = 0 To imgData.Length - 1
                w(i) = imgData(i) / max - 0.5 ' normalize image pixels to [-0.5, 0.5]
            Next
        End Sub

        ''' <summary>
        ''' prepare the input: get pixels and normalize them
        ''' </summary>
        ''' <param name="imgData"></param>
        ''' <param name="maxvalue"></param>
        Public Overridable Sub addImageData(imgData As Double(), maxvalue As Double)
            For i = 0 To imgData.Length - 1
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

            For i = 0 To imgData.Length - 1
                w(i) = imgData(i) / max - 0.5 ' normalize image pixels to [-0.5, 0.5]
            Next
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overridable Function getWeight(ix As Integer) As Double
            Return w(ix)
        End Function

        Public Overridable Function getWeight(x As Integer, y As Integer, depth As Integer) As Double
            Dim ix = (_SX * y + x) * _Depth + depth
            Return w(ix)
        End Function

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

        Public Overridable Function getGradient(ix As Integer) As Double
            Return dw(ix)
        End Function

        Public Overridable Sub setGradient(x As Integer, y As Integer, depth As Integer, val As Double)
            Dim ix = (_SX * y + x) * _Depth + depth
            setGradient(ix, val)
        End Sub

        Public Overridable Sub setGradient(ix As Integer, val As Double)
            dw(ix) = val
        End Sub


        Public Overridable Sub addGradient(x As Integer, y As Integer, depth As Integer, val As Double)
            Dim ix = (_SX * y + x) * _Depth + depth
            addGradient(ix, val)
        End Sub

        Public Overridable Sub addGradient(ix As Integer, val As Double)
            dw(ix) += val
        End Sub

        Public Overridable Sub subGradient(ix As Integer, val As Double)
            dw(ix) -= val
        End Sub

        Public Overridable Sub mulGradient(ix As Integer, val As Double)
            dw(ix) *= val
        End Sub


        Public Overridable Function cloneAndZero() As DataBlock
            Return New DataBlock(_SX, _SY, _Depth, 0.0)
        End Function

        Public Overridable Function clone() As DataBlock
            Dim db As DataBlock = New DataBlock(_SX, _SY, _Depth, 0.0)
            For i = 0 To w.Length - 1
                db.w(i) = w(i)
            Next
            Return db
        End Function

        Public Overridable Sub clearGradient()
            dw.fill(0)
        End Sub

        Public Overridable Sub addFrom(db As DataBlock)
            For i = 0 To w.Length - 1
                w(i) = db.w(i)
            Next
        End Sub

        Public Overridable Sub addFromScaled(db As DataBlock, a As Double)
            For i = 0 To w.Length - 1
                w(i) = db.w(i) * a
            Next
        End Sub
    End Class
End Namespace
