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
        Dim deque As IDeque(Of Node)
    End Class
End Namespace