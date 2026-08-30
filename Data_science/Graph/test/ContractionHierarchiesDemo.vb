Imports Microsoft.VisualBasic.Data.GraphTheory.DistPreprocessSmall

Module ContractionHierarchiesDemo

    ' ========== 主程序 ==========
    Public Sub Main(args As String())
        Dim input As String() = Console.ReadLine().Split(" "c)
        Dim n As Integer = Integer.Parse(input(0))
        Dim m As Integer = Integer.Parse(input(1))

        Dim graph As Vertex() = New Vertex(n - 1) {}
        For i As Integer = 0 To n - 1
            graph(i) = New Vertex(i)
        Next

        For i As Integer = 0 To m - 1
            Dim parts As String() = Console.ReadLine().Split(" "c)
            Dim x As Integer = Integer.Parse(parts(0)) - 1
            Dim y As Integer = Integer.Parse(parts(1)) - 1
            Dim c As Long = Long.Parse(parts(2))

            graph(x).outEdges.Add(y)
            graph(x).outECost.Add(c)
            graph(y).inEdges.Add(x)
            graph(y).inECost.Add(c)
        Next

        Dim process As PreProcess = New PreProcess()
        Dim nodeOrdering As Integer() = process.processing(graph)

        Console.WriteLine("Ready")

        Dim bd As BidirectionalDijkstra = New BidirectionalDijkstra()
        Dim t As Integer = Integer.Parse(Console.ReadLine())

        For i As Integer = 0 To t - 1
            Dim parts As String() = Console.ReadLine().Split(" "c)
            Dim u As Integer = Integer.Parse(parts(0)) - 1
            Dim v As Integer = Integer.Parse(parts(1)) - 1
            Console.WriteLine(bd.computeDist(graph, u, v, i, nodeOrdering))
        Next
    End Sub
End Module
