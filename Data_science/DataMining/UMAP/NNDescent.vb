Imports System

Friend Module NNDescent
    Public Delegate Function NNDescentFn(ByVal data As Single()(), ByVal leafArray As Integer()(), ByVal nNeighbors As Integer, ByVal Optional nIters As Integer = 10, ByVal Optional maxCandidates As Integer = 50, ByVal Optional delta As Single = 0.001F, ByVal Optional rho As Single = 0.5F, ByVal Optional rpTreeInit As Boolean = True, ByVal Optional startingIteration As Action(Of Integer, Integer) = Nothing) As (Integer()(), Single()())

    ''' <summary>
    ''' Create a version of nearest neighbor descent.
    ''' </summary>
    Public Function MakeNNDescent(ByVal distanceFn As DistanceCalculation, ByVal random As IProvideRandomValues) As NNDescent.NNDescentFn
        Return Function(data, leafArray, nNeighbors, nIters, maxCandidates, delta, rho, rpTreeInit, startingIteration)
                   Dim nVertices = data.Length
                   Dim currentGraph = Heaps.MakeHeap(data.Length, nNeighbors)

                   For i = 0 To data.Length - 1
                       Dim indices = Utils.RejectionSample(nNeighbors, data.Length, random)

                       For j = 0 To indices.Length - 1
                           Dim d = distanceFn(data(i), data(indices(j)))
                           Heaps.HeapPush(currentGraph, i, d, indices(j), 1)
                           Heaps.HeapPush(currentGraph, indices(j), d, i, 1)
                       Next
                   Next

                   If rpTreeInit Then
                       For n = 0 To leafArray.Length - 1

                           For i = 0 To leafArray(CInt(n)).Length - 1
                               If leafArray(n)(i) < 0 Then Exit For

                               For j = i + 1 To leafArray(CInt(n)).Length - 1
                                   If leafArray(n)(j) < 0 Then Exit For
                                   Dim d = distanceFn(data(leafArray(n)(i)), data(leafArray(n)(j)))
                                   Heaps.HeapPush(currentGraph, leafArray(n)(i), d, leafArray(n)(j), 1)
                                   Heaps.HeapPush(currentGraph, leafArray(n)(j), d, leafArray(n)(i), 1)
                               Next
                           Next
                       Next
                   End If

                   For n = 0 To nIters - 1
                       startingIteration?.Invoke(n, nIters)
                       Dim candidateNeighbors = Heaps.BuildCandidates(currentGraph, nVertices, nNeighbors, maxCandidates, random)
                       Dim c = 0

                       For i = 0 To nVertices - 1

                           For j = 0 To maxCandidates - 1
                               Dim p = CInt(Math.Floor(candidateNeighbors(0)(i)(j)))
                               If p < 0 OrElse (random.NextFloat() < rho) Then Continue For

                               For k = 0 To maxCandidates - 1
                                   Dim q = CInt(Math.Floor(candidateNeighbors(0)(i)(k)))
                                   Dim cj = candidateNeighbors(2)(i)(j)
                                   Dim ck = candidateNeighbors(2)(i)(k)
                                   If q < 0 OrElse cj = 0 AndAlso ck = 0 Then Continue For
                                   Dim d = distanceFn(data(p), data(q))
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
    End Function
End Module
