Imports Microsoft.VisualBasic.Data.visualize.Network.Layouts.Cola.GridRouter

Namespace Layouts.Cola

    Public Class IConstraint
        Public Property axis As String
        Public Property left() As Double
        Public Property right() As Double
        Public Property gap() As Double
    End Class


    Public Interface DirectedEdgeConstraints
        Property axis() As String
        Property gap() As Double
    End Interface

    Public Class LinkSepAccessor(Of Link)
        Inherits LinkAccessor(Of Link)

        Public Delegate Function IGetMinSeperation(l As Link) As Double

        Public Property getMinSeparation As IGetMinSeperation
    End Class
End Namespace