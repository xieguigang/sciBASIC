Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.DataVisualization.Network.Graph

Namespace Layouts

    Public Module LayoutUpdater

        <Extension>
        Public Sub Updates(Of T As AbstractVector)(engine As ForceDirected(Of T), ByRef net As NetworkGraph)
            For Each node As Node In net.nodes
                Dim p As LayoutPoint = engine.GetPoint(node)
            Next
        End Sub
    End Module
End Namespace