Imports Microsoft.VisualBasic.Imaging.LayoutModel
Imports number = System.Double

Namespace Layouts.Cola

    Public Class Leaf
        Public bounds As Rectangle2D
        Public variable As Variable
    End Class

    Public Class ProjectionGroup
        Public bounds As Rectangle2D
        Public padding As Number
        Public stiffness As Number
        Public leaves As Leaf()
        Public groups As ProjectionGroup()
        Public minVar As Variable
        Public maxVar As Variable
    End Class

    Public Class [Event]

        Public isOpen As Boolean
        Public v As Node
        Public pos As number

        Sub New(isOpen As Boolean, v As Node, pos As number)
            Me.isOpen = isOpen
            Me.v = v
            Me.pos = pos
        End Sub

    End Class
End Namespace