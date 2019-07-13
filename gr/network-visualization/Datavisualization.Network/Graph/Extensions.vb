#Region "Microsoft.VisualBasic::3c47a3eb8410c791a4fb5c0be1e82290, gr\network-visualization\Datavisualization.Network\Graph\Extensions.vb"

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

    '     Module Extensions
    ' 
    '         Function: GetNeighbours, NodesID, RemoveDuplicated, RemoveSelfLoop
    ' 
    '         Sub: ApplyAnalysis
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.visualize.Network.FileStream
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph.Abstract
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq

Namespace Graph

    Public Module Extensions

        ''' <summary>
        ''' Get all of the connected nodes ID from the edges data
        ''' </summary>
        ''' <param name="network"></param>
        ''' <returns></returns>
        <Extension>
        Public Function NodesID(network As IEnumerable(Of IInteraction)) As String()
            Return network _
                .Select(Function(link) {link.source, link.target}) _
                .IteratesALL _
                .Distinct _
                .Where(Function(id) Not id.StringEmpty) _
                .ToArray
        End Function

        <Extension>
        Public Sub ApplyAnalysis(ByRef net As NetworkGraph)
            For Each node In net.vertex
                node.data.neighbours = net.GetNeighbours(node.Label).ToArray
            Next
        End Sub

        <Extension>
        Public Iterator Function GetNeighbours(net As NetworkGraph, node As String) As IEnumerable(Of Integer)
            For Each edge As Edge In net.graphEdges
                Dim connected As String = edge.GetConnectedNode(node)
                If Not String.IsNullOrEmpty(connected) Then
                    Yield CInt(connected)
                End If
            Next
        End Function

        ''' <summary>
        ''' 移除的重复的边
        ''' </summary>
        ''' <remarks></remarks>
        ''' <param name="directed">是否忽略方向？</param>
        ''' <param name="ignoreTypes">是否忽略边的类型？</param>
        <Extension> Public Function RemoveDuplicated(Of T As NetworkEdge)(
                                                    edges As IEnumerable(Of T),
                                                    Optional directed As Boolean = True,
                                                    Optional ignoreTypes As Boolean = False) As T()
            Dim uid = Function(edge As T) As String
                          If directed Then
                              Return edge.GetDirectedGuid(ignoreTypes)
                          Else
                              Return edge.GetNullDirectedGuid(ignoreTypes)
                          End If
                      End Function
            Dim LQuery = edges _
                .GroupBy(uid) _
                .Select(Function(g) g.First) _
                .ToArray

            Return LQuery
        End Function

        ''' <summary>
        ''' 移除自身与自身的边
        ''' </summary>
        ''' <remarks></remarks>
        <Extension> Public Function RemoveSelfLoop(Of T As NetworkEdge)(edges As IEnumerable(Of T)) As T()
            Dim LQuery = LinqAPI.Exec(Of T) <=
 _
                From x As T
                In edges
                Where Not x.SelfLoop
                Select x

            Return LQuery
        End Function
    End Module
End Namespace
