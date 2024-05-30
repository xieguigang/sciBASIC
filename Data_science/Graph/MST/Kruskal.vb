#Region "Microsoft.VisualBasic::c9f0a238f5b0db9ae13dec5f0fac5332, Data_science\Graph\MST\Kruskal.vb"

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

    '   Total Lines: 59
    '    Code Lines: 43 (72.88%)
    ' Comment Lines: 3 (5.08%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 13 (22.03%)
    '     File Size: 1.87 KB


    '     Class Kruskal
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: calculateNumNodes, findMinTree, NodeIdTuple
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Scripting.Expressions

Namespace MinimumSpanningTree

    ''' <summary>
    ''' Minimum spanning tree via Kruskal algorithm
    ''' </summary>
    Public Class Kruskal

        Private djset As New DisjointSet()
        Private edge As New List(Of VertexEdge)
        Private numNodes As Integer

        Public Sub New(edges As IEnumerable(Of VertexEdge))
            edge = edges.AsList
            edge.Sort(Function(e1, e2)
                          If e1.weight > e2.weight Then Return 1
                          If e1.weight < e2.weight Then Return -1
                          Return 0
                      End Function)

            numNodes = calculateNumNodes(edge)
        End Sub

        Public Function calculateNumNodes(edges As IEnumerable(Of VertexEdge)) As Integer
            Return (From i In edges.AsEnumerable Select NodeIdTuple(i)).IteratesALL.Distinct.Count
        End Function

        Private Iterator Function NodeIdTuple(i As VertexEdge) As IEnumerable(Of Integer)
            Yield i.U.ID
            Yield i.V.ID
        End Function

        Public Iterator Function findMinTree() As IEnumerable(Of VertexEdge)
            Dim i = 1

            While i <= numNodes
                djset.makeset(i)
                i += 1
            End While

            Dim count = 0
            Dim idx = 0

            While count < numNodes - 1
                If djset.findset(edge(idx).V.ID) <> djset.findset(edge(idx).U.ID) Then
                    Yield edge(idx)

                    count += 1
                    djset.union(edge(idx).V.ID, edge(idx).U.ID)
                End If

                idx += 1
            End While
        End Function
    End Class
End Namespace
