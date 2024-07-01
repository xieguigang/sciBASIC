#Region "Microsoft.VisualBasic::d736006bc3b955b9dcb5e98d25c831e8, Data_science\DataMining\UMAP\NNDescent\NNDescentLoop.vb"

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

    '   Total Lines: 67
    '    Code Lines: 49 (73.13%)
    ' Comment Lines: 3 (4.48%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 15 (22.39%)
    '     File Size: 1.89 KB


    ' Class NNDescentLoop
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    '     Function: [Loop]
    ' 
    '     Sub: Reset, Solve
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Parallel
Imports std = System.Math

''' <summary>
''' implements NNDescent parallel loop
''' </summary>
Friend Class NNDescentLoop : Inherits VectorTask

    Public currentGraph As Heap
    Public wrap As NNDescent
    Public rho As Double
    Public data As Double()()
    Public maxCandidates As Integer
    Public candidateNeighbors As Heap
    Public ReadOnly c As Double()

    Public Sub New(nVertices As Integer)
        MyBase.New(nVertices)
        c = Allocate(Of Double)(all:=False)
    End Sub

    Public Sub Reset()
        For i As Integer = 0 To c.Length - 1
            c(i) = 0
        Next
    End Sub

    Protected Overrides Sub Solve(start As Integer, ends As Integer, cpu_id As Integer)
        Dim c As Double = 0

        For i As Integer = start To ends
            c += [Loop](i)
        Next

        Me.c(cpu_id) = c
    End Sub

    Private Function [Loop](i As Integer) As Double
        Dim c, d As Double

        For j As Integer = 0 To maxCandidates - 1
            Dim p = CInt(std.Floor(candidateNeighbors(0)(i)(j)))

            If p < 0 OrElse (wrap.random.NextFloat() < rho) Then
                Continue For
            End If

            For k As Integer = 0 To maxCandidates - 1
                Dim q = CInt(std.Floor(candidateNeighbors(0)(i)(k)))
                Dim cj = candidateNeighbors(2)(i)(j)
                Dim ck = candidateNeighbors(2)(i)(k)

                If q < 0 OrElse cj = 0 AndAlso ck = 0 Then
                    Continue For
                Else
                    d = wrap.distanceFn(data(p), data(q))
                End If

                c += Heaps.HeapPush(currentGraph, p, d, q, 1)
                c += Heaps.HeapPush(currentGraph, q, d, p, 1)
            Next
        Next

        Return c
    End Function

End Class

