Imports System.Runtime.CompilerServices
Imports std = System.Math

Namespace EmGaussian

    ''' <summary>
    ''' A possible signle peak
    ''' </summary>
    Public Class Variable

        ''' <summary>
        ''' height,center,width,offset
        ''' </summary>
        Friend Const argument_size As Integer = 4

        Public Property height As Double
        Public Property center As Double
        Public Property width As Double
        Public Property offset As Double

        Sub New()
        End Sub

        Sub New(center As Double, width As Double, height As Double, Optional offset As Double = 0)
            Me.center = center
            Me.width = width
            Me.height = height
            Me.offset = offset
        End Sub

        ''' <summary>
        ''' make data copy
        ''' </summary>
        ''' <param name="clone"></param>
        Sub New(clone As Variable)
            height = clone.height
            center = clone.center
            width = clone.width
            offset = clone.offset
        End Sub

        Public Overrides Function ToString() As String
            Return $"gaussian(x) = {height.ToString("G3")} * exp(-((x - {center.ToString("G3")}) ^ 2) / (2 * ({width.ToString("G3")} ^ 2))) + {offset.ToString("G3")}"
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function gaussian(x As Double) As Double
            Return height * std.Exp(-((x - center) ^ 2) / (2 * (width ^ 2))) + offset
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function Multi_gaussian(x As Double, vars As Variable(), offset As Double) As Double
            Return offset + Aggregate c As Variable In vars Into Sum(c.gaussian(x))
        End Function

    End Class
End Namespace