Imports Microsoft.VisualBasic.Data.GraphTheory
Imports Microsoft.VisualBasic.Data.GraphTheory.Analysis
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph

Module ullmannTest

    Sub Main()
        Call identicalTest()
        Call Pause()
    End Sub

    Sub identicalTest()
        Dim a As New NetworkGraph From {("A", "B"), ("B", "C"), ("C", "A"), ("A", "E")}
        Dim b As New NetworkGraph From {("A1", "B1"), ("B1", "C1"), ("C1", "A1"), ("A1", "E1")}
        Dim ta As String() = Nothing
        Dim tb As String() = Nothing
        Dim iso As New Ullmann(a.CreateEdgeMatrix(ta), b.CreateEdgeMatrix(tb))

        For Each map In Ullmann.ExplainNodeMapping(iso.FindIsomorphisms, ta, tb)
            Call Console.WriteLine(map)
        Next
    End Sub

End Module
