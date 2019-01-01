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
        Public id As number
        Public rect As Rectangle2D
        Public children As number()

        Public Sub New(id As number, rect As Rectangle2D, children As number())
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

    ''' <summary>
    ''' a horizontal Or vertical line of nodes
    ''' </summary>
    Interface GridLine
        Property nodes As NodeWrapper()
        Property pos As number
    End Interface
End Namespace