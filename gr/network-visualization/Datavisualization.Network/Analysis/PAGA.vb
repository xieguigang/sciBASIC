
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.visualize.Network.FileStream.Generic
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph

Namespace Analysis

    ''' <summary>
    ''' PAGA - partition-based graph abstraction
    ''' </summary>
    Public Module PAGA

        ''' <summary>
        ''' Mapping out the coarse-grained connectivity structures of complex manifolds (Genome Biology, 2019).
        ''' </summary>
        ''' <param name="manifolds">
        ''' the manifolds graph should contains the cluster information inside the node metadata.
        ''' </param>
        ''' <returns>
        ''' an abstract graph of the input manifolds result
        ''' </returns>
        <Extension>
        Public Function Abstraction(manifolds As NetworkGraph) As NetworkGraph
            ' split the nodes by node type
            Dim clusters = manifolds.vertex.GroupBy(Function(v) v(NamesOf.REFLECTION_ID_MAPPING_NODETYPE)).ToArray
            Dim abstract As New NetworkGraph

            For Each cluster As IGrouping(Of String, Node) In clusters

            Next

            Return abstract
        End Function

    End Module
End Namespace