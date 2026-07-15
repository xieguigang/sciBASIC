Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Data.visualize.Network.Layouts
Imports Microsoft.VisualBasic.Data.visualize.Network.Layouts.SpringForce

Namespace NetworkEditor.Layout

    Public Class SpringForceRunner : Implements ILayoutRunner

        Private params As ForceDirectedArgs = ForceDirectedArgs.DefaultNew()

        Public ReadOnly Property Name As String Implements ILayoutRunner.Name
            Get
                Return "Spring Force"
            End Get
        End Property

        Public Function GetParameters() As Object Implements ILayoutRunner.GetParameters
            Return params
        End Function

        Public Sub Apply(g As NetworkGraph, p As Object, Optional progress As Action(Of String) = Nothing) Implements ILayoutRunner.Apply
            Dim par = DirectCast(p, ForceDirectedArgs)
            params = par

            If g.CheckZero() Then
                Call g.doRandomLayout()
            End If

            Call g.doForceLayout(par)
        End Sub
    End Class

End Namespace
