#Region "Microsoft.VisualBasic::1505384c2412f95a6b401698f9316aae, gr\network-visualization\Network.IO.Extensions\Cytoscape\Cytoscape.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 82
    '    Code Lines: 58 (70.73%)
    ' Comment Lines: 9 (10.98%)
    '    - Xml Docs: 88.89%
    ' 
    '   Blank Lines: 15 (18.29%)
    '     File Size: 2.85 KB


    '     Class Edges
    ' 
    '         Properties: data, EdgeBetweenness, interaction, name, SUID
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
    '         Function: ToString, ToTable
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.SchemaMaps
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace FileStream.Cytoscape

    ' Cytoscape exports csv files

    Public Class Edges

        Public Property SUID As String
        Public Property EdgeBetweenness As Double
        Public Property interaction As String
        Public Property name As String
        ''' <summary>
        ''' Dynamics extended data
        ''' </summary>
        ''' <returns></returns>
        Public Property data As Dictionary(Of String, String)

        Public Iterator Function GetNodes(nodeTable As Dictionary(Of Graph.Node)) As IEnumerable(Of Graph.Node)
            With GetConnectNodes()
                Yield nodeTable(.First)
                Yield nodeTable(.Last)
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
        <Column(Name:="shared name")>
        Public Property SharedName As String
        Public Property Stress As String
        Public Property TopologicalCoefficient As String

        ''' <summary>
        ''' Dynamics extended data
        ''' </summary>
        ''' <returns></returns>
        Public Property data As Dictionary(Of String, String)

        Public Function ToTable() As Dictionary(Of String, String)
            Dim table As New Dictionary(Of String, String)(data)



            Return table
        End Function

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function
    End Class
End Namespace
