#Region "Microsoft.VisualBasic::635463b36f93a129c080a6f851f3c9c3, Data_science\Visualization\Visualization\Kmeans\KMeansNetwork.vb"

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

    '     Module KMeansNetwork
    ' 
    '         Function: ToNetwork
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.visualize.Network.FileStream
Imports Microsoft.VisualBasic.Data.visualize.Network
Imports Microsoft.VisualBasic.DataMining.KMeans
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Language

Namespace KMeans

    Public Module KMeansNetwork

        ''' <summary>
        ''' 相似度矩阵之中大于0的值会被作为边的强度，小于或者等于0表示没有边
        ''' </summary>
        ''' <param name="kmeans"></param>
        ''' <returns></returns>
        <Extension> Public Function ToNetwork(kmeans As IEnumerable(Of EntityClusterModel),
                                              Optional colors As Dictionary(Of String, Color) = Nothing,
                                              Optional defaultColor$ = "lightgray",
                                              Optional cut As Func(Of Double, Boolean) = Nothing) As NetworkTables

            Dim data As EntityClusterModel() = kmeans.ToArray
            Dim edges As New List(Of NetworkEdge)
            Dim clusterColors As New Dictionary(Of NamedValue(Of String))
            Dim [default] As New NamedValue(Of String) With {
                .Name = NameOf([default]),
                .Value = defaultColor
            }

            If Not colors Is Nothing Then
                For Each c As KeyValuePair(Of String, Color) In colors
                    clusterColors += New NamedValue(Of String) With {
                        .Name = c.Key,
                        .Value = c.Value.ToHtmlColor
                    }
                Next
            End If

            Dim getColor = Function(cluster$)
                               Return clusterColors.TryGetValue(cluster, [default]).Value
                           End Function
            Dim nodes As Dictionary(Of Node) = data _
                .Select(Function(n) New Node With {
                    .ID = n.ID,
                    .NodeType = n.Cluster,
                    .Properties = New Dictionary(Of String, String) From {
                        {
                            "color", getColor(cluster:= .NodeType)
                        }
                    }
                }).ToDictionary

            If cut Is Nothing Then
                cut = Function(score) score > Double.MinValue
            End If

            For Each vector As EntityClusterModel In data
                Dim from$ = vector.ID
                Dim color$

                For Each hit In vector.Properties.Where(Function(h) cut(h.Value))
                    Dim clusters$() = {
                        nodes(from$), nodes(hit.Key)
                    }.Select(Function(n) n.NodeType) _
                    .Distinct _
                    .ToArray

                    If clusters.Length = 1 Then
                        color = getColor(cluster:=clusters(Scan0))
                    Else
                        color = clusters _
                            .Select(getColor) _
                            .Select(AddressOf TranslateColor) _
                            .Average _
                            .ToHtmlColor
                    End If

                    edges += New NetworkEdge With {
                        .value = hit.Value,
                        .FromNode = from,
                        .ToNode = hit.Key,
                        .Interaction = clusters.JoinBy("|"),
                        .Properties = New Dictionary(Of String, String) From {
                            {"color", color}
                        }
                    }
                Next
            Next

            Return New NetworkTables With {
                .edges = edges _
                    .RemoveSelfLoop _
                    .RemoveDuplicated(False, False), ' 相似度是无方向的
                .nodes = nodes
            }
        End Function
    End Module
End Namespace
