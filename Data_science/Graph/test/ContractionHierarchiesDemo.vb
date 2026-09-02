#Region "Microsoft.VisualBasic::07d49c2ad9eb46373487c1b6d47a2467, Data_science\Graph\test\ContractionHierarchiesDemo.vb"

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

    '   Total Lines: 43
    '    Code Lines: 33 (76.74%)
    ' Comment Lines: 1 (2.33%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 9 (20.93%)
    '     File Size: 1.60 KB


    ' Module ContractionHierarchiesDemo
    ' 
    '     Sub: Main
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Data.GraphTheory.Analysis.ContractionHierarchies

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

