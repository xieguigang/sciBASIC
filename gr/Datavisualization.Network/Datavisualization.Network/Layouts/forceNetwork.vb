Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph

Namespace Layouts

    Public Module forceNetwork

        ''' <summary>
        ''' Applies the spring embedder.
        ''' </summary>
        ''' <param name="Network"></param>
        ''' <param name="iterations"></param>
        <ExportAPI("Layout.ForceDirected")>
        <Extension>
        Public Sub doLayout(Network As NetworkGraph, iterations As Integer, size As Size)

        End Sub
    End Module
End Namespace