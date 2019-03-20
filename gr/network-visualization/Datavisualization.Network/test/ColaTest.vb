Imports Microsoft.VisualBasic.Data.visualize.Network.Layouts
Imports Microsoft.VisualBasic.Data.visualize.Network.Layouts.Cola

Module ColaTest

    Sub Main()

        Dim network As network = network.example


        Pause()
    End Sub


    Public Class network : Implements Cola.network

        Public Property nodes As Node() Implements Cola.network.nodes
        Public Property links As Link(Of Node)() Implements Cola.network.links

        Public Shared Function example() As network
            Dim nodes As New Dictionary(Of String, Node)

            nodes("1") = New Node With {.name = 1}
            nodes("2") = New Node With {.name = 2}
            nodes("3") = New Node With {.name = 3}
            nodes("4") = New Node With {.name = 4}
            nodes("5") = New Node With {.name = 5}

            Dim links As New List(Of Link(Of Node))
            Dim addLink = Sub(a$, b$)
                              Call links.Add(New Link(Of Node) With {.source = nodes(a), .target = nodes(b)})
                          End Sub

            Call addLink(1, 2)
            Call addLink(1, 3)
            Call addLink(2, 4)
            Call addLink(2, 5)
            Call addLink(5, 3)

            Return New network With {
                .nodes = nodes.Values.ToArray,
                .links = links.ToArray
            }
        End Function
    End Class
End Module
