Imports Microsoft.VisualBasic.Data.visualize.Network
Imports Microsoft.VisualBasic.Data.visualize.Network.Analysis
Imports Microsoft.VisualBasic.Data.visualize.Network.FileStream
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Serialization.JSON

Module LouvainTest

    Sub Main()

        Dim links = "E:\GCModeller\src\runtime\sciBASIC#\Data_science\algorithms\Louvain\facebook_combined.txt".ReadAllLines.Skip(1).Select(Function(str) Strings.Trim(str).StringSplit("\s+")).ToArray
        Dim g As New NetworkGraph

        For Each line In links
            If g.GetElementByID(line(0)) Is Nothing Then
                g.CreateNode(line(0))
            End If
            If g.GetElementByID(line(1)) Is Nothing Then
                g.CreateNode(line(1))
            End If

            g.CreateEdge(g.GetElementByID(line(0)), g.GetElementByID(line(1)), 1)
        Next

        Dim clusters = Communities.Analysis(g)

        Call Console.WriteLine(Communities.Modularity(clusters))
        Call Console.WriteLine(Communities.Community(g).GetJson(indent:=True))

        Call clusters.Tabular.Save("E:\GCModeller\src\runtime\sciBASIC#\Data_science\algorithms\Louvain\facebook_combined_graph")

        Pause()
    End Sub

End Module
