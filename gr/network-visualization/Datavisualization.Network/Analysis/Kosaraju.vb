Imports Microsoft.VisualBasic.ComponentModel.Collection.Deque
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph

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
    Public Class Kosaraju

        Dim t As Integer
        ''' <summary>
        ''' the strong connected components
        ''' </summary>
        Dim scc As System.Collections.Generic.List(Of Integer?) = New System.Collections.Generic.List(Of Integer?)()
        Dim pass As Integer = 0
        Dim deque As Deque(Of Node)

        Sub dfsLoop(ByVal gr As NetworkGraph, ByVal tp As EdgeTraversalPolicy)
            t = 0
            deque = New Deque(Of Node)()

            Dim vs As ICollection(Of Node)

            If pass = 0 Then
                vs = gr.verticesInReversedOrder.Values
            Else
                vs = New System.Collections.Generic.SortedSet(Of com.technalaa.kosaraju.DirectedVertex)(New com.technalaa.kosaraju.KosarajuSCC.ComparatorAnonymousInnerClass())
                vs.addAll(gr.vertices.Values)
            End If

            For Each v As com.technalaa.kosaraju.DirectedVertex In vs
                If Not v.visited Then
                    v.visited = True
                    deque.push(v)

                    While Not deque.Empty
                        v = deque.peek()
                        dfs(tp, v)
                    End While

                    If pass = 1 Then
                        scc.Add(t)
                        t = 0
                    End If
                End If
            Next

            pass += 1
        End Sub

        Private Sub dfs(ByVal tp As EdgeTraversalPolicy, ByVal v As Node)
            For Each edge As Edge In tp.edges(v)
                Dim [next] As Node = tp.vertex(edge)

                If Not [next].visited Then
                    [next].visited = True
                    deque.push([next])
                    Return
                End If
            Next

            t += 1

            If pass = 0 Then
                v.f = t
            End If

            deque.pop()
        End Sub
    End Class
End Namespace