#Region "Microsoft.VisualBasic::de0c8b3fec529b2a1645c10cd9d8e45f, ..\sciBASIC#\gr\Datavisualization.Network\Datavisualization.Network\IO\FileStream\csv\Cytoscape.vb"

' Author:
' 
'       asuka (amethyst.asuka@gcmodeller.org)
'       xieguigang (xie.guigang@live.com)
'       xie (genetics@smrucc.org)
' 
' Copyright (c) 2016 GPL3 Licensed
' 
' 
' GNU GENERAL PUBLIC LICENSE (GPL3)
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
                .Select(Function(s) s.Trim)
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
