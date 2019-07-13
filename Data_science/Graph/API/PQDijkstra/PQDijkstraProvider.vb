#Region "Microsoft.VisualBasic::6954a42314c3147cc56cf03beb920f4d, Data_science\Graph\API\PQDijkstra\PQDijkstraProvider.vb"

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

    '     Class PQDijkstraProvider
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Compute
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Dijkstra.PQDijkstra

    Public MustInherit Class PQDijkstraProvider

        Dim dijkstra As DijkstraFast

        ''' <summary>
        ''' get costs. If there is no connection, then cost is maximum.(»ñÈ¡)
        ''' </summary>
        ''' <param name="start"></param>
        ''' <param name="finish"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Protected MustOverride Function getInternodeTraversalCost(start As Integer, finish As Integer) As Single
        ''' <summary>
        ''' »ñÈ¡ÓëÄ¿±ê½ÚµãÖ±½ÓÏàÁÚµÄËùÓÐµÄ½ÚµãµÄ±àºÅ
        ''' </summary>
        ''' <param name="startingNode"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Protected MustOverride Function GetNearbyNodes(startingNode As Integer) As IEnumerable(Of Integer)

        ''' <summary>
        ''' ÍøÂçÖ®ÖÐµÄ½ÚµãµÄ×ÜÊýÄ¿
        ''' </summary>
        ''' <param name="totalNodes"></param>
        ''' <remarks></remarks>
        Sub New(totalNodes As Integer)
            dijkstra = New DijkstraFast(totalNodes, New DijkstraFast.InternodeTraversalCost(AddressOf getInternodeTraversalCost), New DijkstraFast.NearbyNodesHint(AddressOf GetNearbyNodes))
        End Sub

        Public Function Compute(start As Integer, ends As Integer) As Integer()
            Dim minPath As Integer() = dijkstra.GetMinimumPath(start, ends)
            Return minPath
        End Function
    End Class
End Namespace
