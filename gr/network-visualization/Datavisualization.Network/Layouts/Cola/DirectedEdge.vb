Imports Microsoft.VisualBasic.ComponentModel.Algorithm.BinaryTree
Imports Microsoft.VisualBasic.Imaging.LayoutModel
Imports number = System.Double

Namespace Layouts.Cola

    ''' <summary>
    ''' An object with three point properties, the intersection with the
    ''' source rectangle (sourceIntersection), the intersection with then
    ''' target rectangle (targetIntersection), And the point an arrow
    ''' head of the specified size would need to start (arrowStart).
    ''' </summary>
    Public Structure DirectedEdge

        Public sourceIntersection As Point2D
        Public targetIntersection As Point2D
        Public arrowStart As Point2D

    End Structure

    Public Class Node

        Public prev As RBNode(Of Node, Object)
        Public [next] As RBNode(Of Node, Object)

        Public r As Rectangle2D
        Public pos As Number

        Public Shared Function makeRBTree() As RBNode(Of Node, Object)
            Return New RBNode(Of Node, Object)(Nothing, Nothing)
        End Function

    End Class

    Public Class Variable
        Public offset As number = 0
        Public block As Block
        Public cIn As Constraint()
        Public cOut As Constraint()
        Public weight As number = 1
        Public scale As number = 1
        Public desiredPosition As number

        Public ReadOnly Property dfdv As number
            Get
                Return 2 * weight * (position - desiredPosition)
            End Get
        End Property

        Public ReadOnly Property position As number
            Get
                Return (block.ps.scale * block.posn + offset) / scale
            End Get
        End Property

        Public Sub visitNeighbours(prev As Variable, f As Action(Of Constraint, Variable))
            Dim ff = Sub(c As Constraint, [next] As Variable)
                         If c.active AndAlso Not prev Is [next] Then
                             Call f(c, [next])
                         End If
                     End Sub

            cOut.ForEach(Sub(c, i) ff(c, c.right))
            cIn.ForEach(Sub(c, i) ff(c, c.left))
        End Sub
    End Class

    Public Class Block
        Public ps As PositionStats
        Public posn As number
        Public vars As New List(Of Variable)
        Public blockId As number

        Sub New(v As Variable)
            v.offset = 0
            ps = New PositionStats(v.scale)
            addVariable(v)
        End Sub

        Private Sub addVariable(v As Variable)
            v.block = Me
            vars.Add(v)
            ps.addVariable(v)
            posn = ps.Posn
        End Sub

        ''' <summary>
        ''' move the block where it needs to be to minimize cost
        ''' </summary>
        Sub updateWeightedPosition()
            ps.AB = ps.AD = ps.A2 = 0

            For i As Integer = 0 To vars.Count - 1
                ps.addVariable(vars(i))
            Next

            posn = ps.Posn
        End Sub
    End Class

    Public Class PositionStats
        Public scale As number

        Public AB As number = 0
        Public AD As number = 0
        Public A2 As number = 0

        Public ReadOnly Property Posn As number
            Get
                Return (AD - AB) / A2
            End Get
        End Property

        Sub New(scale As number)
            Me.scale = scale
        End Sub

        Public Sub addVariable(v As Variable)
            Dim ai = scale / v.scale
            Dim bi = v.offset / v.scale
            Dim wi = v.weight

            AB += wi * ai * bi
            AD += wi * ai * v.desiredPosition
            A2 += wi * ai * ai
        End Sub
    End Class

    Public Class Constraint
        Public lm As number
        Public active As Boolean = False
        Public unsatisfiable As Boolean = False

        Public left As Variable
        Public right As Variable
        Public gap As number
        Public equality As Boolean = False

        Public ReadOnly Property slack As number
            Get
                If unsatisfiable Then
                    Return number.MaxValue
                Else
                    Return right.scale * right.position - gap - left.scale * left.position
                End If
            End Get
        End Property
    End Class
End Namespace