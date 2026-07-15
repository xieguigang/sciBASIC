Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Data.visualize.Network.Layouts.Radial

Namespace NetworkEditor.Layout

    Public Class RadialRunner : Implements ILayoutRunner

        Private params As New RadialLayoutParameters

        Public ReadOnly Property Name As String Implements ILayoutRunner.Name
            Get
                Return "Radial"
            End Get
        End Property

        Public Function GetParameters() As Object Implements ILayoutRunner.GetParameters
            Return params
        End Function

        Public Sub Apply(g As NetworkGraph, p As Object, Optional progress As Action(Of String) = Nothing) Implements ILayoutRunner.Apply
            Dim par = DirectCast(p, RadialLayoutParameters)
            params = par

            Call LayoutHelper.EnsurePositions(g, 1000, 1000)
            Call RadialLayout.LayoutNodes(g, par)
        End Sub
    End Class

End Namespace
