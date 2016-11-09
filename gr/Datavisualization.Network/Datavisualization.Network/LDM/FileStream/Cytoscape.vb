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
            Dim tokens As String() =
                Strings.Split(name, $"({interaction})") _
                .ToArray(Function(s) s.Trim)

            Yield nodeHash(tokens.First)
            Yield nodeHash(tokens.Last)
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
        <Column("shared name")> Public Property SharedName As String
        Public Property Stress As String
        Public Property TopologicalCoefficient As String
        ''' <summary>
        ''' Dynamics extended data
        ''' </summary>
        ''' <returns></returns>
        Public Property Data As Dictionary(Of String, String)

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function
    End Class
End Namespace