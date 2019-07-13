#Region "Microsoft.VisualBasic::69ceea95906ed80e045120aa58ab0bf6, Microsoft.VisualBasic.Core\Extensions\Image\Math\Models\Vector2D.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    '     Class Vector2D
    ' 
    '         Properties: Length
    ' 
    '         Constructor: (+3 Overloads) Sub New
    '         Operators: -, (+2 Overloads) *
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Imaging.Math2D

    ''' <summary>
    ''' <see cref="Drawing.PointF"/>
    ''' </summary>
    Public Class Vector2D

        Public x As Double
        Public y As Double

        Public ReadOnly Property Length As Double
            Get
                Return Math.Sqrt(x ^ 2 + y ^ 2)
            End Get
        End Property

        Public Sub New()
            Me.New(0.0, 0.0)
        End Sub

        Public Sub New(x As Double, y As Double)
            Me.x = x
            Me.y = y
        End Sub

        Public Sub New(x As Integer, y As Integer)
            Me.x = x
            Me.y = y
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
