Imports System
Imports System.Drawing
Imports System.Linq
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Data.visualize.Network.Layouts
Imports radial = Microsoft.VisualBasic.Data.visualize.Network.Layouts.Radial.RadialLayout
Imports circular = Microsoft.VisualBasic.Data.visualize.Network.Layouts.Circular.CircularLayout

Module Program

    Sub Main()
        Console.WriteLine("=== SpringForce.doForceLayout ===")
        Call SanityCheck(AddressOf RunSpringForce, "SpringForce")

        Console.WriteLine("=== SpringEmbedder.ForceDirectedLayout ===")
        Call SanityCheck(AddressOf RunSpringEmbedder, "SpringEmbedder")

        Console.WriteLine("=== RadialLayout ===")
        Call SanityCheck(AddressOf RunRadial, "Radial")

        Console.WriteLine("=== CircularLayout ===")
        Call SanityCheck(AddressOf RunCircular, "Circular")

        Console.WriteLine("=== CircularLayout + 2-opt 交叉优化 ===")
        Call SanityCheck(AddressOf RunCircularOptimized, "Circular+Opt")

        Console.WriteLine("ALL CHECKS DONE")
    End Sub

    ' 构造一个 40 节点的环形 + 随机附加边的小网络
    Private Function BuildGraph() As NetworkGraph
        Dim g As New NetworkGraph()
        Dim n = 40
        Dim nodes(n - 1) As Node
        Dim rnd As New Random(123)

        For i = 0 To n - 1
            nodes(i) = g.CreateNode("n" & i)
            ' 赋随机初始坐标，避免全零导致布局失效
            nodes(i).data.initialPostion = New FDGVector2(rnd.NextDouble() * 1000, rnd.NextDouble() * 1000)
        Next

        For i = 0 To n - 1
            Call g.CreateEdge(nodes(i), nodes((i + 1) Mod n), 1.0)
        Next

        For i = 0 To 20
            Dim a = rnd.Next(n)
            Dim b = rnd.Next(n)
            If a <> b Then Call g.CreateEdge(nodes(a), nodes(b), 1.0)
        Next

        Return g
    End Function

    Private Delegate Sub LayoutRunner(g As NetworkGraph)

    Private Sub RunSpringForce(g As NetworkGraph)
        Call g.doForceLayout(Stiffness:=80, Repulsion:=4000, Damping:=0.83, iterations:=1000)
    End Sub

    Private Sub RunSpringEmbedder(g As NetworkGraph)
        Call SpringEmbedder.ForceDirectedLayout(g, New Size(1000, 1000), 1000)
    End Sub

    Private Sub RunRadial(g As NetworkGraph)
        Call radial.LayoutNodes(g)
    End Sub

    Private Sub RunCircular(g As NetworkGraph)
        Call circular.LayoutNodes(g, sortByDegree:=True)
    End Sub

    Private Sub RunCircularOptimized(g As NetworkGraph)
        Call circular.LayoutNodesWithCrossingOptimization(g, maxSwaps:=1000)
    End Sub

    Private Sub SanityCheck(runner As LayoutRunner, name As String)
        Dim g = BuildGraph()
        Call runner(g)

        Dim minx = Double.MaxValue, miny = Double.MaxValue, maxx = Double.MinValue, maxy = Double.MinValue
        Dim nanCount = 0
        Dim vs = g.vertex.ToArray()

        For Each v As Node In vs
            Dim p = TryCast(v.data.initialPostion, FDGVector2)

            If p Is Nothing OrElse p.isNaN Then
                nanCount += 1
                Continue For
            End If

            If p.x < minx Then minx = p.x
            If p.y < miny Then miny = p.y
            If p.x > maxx Then maxx = p.x
            If p.y > maxy Then maxy = p.y
        Next

        Dim w = maxx - minx
        Dim h = maxy - miny

        Dim minD = Double.MaxValue

        For i = 0 To vs.Length - 1
            For j = i + 1 To vs.Length - 1
                Dim pi = TryCast(vs(i).data.initialPostion, FDGVector2)
                Dim pj = TryCast(vs(j).data.initialPostion, FDGVector2)

                If pi Is Nothing OrElse pj Is Nothing Then
                    Continue For
                End If

                Dim d = System.Math.Sqrt((pi.x - pj.x) * (pi.x - pj.x) + (pi.y - pj.y) * (pi.y - pj.y))

                If d < minD Then
                    minD = d
                End If
            Next
        Next

        Console.WriteLine($"  bbox=({minx:F1},{miny:F1})-({maxx:F1},{maxy:F1}) size=({w:F1}x{h:F1}) minPairDist={minD:F3} nan={nanCount}")

        If nanCount > 0 Then
            Console.WriteLine("  [FAIL] 存在 NaN 坐标")
        End If
        If w < 50 OrElse h < 50 Then
            Console.WriteLine("  [FAIL] 布局塌缩（包围盒过小）")
        End If
        If minD < 0.5 Then
            Console.WriteLine("  [FAIL] 节点重叠（最小两两距离过小）")
        End If
        If nanCount = 0 AndAlso w >= 50 AndAlso h >= 50 AndAlso minD >= 0.5 Then
            Console.WriteLine("  [PASS] 布局合理：不塌缩、无 NaN、节点不重叠")
        End If
    End Sub
End Module
