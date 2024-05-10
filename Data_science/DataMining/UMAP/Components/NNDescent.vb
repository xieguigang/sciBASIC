#Region "Microsoft.VisualBasic::9ed7b7f692d32c2dc0721df7380401e2, G:/GCModeller/src/runtime/sciBASIC#/Data_science/DataMining/UMAP//Components/NNDescent.vb"

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

    '   Total Lines: 220
    '    Code Lines: 154
    ' Comment Lines: 24
    '   Blank Lines: 42
    '     File Size: 9.05 KB


    ' Interface NNDescentFn
    ' 
    '     Function: NNDescent
    ' 
    ' Class NNDescent
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: MakeNNDescent, NNDescentLoop, NNDescentLoopPar, rpTreeInit
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.DataMining.UMAP.KNN
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Math
Imports std = System.Math

Public Interface NNDescentFn

    Function NNDescent(data As Double()(), leafArray As Integer()(), nNeighbors As Integer,
                       Optional nIters As Integer = 10,
                       Optional maxCandidates As Integer = 50,
                       Optional delta As Double = 0.001F,
                       Optional rho As Double = 0.5F,
                       Optional rpTreeInit As Boolean = True,
                       Optional startingIteration As Action(Of Integer, Integer, String) = Nothing) As KNNState

End Interface

