#Region "Microsoft.VisualBasic::cce6a6697567fd049b80a6472b63c453, gr\network-visualization\Network.IO.Extensions\IO\TabularCreator.vb"

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

    '   Total Lines: 224
    '    Code Lines: 154 (68.75%)
    ' Comment Lines: 40 (17.86%)
    '    - Xml Docs: 87.50%
    ' 
    '   Blank Lines: 30 (13.39%)
    '     File Size: 9.53 KB


    '     Module TabularCreator
    ' 
    '         Function: (+2 Overloads) CreateGraphTable, (+2 Overloads) CreateNodesMetaData, dumpNodeVertex, GetUnionProperties, (+3 Overloads) Tabular
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports names = Microsoft.VisualBasic.Data.visualize.Network.FileStream.Generic.NamesOf

Namespace FileStream

    Public Module TabularCreator

        ''' <summary>
        ''' 将<see cref="NetworkGraph"/>保存到csv文件之中
        ''' </summary>
        ''' <param name="g"></param>
        ''' <param name="propertyNames">
        ''' The data property names of nodes and edges.
        ''' </param>
        ''' <returns></returns>
        <Extension>
        Public Function Tabular(g As NetworkGraph,
                                Optional propertyNames$() = Nothing,
                                Optional is2D As Boolean = True,
                                Optional creators As String() = Nothing,
                                Optional title$ = Nothing,
                                Optional description$ = Nothing,
                                Optional keywords$() = Nothing,
                                Optional links$() = Nothing,
                                Optional meta As Dictionary(Of String, String) = Nothing) As NetworkTables

            Dim data As New MetaData With {
                .create_time = Now,
                .creators = creators,
                .description = description,
                .keywords = keywords,
                .links = links,
                .title = title,
                .additionals = meta
            }

            Return g.Tabular(propertyNames, is2D, data)
        End Function

        ''' <summary>
        ''' 将<see cref="NetworkGraph"/>保存到csv文件之中
        ''' </summary>
        ''' <param name="g"></param>
        ''' <returns></returns>
        <Extension>
        Public Function Tabular(g As NetworkGraph) As NetworkTables
            Return g.Tabular(properties:={"*"}, meta:=New MetaData)
        End Function

        ''' <summary>
        ''' 将<see cref="NetworkGraph"/>保存到csv文件之中
        ''' </summary>
        ''' <param name="g"></param>
        ''' <param name="properties">
        ''' The data property names of nodes and edges.
        ''' </param>
        ''' <returns></returns>
        <Extension>
        Public Function Tabular(g As NetworkGraph,
                                Optional properties$() = Nothing,
                                Optional is2D As Boolean = True,
                                Optional meta As MetaData = Nothing) As NetworkTables

            Dim nodes As Node() = g.CreateNodesMetaData(properties, is2D).ToArray
            Dim edges As NetworkEdge() = g.CreateGraphTable(properties, is2D).ToArray

            Return New NetworkTables With {
                .edges = edges,
                .nodes = nodes,
                .meta = If(meta, New MetaData)
            }
        End Function

        ''' <summary>
        ''' create table of the network graph edges
        ''' </summary>
        ''' <param name="g"></param>
        ''' <param name="properties$"></param>
        ''' <param name="is2Dlayout"></param>
        ''' <returns></returns>
        <Extension>
        Public Iterator Function CreateGraphTable(g As Edge(), properties$(), is2Dlayout As Boolean) As IEnumerable(Of NetworkEdge)
            Dim edgeProperties As String() = properties
            Dim edge As NetworkEdge

            If (Not edgeProperties.IsNullOrEmpty) AndAlso edgeProperties.Length = 1 AndAlso edgeProperties(Scan0) = "*" Then
                edgeProperties = (From e As Graph.Edge In g Select e.data).GetUnionProperties
            End If

            For Each l As Edge In g
                edge = New NetworkEdge With {
                    .fromNode = l.U.label,
                    .toNode = l.V.label,
                    .interaction = l.data(names.REFLECTION_ID_MAPPING_INTERACTION_TYPE),
                    .value = l.weight,
                    .Properties = New Dictionary(Of String, String) From {
                        {NameOf(EdgeData.label), l.data.label},
                        {names.REFLECTION_ID_MAPPING_EDGE_GUID, l.ID},
                        {"color", If(l.data.style Is Nothing, "black", l.data.style.Color.ToHtmlColor)},
                        {"width", If(l.data.style Is Nothing, 1, l.data.style.Width)}
                    }
                }

                With edge
                    If Not edgeProperties.IsNullOrEmpty Then
                        For Each key As String In edgeProperties.Where(Function(p) l.data.HasProperty(p))
                            .ItemValue(key) = l.data(key)
                        Next
                    End If

                    .Properties.Remove(names.REFLECTION_ID_MAPPING_INTERACTION_TYPE)
                End With

                Yield edge
            Next
        End Function

        ''' <summary>
        ''' create table of the network graph edges
        ''' </summary>
        ''' <param name="g"></param>
        ''' <param name="properties$"></param>
        ''' <param name="is2Dlayout"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function CreateGraphTable(g As NetworkGraph, properties$(), is2Dlayout As Boolean) As IEnumerable(Of NetworkEdge)
            Return g.graphEdges.ToArray.CreateGraphTable(properties, is2Dlayout)
        End Function

        <Extension>
        Private Function GetUnionProperties(Of T As GraphData)(vlist As IEnumerable(Of T)) As String()
            Return vlist.Select(Function(v) v.Properties.Keys) _
                .IteratesALL _
                .Distinct _
                .ToArray
        End Function

        <Extension>
        Public Iterator Function CreateNodesMetaData(g As Graph.Node(), properties$(), is2Dlayout As Boolean) As IEnumerable(Of Node)
            If Not properties.IsNullOrEmpty AndAlso properties.Length = 1 AndAlso properties(Scan0) = "*" Then
                properties = (From v In g Select v.data).GetUnionProperties
                properties = properties _
                    .Where(Function(name) name <> names.REFLECTION_ID_MAPPING_NODETYPE) _
                    .ToArray
            End If

            For Each n As Graph.Node In g
                If n.data Is Nothing Then
                    n.data = New NodeData
                End If

                Yield dumpNodeVertex(n, properties, is2Dlayout)
            Next
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function CreateNodesMetaData(g As NetworkGraph, properties$(), is2Dlayout As Boolean) As IEnumerable(Of Node)
            Return g.vertex.ToArray.CreateNodesMetaData(properties, is2Dlayout)
        End Function

        Private Function dumpNodeVertex(n As Graph.Node, properties As String(), is2Dlayout As Boolean) As Node
            Dim data As New Dictionary(Of String, String) From {
                {"weight", n.data.mass}
            }

            If Not n.data.initialPostion Is Nothing Then
                ' skip coordination information when no layout data.
                data("x") = n.data.initialPostion.x
                data("y") = n.data.initialPostion.y

                If Not is2Dlayout Then
                    data("z") = n.data.initialPostion.z
                End If
            End If

            If Not n.data.color Is Nothing AndAlso n.data.color.GetType Is GetType(SolidBrush) Then
                data(names.REFLECTION_ID_MAPPING_NODECOLOR) = DirectCast(n.data.color, SolidBrush).Color.ToHtmlColor
            End If

            If Not properties Is Nothing Then
                For Each key As String In properties.Where(Function(p) n.data.HasProperty(p))
                    data(key) = n.data(key)
                Next
            End If

            For Each key As String In {
                names.REFLECTION_ID_MAPPING_DEGREE,
                names.REFLECTION_ID_MAPPING_DEGREE_IN,
                names.REFLECTION_ID_MAPPING_DEGREE_OUT,
                names.REFLECTION_ID_MAPPING_RELATIVE_DEGREE_CENTRALITY,
                names.REFLECTION_ID_MAPPING_RELATIVE_OUTDEGREE_CENTRALITY,
                names.REFLECTION_ID_MAPPING_BETWEENESS_CENTRALITY,
                names.REFLECTION_ID_MAPPING_RELATIVE_BETWEENESS_CENTRALITY
            }.Where(Function(p) n.data.HasProperty(p))

                data(key) = n.data(key)
            Next

            ' 20191022
            ' name 会和cytoscape之中的name属性产生冲突
            ' 所以在这里修改为label
            If Not data.ContainsKey("label") Then
                Call data.Add("label", n.data.label)
            End If
            If Not data.ContainsKey(NameOf(NodeData.origID)) Then
                Call data.Add(NameOf(NodeData.origID), Strings.Trim(n.data.origID).Replace(","c, "."c).Replace(""""c, "'"c))
            End If

            Return New Node With {
                .ID = n.label,
                .NodeType = n.data(names.REFLECTION_ID_MAPPING_NODETYPE),
                .Properties = data
            }
        End Function
    End Module
End Namespace
