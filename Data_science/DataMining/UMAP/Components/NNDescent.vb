#Region "Microsoft.VisualBasic::05815521caa5ffab2fd03e88b5840cfd, Data_science\DataMining\UMAP\Components\NNDescent.vb"

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

' Interface NNDescentFn
' 
'     Function: NNDescent
' 
' Class NNDescent
' 
'     Constructor: (+1 Overloads) Sub New
'     Function: MakeNNDescent
' 
' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Math
Imports stdNum = System.Math

Public Interface NNDescentFn

    Function NNDescent(data As Double()(), leafArray As Integer()(), nNeighbors As Integer,
                       Optional nIters As Integer = 10,
                       Optional maxCandidates As Integer = 50,
                       Optional delta As Double = 0.001F,
                       Optional rho As Double = 0.5F,
                       Optional rpTreeInit As Boolean = True,
                       Optional startingIteration As Action(Of Integer, Integer) = Nothing) As (Integer()(), Double()())

End Interface

Friend Class NNDescent : Implements NNDescentFn

    ReadOnly distanceFn As DistanceCalculation
    ReadOnly random As IProvideRandomValues

    Sub New(distanceFn As DistanceCalculation, random As IProvideRandomValues)
        Me.distanceFn = distanceFn
        Me.random = random
    End Sub

    ''' <summary>
    ''' Create a version of nearest neighbor descent.
    ''' </summary>
    Public Function MakeNNDescent(data As Double()(), leafArray As Integer()(), nNeighbors As Integer,
                                  Optional nIters As Integer = 10,
                                  Optional maxCandidates As Integer = 50,
                                  Optional delta As Double = 0.001F,
                                  Optional rho As Double = 0.5F,
                                  Optional rpTreeInit As Boolean = True,
                                  Optional startingIteration As Action(Of Integer, Integer) = Nothing) As (Integer()(), Double()()) Implements NNDescentFn.NNDescent

        Dim nVertices = data.Length
        Dim currentGraph = Heaps.MakeHeap(data.Length, nNeighbors)
        Dim d As Double

        For i = 0 To data.Length - 1
            Dim indices As Integer() = Utils.RejectionSample(nNeighbors, data.Length, random)

            For j = 0 To indices.Length - 1
                d = distanceFn(data(i), data(indices(j)))

                Call Heaps.HeapPush(currentGraph, i, d, indices(j), 1)
                Call Heaps.HeapPush(currentGraph, indices(j), d, i, 1)
            Next
        Next

        If rpTreeInit Then
            For n = 0 To leafArray.Length - 1

                For i = 0 To leafArray(CInt(n)).Length - 1
                    If leafArray(n)(i) < 0 Then
                        Exit For
                    End If

                    For j = i + 1 To leafArray(CInt(n)).Length - 1
                        If leafArray(n)(j) < 0 Then
                            Exit For
                        Else
                            d = distanceFn(data(leafArray(n)(i)), data(leafArray(n)(j)))
                        End If

                        Call Heaps.HeapPush(currentGraph, leafArray(n)(i), d, leafArray(n)(j), 1)
                        Call Heaps.HeapPush(currentGraph, leafArray(n)(j), d, leafArray(n)(i), 1)
                    Next
                Next
            Next
        End If

        Dim candidateNeighbors As Heap
        Dim c As Integer

        For n As Integer = 0 To nIters - 1
            Call startingIteration?.Invoke(n, nIters)

            candidateNeighbors = Heaps.BuildCandidates(currentGraph, nVertices, nNeighbors, maxCandidates, random)
            c = 0

            For i As Integer = 0 To nVertices - 1
                For j As Integer = 0 To maxCandidates - 1
                    Dim p = CInt(stdNum.Floor(candidateNeighbors(0)(i)(j)))

                    If p < 0 OrElse (random.NextFloat() < rho) Then
                        Continue For
                    End If

                    For k = 0 To maxCandidates - 1
                        Dim q = CInt(stdNum.Floor(candidateNeighbors(0)(i)(k)))
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

            If c <= delta * nNeighbors * data.Length Then
                Exit For
            End If
        Next

        Return Heaps.DeHeapSort(currentGraph)
    End Function
End Class
