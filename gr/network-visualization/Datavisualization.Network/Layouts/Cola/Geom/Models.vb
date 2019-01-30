Imports Microsoft.VisualBasic.Imaging.LayoutModel

Namespace Layouts.Cola

    Public Class PolyPoint : Inherits Point2D

        Public Property polyIndex() As Integer

    End Class

    Public Class tangentPoly

        ''' <summary>
        ''' index of rightmost tangent point V[rtan]
        ''' </summary>
        ''' <returns></returns>
        Public Property rtan() As Integer
        ''' <summary>
        ''' index of leftmost tangent point V[ltan]
        ''' </summary>
        ''' <returns></returns>
        Public Property ltan() As Integer

    End Class

    Public Class BiTangent

        Public t1 As Integer, t2 As Integer

        Public Sub New()
        End Sub

        Public Sub New(t1 As Integer, t2 As Integer)
            Me.t1 = t1
            Me.t2 = t2
        End Sub
    End Class

    Public Class BiTangents
        Public rl As BiTangent
        Public lr As BiTangent
        Public ll As BiTangent
        Public rr As BiTangent
    End Class

    Public Class TVGPoint : Inherits Point2D
        Public vv As VisibilityVertex
    End Class

    Public Class VisibilityVertex

        Public id As Double
        Public polyid As Double
        Public polyvertid As Double
        Public p As TVGPoint

        Public Sub New(id As Double, polyid As Double, polyvertid As Double, p As TVGPoint)
            p.vv = Me
            Me.id = id
            Me.polyid = polyid
            Me.polyvertid = polyvertid
            Me.p = p
        End Sub
    End Class

    Public Class VisibilityEdge
        Public source As VisibilityVertex
        Public target As VisibilityVertex

        Private Sub New(source As VisibilityVertex, target As VisibilityVertex)
            Me.source = source

            Me.target = target
        End Sub
        Public ReadOnly Property length() As Double
            Get
                Dim dx = Me.source.p.X - Me.target.p.X
                Dim dy = Me.source.p.Y - Me.target.p.Y
                Return Math.Sqrt(dx * dx + dy * dy)
            End Get
        End Property
    End Class
End Namespace