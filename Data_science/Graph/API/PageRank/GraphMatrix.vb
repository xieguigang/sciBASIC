#Region "Microsoft.VisualBasic::64776a5194ba9b734c6ee05bf373f4cd, ..\sciBASIC#\Data_science\Graph\API\PageRank\GraphMatrix.vb"

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

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Analysis.PageRank

    ''' <summary>
    ''' 可以用来构建<see cref="PageRank"/>计算所需要的index矩阵
    ''' </summary>
    Public Class GraphMatrix

        Dim indices As New Dictionary(Of String, List(Of Integer))
        Dim nodes As Vertex()
        Dim edges As Edge()

        ''' <summary>
        ''' 
        ''' </summary>
        Sub New(g As Graph)
            Dim index As New Index(Of String)(g.Vertex.Keys)

            nodes = g.Vertex
            edges = g.ToArray

            For Each node As Vertex In nodes
                indices(node.Label) = New List(Of Integer)
            Next

            For Each edge As Edge In edges
                With edge
                    Call indices(.U.Label) _
                        .Add(index(.V.Label))
                End With
            Next

            For Each k In indices.Keys.ToArray
                indices(k) = indices(k).Distinct.AsList
            Next
        End Sub

        ''' <summary>
        ''' 对于文本处理的时候，textrank的这部分数据可能会比较有用
        ''' </summary>
        ''' <returns></returns>
        Public Function GetEdgeCount() As Dictionary(Of String, Integer)
            Dim counts As New Dictionary(Of String, (Edge As Edge, C As int))
            Dim uid$

            For Each edge As Edge In edges
                uid = edge.Key

                If Not counts.ContainsKey(uid) Then
                    Call counts.Add(uid, (edge, 1))
                Else
                    counts(uid).C.Value += 1
                End If
            Next

            ' 统计计数完毕之后再重新赋值
            Return counts.ToDictionary(
                Function(e) e.Key,
                Function(c)
                    Return c.Value.C.Value
                End Function)
        End Function

        Public Function TranslateVector(v#(), Optional reorder As Boolean = False) As Dictionary(Of String, Double)
            If Not reorder Then
                Return nodes _
                    .ToDictionary(Function(n) n.Label,
                                  Function(i) v(i.ID))
            Else
                Dim orders As SeqValue(Of Double)() = v _
                    .SeqIterator _
                    .OrderByDescending(Function(x) x.value) _
                    .ToArray

                Return orders.ToDictionary(
                    Function(i) nodes(i).Label,
                    Function(value) +value)
            End If
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function ToString() As String
            Return indices.GetJson
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Narrowing Operator CType(gm As GraphMatrix) As List(Of Integer)()
            Return gm.nodes.Select(Function(k) gm.indices(k.ID)).ToArray
        End Operator
    End Class
End Namespace
