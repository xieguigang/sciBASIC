Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Data.visualize.Network.Layouts.Circular

Namespace NetworkEditor.Layout

    Public Class CircularRunner : Implements ILayoutRunner

        Private params As New CircularLayoutParameters

        Public ReadOnly Property Name As String Implements ILayoutRunner.Name
            Get
                Return "Circular"
            End Get
        End Property

        Public Function GetParameters() As Object Implements ILayoutRunner.GetParameters
            Return params
        End Function

        Public Sub Apply(g As NetworkGraph, p As Object, Optional progress As Action(Of String) = Nothing) Implements ILayoutRunner.Apply
            Dim par = DirectCast(p, CircularLayoutParameters)
            params = par
            Call CircularLayout.LayoutNodes(g, par)
        End Sub
    End Class

End Namespace
