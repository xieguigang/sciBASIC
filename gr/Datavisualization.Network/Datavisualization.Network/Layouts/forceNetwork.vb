Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph

Namespace Layouts

    Public Module forceNetwork

        ''' <summary>
        ''' Applies the force directed layout.
        ''' </summary>
        ''' <param name="net"></param>
        ''' <param name="iterations"></param>
        <ExportAPI("Layout.ForceDirected")>
        <Extension>
        Public Sub doForceLayout(ByRef net As NetworkGraph, Optional Stiffness# = 80, Optional Repulsion# = 4000, Optional Damping# = 0.83, Optional iterations% = 1000)
            Dim physicsEngine As New ForceDirected2D(net, Stiffness, Repulsion, Damping)

            For i As Integer = 0 To iterations
                Call physicsEngine.Calculate(0.05F)
            Next

            Call physicsEngine.EachNode(
                Sub(node, point)
                    node.Data.initialPostion = point.position
                End Sub)
        End Sub
    End Module
End Namespace