Friend Class NNDescent : Implements NNDescentFn

    ReadOnly distanceFn As DistanceCalculation
    ReadOnly random As IProvideRandomValues

    Sub New(distanceFn As DistanceCalculation, random As IProvideRandomValues)
        Me.distanceFn = distanceFn
        Me.random = random
    End Sub

    Private Function rpTreeInit(leafArray As Integer()(), data As Double()(), currentGraph As Heap, startingIteration As Action(Of Integer, Integer, String)) As Heap
        Dim d As Double
        Dim leafSize As Integer = leafArray.Length
        Dim jj As i32 = 0
        Dim dd As Integer = leafSize / 20

        For n As Integer = 0 To leafSize - 1
            For i As Integer = 0 To leafArray(n).Length - 1
                If leafArray(n)(i) < 0 Then
                    Exit For
                End If

                For j = i + 1 To leafArray(n).Length - 1
                    If leafArray(n)(j) < 0 Then
                        Exit For
                    Else
                        d = distanceFn(data(leafArray(n)(i)), data(leafArray(n)(j)))
                    End If

                    Call Heaps.HeapPush(currentGraph, leafArray(n)(i), d, leafArray(n)(j), 1)
                    Call Heaps.HeapPush(currentGraph, leafArray(n)(j), d, leafArray(n)(i), 1)
                Next
            Next

            If startingIteration IsNot Nothing AndAlso ++jj = dd Then
                jj = 0
                startingIteration.Invoke(n, leafArray.Length, $"rpTreeInit {CInt(n / leafSize * 100)}% [{n}/{leafSize}]")
            End If
        Next

        Return currentGraph
    End Function

    ''' <summary>
    ''' Create a version of nearest neighbor descent.
    ''' </summary>
    Public Function MakeNNDescent(data As Double()(), leafArray As Integer()(), nNeighbors As Integer,
                                  Optional nIters As Integer = 10,
                                  Optional maxCandidates As Integer = 50,
                                  Optional delta As Double = 0.001F,
                                  Optional rho As Double = 0.5F,
                                  Optional rpTreeInit As Boolean = True,
                                  Optional startingIteration As Action(Of Integer, Integer, String) = Nothing) As KNNState Implements NNDescentFn.NNDescent

        Dim nVertices As Integer = data.Length
        Dim currentGraph As Heap = Heaps.MakeHeap(nVertices, nNeighbors)
        Dim d As Double
        Dim jj As i32 = 0
        Dim dd As Integer = nVertices / 10

        Call startingIteration?.Invoke(0, 1, "start sample rejection loop...")

        For i As Integer = 0 To nVertices - 1
            Dim indices As Integer() = Utils.RejectionSample(nNeighbors, data.Length, random)

            For j As Integer = 0 To indices.Length - 1
                d = distanceFn(data(i), data(indices(j)))

                Call Heaps.HeapPush(currentGraph, i, d, indices(j), 1)
                Call Heaps.HeapPush(currentGraph, indices(j), d, i, 1)
            Next

            If startingIteration IsNot Nothing AndAlso ++jj = dd Then
                jj = 0
                startingIteration.Invoke(i, nVertices, $"Heaps.HeapPush {CInt(100 * i / nVertices)}% [{i}/{nVertices}]")
            End If
        Next

        If rpTreeInit Then
            currentGraph = Me.rpTreeInit(leafArray, data, currentGraph, startingIteration)
        End If

        Dim candidateNeighbors As Heap
        Dim c As Integer
        Dim dataSize As Integer = data.Length

        ' 这里是限速步骤
        For n As Integer = 0 To nIters - 1
            startingIteration?.Invoke(n, nIters, $"NNDescentLoop {n}/{nIters}")
            candidateNeighbors = Heaps.BuildCandidates(currentGraph, nVertices, nNeighbors, maxCandidates, random)

            c = NNDescentLoopPar(currentGraph, nVertices, maxCandidates, candidateNeighbors, rho, data)

            If c <= delta * nNeighbors * dataSize Then
                Exit For
            End If
        Next

        Return Heaps.DeHeapSort(currentGraph, startingIteration)
    End Function

    ''' <summary>
    ''' <see cref="NNDescentLoop"/>的并行化版本
    ''' </summary>
    ''' <param name="currentGraph">被修改的数据</param>
    ''' <param name="nVertices">readonly</param>
    ''' <param name="maxCandidates">readonly</param>
    ''' <param name="candidateNeighbors">readonly</param>
    ''' <param name="rho">readonly</param>
    ''' <param name="data">readonly</param>
    ''' <returns></returns>
    Private Function NNDescentLoopPar(currentGraph As Heap,
                                      nVertices As Integer,
                                      maxCandidates As Integer,
                                      candidateNeighbors As Heap,
                                      rho As Double,
                                      data As Double()()) As Double

        Dim f As Func(Of Integer, Double) =
            Function(i)
                Dim c As Double

                For j As Integer = 0 To maxCandidates - 1
                    Dim p = CInt(std.Floor(candidateNeighbors(0)(i)(j)))
                    Dim d As Double

                    If p < 0 OrElse (random.NextFloat() < rho) Then
                        Continue For
                    End If

                    For k As Integer = 0 To maxCandidates - 1
                        Dim q = CInt(std.Floor(candidateNeighbors(0)(i)(k)))
                        Dim cj = candidateNeighbors(2)(i)(j)
                        Dim ck = candidateNeighbors(2)(i)(k)

                        If q < 0 OrElse cj = 0 AndAlso ck = 0 Then
                            Continue For
                        Else
                            d = distanceFn(data(p), data(q))
                        End If

                        c += Heaps.HeapPush(currentGraph, p, d, q, 1)
                        c += Heaps.HeapPush(currentGraph, q, d, p, 1)
                    Next
                Next

                Return c
            End Function
        Dim cc As Double = Enumerable.Range(0, nVertices) _
            .AsParallel _
            .Select(Function(x) f(x)) _
            .Sum

        Return cc
    End Function

    ''' <summary>
    ''' 这个loop在大样本数据集下会非常慢
    ''' </summary>
    ''' <param name="currentGraph">被修改的数据</param>
    ''' <param name="nVertices">readonly</param>
    ''' <param name="maxCandidates">readonly</param>
    ''' <param name="candidateNeighbors">readonly</param>
    ''' <param name="rho">readonly</param>
    ''' <param name="data">readonly</param>
    ''' <returns></returns>
    Private Function NNDescentLoop(currentGraph As Heap,
                                   nVertices As Integer,
                                   maxCandidates As Integer,
                                   candidateNeighbors As Heap,
                                   rho As Double,
                                   data As Double()()) As Double
        Dim d As Double
        Dim c As Double

        Call Console.WriteLine("NNDescentLoop")

        For i As Integer = 0 To nVertices - 1
            For j As Integer = 0 To maxCandidates - 1
                Dim p = CInt(std.Floor(candidateNeighbors(0)(i)(j)))

                If p < 0 OrElse (random.NextFloat() < rho) Then
                    Continue For
                End If

                For k = 0 To maxCandidates - 1
                    Dim q = CInt(std.Floor(candidateNeighbors(0)(i)(k)))
                    Dim cj = candidateNeighbors(2)(i)(j)
                    Dim ck = candidateNeighbors(2)(i)(k)

                    If q < 0 OrElse cj = 0 AndAlso ck = 0 Then
                        Continue For
                    Else
                        d = distanceFn(data(p), data(q))
                    End If

                    c += Heaps.HeapPush(currentGraph, p, d, q, 1)
                    c += Heaps.HeapPush(currentGraph, q, d, p, 1)
                Next
            Next
        Next

        Return c
    End Function
End Class
