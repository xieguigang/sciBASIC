#Region "Microsoft.VisualBasic::ed522b9afa92eedd806891ab202319f9, gr\network-visualization\Datavisualization.Network\IO\FileStream\csv\Cytoscape.vb"

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

    '     Class Edges
    ' 
    '         Properties: Data, EdgeBetweenness, interaction, name, SUID
    ' 
    '         Function: GetConnectNodes, GetNodes, ToString
    ' 
    '     Class Nodes
    ' 
    '         Properties: AverageShortestPathLength, BetweennessCentrality, ClosenessCentrality, ClusteringCoefficient, data
    '                     Degree, Eccentricity, IsSingleNode, name, NeighborhoodConnectivity
    '                     NumberOfDirectedEdges, NumberOfUndirectedEdges, PartnerOfMultiEdgedNodePairs, Radiality, SelfLoops
    '                     SharedName, Stress, SUID, TopologicalCoefficient
    ' 
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Data.csv.StorageProvider.Reflection
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace FileStream.Cytoscape

    ' Cytoscape exports csv files

    Public Class Edges

        Public Property SUID As String
        Public Property EdgeBetweenness As String
        Public Property interaction As String
        Public Property name As String
        ''' <summary>
        ''' Dynamics extended data
        ''' </summary>
        ''' <returns></returns>
        Public Property Data As Dictionary(Of String, String)

        Public Iterator Function GetNodes(nodeHash As Dictionary(Of Graph.Node)) As IEnumerable(Of Graph.Node)
            With GetConnectNodes()
                Yield nodeHash(.First)
                Yield nodeHash(.Last)
            End With
        End Function

        Public Function GetConnectNodes() As String()
            Dim tokens$() = Strings _
                .Split(name, $"({interaction})") _
                .Select(Function(s) s.Trim) _
                .ToArray
            Return tokens
        End Function

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function
    End Class

    Public Class Nodes

        Public Property SUID As String
        Public Property AverageShortestPathLength As String
        Public Property BetweennessCentrality As String
        Public Property ClosenessCentrality As String
        Public Property ClusteringCoefficient As String
        Public Property Degree As Integer
        Public Property Eccentricity As Integer
        Public Property IsSingleNode As String
        Public Property name As String
        Public Property NeighborhoodConnectivity As String
        Public Property NumberOfDirectedEdges As String
        Public Property NumberOfUndirectedEdges As String
        Public Property PartnerOfMultiEdgedNodePairs As String
        Public Property Radiality As String
        Public Property SelfLoops As String
        <Column("shared name")>
        Public Property SharedName As String
        Public Property Stress As String
        Public Property TopologicalCoefficient As String
        ''' <summary>
        ''' Dynamics extended data
        ''' </summary>
        ''' <returns></returns>
        Public Property data As Dictionary(Of String, String)

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function
    End Class
End Namespace
