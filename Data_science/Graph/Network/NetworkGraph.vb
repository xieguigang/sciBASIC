Namespace Network

    Public Class NetworkGraph(Of Node As {New, Network.Node}, Edge As {New, Network.Edge(Of Node)}) : Inherits Graph(Of Node, Edge, NetworkGraph(Of Node, Edge))

        Sub New()
        End Sub

        ''' <summary>
        ''' Network model copy
        ''' </summary>
        ''' <param name="nodes"></param>
        ''' <param name="edges"></param>
        Sub New(nodes As IEnumerable(Of Node), edges As IEnumerable(Of Edge))

        End Sub
    End Class
End Namespace

