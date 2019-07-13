#Region "Microsoft.VisualBasic::1386ca12999760d88a8a7926b2eb684e, Data_science\Graph\Model\Abstract\General.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    ' Class VertexEdge
    ' 
    '     Function: EdgeKey
    ' 
    ' Class Graph
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports TV = Microsoft.VisualBasic.Data.GraphTheory.Vertex
Imports V = Microsoft.VisualBasic.Data.GraphTheory.Vertex

Public Class VertexEdge : Inherits Edge(Of V)

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Shared Function EdgeKey(U%, V%) As String
        Return $"{U}-{V}"
    End Function
End Class

''' <summary>
''' A graph ``G = (V, E)`` consists of a set V of vertices and a set E edges, that is, unordered
''' pairs Of vertices. Unless explicitly stated otherwise, we assume that the graph Is simple,
''' that Is, it has no multiple edges And no self-loops.
''' </summary>
Public Class Graph : Inherits Graph(Of TV, VertexEdge, Graph)

End Class
