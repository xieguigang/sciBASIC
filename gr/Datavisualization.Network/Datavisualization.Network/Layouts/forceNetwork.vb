Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph

Namespace Layouts

    Public Module forceNetwork

        ''' <summary>
        ''' Applies the spring embedder.
        ''' </summary>
        ''' <param name="net"></param>
        ''' <param name="iterations"></param>
        <ExportAPI("Layout.ForceDirected")>
        <Extension>
        Public Sub doLayout(ByRef net As NetworkGraph, Optional iterations% = 1000)
            Dim physicsEngine As New ForceDirected2D(net, 0, 0, 0)
            For i As Integer = 0 To iterations
                Call physicsEngine.Calculate(0.05F)
            Next
        End Sub
    End Module
End Namespace