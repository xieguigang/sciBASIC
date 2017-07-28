
Namespace Imaging.Math2D

    Public Class Vector2D

        Public x As Double
        Public y As Double

        Public ReadOnly Property Length() As Double
            Get
                Return Math.Sqrt(x ^ 2 + y ^ 2)
            End Get
        End Property

        Public Sub New()
            Me.New(0.0, 0.0)
        End Sub

        Public Sub New(paramDouble1 As Double, paramDouble2 As Double)
            Me.x = paramDouble1
            Me.y = paramDouble2
        End Sub

        Public Sub New(paramInt1 As Integer, paramInt2 As Integer)
            Me.x = paramInt1
            Me.y = paramInt2
        End Sub

        ''' <summary>
        ''' reverse
        ''' </summary>
        ''' <param name="v"></param>
        ''' <returns></returns>
        Public Shared Operator -(v As Vector2D) As Vector2D
            With v
                Return New Vector2D(- .x, - .y)
            End With
        End Operator

        ''' <summary>
        ''' multiple
        ''' </summary>
        ''' <param name="scale#"></param>
        ''' <param name="v"></param>
        ''' <returns></returns>
        Public Shared Operator *(scale#, v As Vector2D) As Vector2D
            With v
                Return New Vector2D(scale * .x, scale * .y)
            End With
        End Operator

        ''' <summary>
        ''' multiple
        ''' </summary>
        ''' <param name="v"></param>
        ''' <param name="scale#"></param>
        ''' <returns></returns>
        Public Shared Operator *(v As Vector2D, scale#) As Vector2D
            With v
                Return New Vector2D(scale * .x, scale * .y)
            End With
        End Operator
    End Class
End Namespace
