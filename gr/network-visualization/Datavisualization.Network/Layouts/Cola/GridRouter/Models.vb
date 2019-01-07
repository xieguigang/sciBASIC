Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Imaging.LayoutModel
Imports number = System.Double

Namespace Layouts.Cola.GridRouter

    Public Interface NodeAccessor(Of Node)
        Function getChildren(v As Node) As number()
        Function getBounds(v As Node) As Rectangle2D
    End Interface

    Public Class NodeWrapper
        Public leaf As Boolean
        Public parent As NodeWrapper
        Public ports As Vert()
        Public id As Integer
        Public rect As Rectangle2D
        Public children As Integer()

        Public Sub New(id As number, rect As Rectangle2D, children As Integer())
            Me.id = id
            Me.rect = rect
            Me.children = children

            leaf = children.IsNullOrEmpty
        End Sub
    End Class

    Public Class Vert

        Public id As number
        Public x As number
        Public y As number
        Public node As NodeWrapper
        Public line

        Sub New(id As number, x As number, y As number, Optional node As NodeWrapper = Nothing, Optional line As Object = Nothing)
            Me.id = id
            Me.x = x
            Me.y = y
            Me.node = node
            Me.line = line
        End Sub
    End Class

    Public Class [Event]
        Public type As Integer
        Public s As route
        Public pos As number

        Public Structure Comparer : Implements IComparer(Of [Event])

            Public Function Compare(a As [Event], b As [Event]) As Integer Implements IComparer(Of [Event]).Compare
                Return a.pos - b.pos + a.type - b.type
            End Function
        End Structure
    End Class

    ''' <summary>
    ''' a horizontal Or vertical line of nodes
    ''' </summary>
    Public Class GridLine

        Public nodes As NodeWrapper()
        Public pos As number

        Public Structure Comparer : Implements IComparer(Of GridLine)

            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Public Function Compare(x As GridLine, y As GridLine) As Integer Implements IComparer(Of GridLine).Compare
                Return x.pos - y.pos
            End Function
        End Structure
    End Class
End Namespace