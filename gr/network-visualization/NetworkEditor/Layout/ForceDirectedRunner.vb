Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Data.visualize.Network.Layouts
Imports Microsoft.VisualBasic.Data.visualize.Network.Layouts.ForceDirected

Namespace NetworkEditor.Layout

    Public Class ForceDirectedRunner : Implements ILayoutRunner

        Private params As New ForceDirectedParameters

        Public ReadOnly Property Name As String Implements ILayoutRunner.Name
            Get
                Return "Force-Directed"
            End Get
        End Property

        Public Function GetParameters() As Object Implements ILayoutRunner.GetParameters
            Return params
        End Function

        Public Sub Apply(g As NetworkGraph, p As Object, Optional progress As Action(Of String) = Nothing) Implements ILayoutRunner.Apply
            Dim par = DirectCast(p, ForceDirectedParameters)
            params = par

            Call LayoutHelper.EnsurePositions(g, par.CanvasWidth, par.CanvasHeight)

            Dim planner As New Planner(g, par)

            For i As Integer = 1 To par.Iterations
                Call planner.Collide()
                If i Mod 50 = 0 AndAlso progress IsNot Nothing Then
                    progress.Invoke($"Force-Directed {i}/{par.Iterations}")
                End If
            Next
        End Sub
    End Class

End Namespace
