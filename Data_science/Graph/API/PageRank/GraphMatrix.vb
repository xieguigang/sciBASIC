#Region "Microsoft.VisualBasic::b1b8a5b926f38bf4f24342ac16bdd706, ..\sciBASIC#\gr\Datavisualization.Network\Datavisualization.Network\Analysis\PageRank\GraphMatrix.vb"

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
Imports Microsoft.VisualBasic.Data.visualize.Network.FileStream
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Analysis.PageRank

    ''' <summary>
    ''' 可以用来构建<see cref="PageRank"/>计算所需要的index矩阵
    ''' </summary>
    Public Class GraphMatrix

        Dim indices As New Dictionary(Of String, List(Of Integer))
        Dim nodes As FileStream.Node()
        Dim edges As NetworkEdge()

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="net"></param>
        ''' <param name="skipCount">
        ''' 对于文本处理的时候，textrank的这部分数据可能会比较有用，这个时候这里可以设置为False.
        ''' </param>
        Sub New(net As FileStream.NetworkTables, Optional skipCount As Boolean = True)
            nodes = net.Nodes
            edges = net.Edges

            Dim index As New Index(Of String)(nodes.Select(Function(x) x.ID))

            For Each node As FileStream.Node In nodes
                Call indices.Add(
                    node.ID,
                    New List(Of Integer))
            Next

            For Each edge As NetworkEdge In edges
                Call indices(edge.FromNode) _
                    .Add(index(edge.ToNode))
            Next

            If Not skipCount Then

                ' 对于文本处理的时候，textrank的这部分数据可能会比较有用
                Dim counts As New Dictionary(Of String, (Edge As NetworkEdge, C As int))
                Dim uid$

                For Each edge As NetworkEdge In edges
                    uid = edge.GetDirectedGuid

                    If Not counts.ContainsKey(uid) Then
                        Call counts.Add(uid, (edge, 1))
                    Else
                        counts(uid).C.value += 1
                    End If
                Next

                ' 统计计数完毕之后再重新赋值
                For Each edge As NetworkEdge In edges
                    uid = edge.GetDirectedGuid
                    edge.Properties.Add("c", counts(uid).C)
                Next
            End If

            For Each k In indices.Keys.ToArray
                indices(k) = indices(k).Distinct.AsList
            Next
        End Sub

        Sub New(g As NetworkGraph)
            Call Me.New(g.Tabular)
        End Sub

        ''' <summary>
        ''' Save network
        ''' </summary>
        ''' <param name="DIR$"></param>
        Public Sub Save(DIR$)
            Call GetNetwork.Save(DIR)
        End Sub

        Public Function GetNetwork() As FileStream.NetworkTables
            Return New FileStream.NetworkTables With {
                .Nodes = nodes,
                .Edges = edges
            }
        End Function

        Public Function TranslateVector(v#(), Optional reorder As Boolean = False) As Dictionary(Of String, Double)
            If Not reorder Then
                Return nodes _
                    .SeqIterator _
                    .ToDictionary(Function(n) (+n).ID,
                                  Function(i) v(i))
            Else
                Dim orders As SeqValue(Of Double)() = v _
                    .SeqIterator _
                    .OrderByDescending(Function(x) x.value) _
                    .ToArray

                Return orders.ToDictionary(
                    Function(i) nodes(i).ID,
                    Function(value) +value)
            End If
        End Function

        Public Overrides Function ToString() As String
            Return indices.GetJson
        End Function

        Public Shared Narrowing Operator CType(gm As GraphMatrix) As List(Of Integer)()
            Return gm.nodes.ToArray(Function(k) gm.indices(k.ID))
        End Operator
    End Class
End Namespace
