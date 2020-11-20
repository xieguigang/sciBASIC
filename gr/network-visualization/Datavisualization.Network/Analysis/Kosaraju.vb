#Region "Microsoft.VisualBasic::3a6f785492189b3f2276162dc8d8bda8, gr\network-visualization\Datavisualization.Network\Analysis\Kosaraju.vb"

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

    '     Class Kosaraju
    ' 
    '         Function: GetComponents, StronglyConnectedComponents
    ' 
    '         Sub: [loop], depthFirstSearch, reset
    '         Class NodeCompares
    ' 
    '             Function: Compare
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection.Deque
Imports Microsoft.VisualBasic.Data.visualize.Network.Analysis.Model
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Language

Namespace Analysis

    ''' <summary>
    ''' Kosaraju's algorithm is a linear time algorithm to find 
    ''' the strongly connected components of a directed graph.
    ''' 
    ''' Kosaraju's algorithm works as follows:
    '''
    ''' + Let G be a directed graph And S be an empty stack.
    ''' + While S does Not contain all vertices:
    '''    + Choose an arbitrary vertex ''v'' not in S. 
    '''    + Perform a depth first search starting at ''v''. 
    '''    + Each time that depth-first search finishes expanding a vertex ''u'', push ''u'' onto S.
    ''' + Reverse the directions Of all arcs To obtain the transpose graph.
    ''' + While S Is nonempty:
    '''    + Pop the top vertex ''v'' from S. 
    '''    + Perform a depth-first search starting at ''v'' in the transpose graph. 
    '''    + The set of visited vertices will give the strongly connected component containing ''v''; 
    '''    + record this and remove all these vertices from the graph G and the stack S. 
    '''    + Equivalently, breadth-first search (BFS) can be used instead of depth-first search.
    '''  
    ''' > https://github.com/awadalaa/kosaraju
    ''' </summary>
    Public NotInheritable Class Kosaraju

        Dim t As Integer
        ''' <summary>
        ''' the strong connected components
        ''' </summary>
        Dim scc As New List(Of Integer)()
        Dim pass As Integer = 0
        Dim deque As New Deque(Of Node)
        Dim subnetwork As New List(Of Edge())
        Dim buffer As New List(Of Edge)

        Shared ReadOnly FORWARD_TRAVERSAL As New ForwardTraversal()
        Shared ReadOnly BACKWARD_TRAVERSAL As New BackwardTraversal()

        Private Class NodeCompares : Implements IComparer(Of Node)

            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Public Function Compare(v1 As Node, v2 As Node) As Integer Implements IComparer(Of Node).Compare
                Return v2.data.mass.CompareTo(v1.data.mass)
            End Function
        End Class

        Public Function GetComponents() As IEnumerable(Of Edge())
            Return subnetwork.AsEnumerable
        End Function

        Public Shared Function StronglyConnectedComponents(gr As NetworkGraph) As Kosaraju
            Dim search As New Kosaraju

            Call search.loop(gr, BACKWARD_TRAVERSAL)
            Call search.reset(gr)
            Call search.loop(gr, FORWARD_TRAVERSAL)
            Call search.reset(gr)

            Return search
        End Function

        Private Sub reset(gr As NetworkGraph)
            ' do graph reset
            For Each v In gr.vertex
                v.visited = False
            Next
        End Sub

        Private Sub [loop](gr As NetworkGraph, tp As EdgeTraversalPolicy)
            Dim vs As ICollection(Of Node)

            If pass = 0 Then
                ' 这里是按照id降序
                vs = gr.vertex _
                    .OrderByDescending(Function(a) a.ID) _
                    .ToArray
            Else
                ' 这里是按照结果值升序
                vs = New SortedSet(Of Node)(gr.vertex, New NodeCompares)
            End If

            For Each v As Node In vs
                If Not v.visited Then
                    v.visited = True
                    deque.AddHead(v)

                    While Not deque.Empty
                        v = deque.Peek()
                        Call depthFirstSearch(tp, v)
                    End While

                    If pass = 1 Then
                        scc.Add(t)
                        t = 0
                        subnetwork.Add(buffer.PopAll)
                    End If
                End If
            Next

            pass += 1
        End Sub

        Private Sub depthFirstSearch(tp As EdgeTraversalPolicy, v As Node)
            For Each edge As Edge In tp.edges(v.directedVertex)
                Dim [next] As Node = tp.vertex(edge)

                buffer.Add(edge)

                If Not [next].visited Then
                    [next].visited = True
                    deque.AddHead([next])
                    Return
                End If
            Next

            t += 1

            If pass = 0 Then
                v.data.mass = t
            End If

            deque.RemoveHead()
        End Sub
    End Class
End Namespace
