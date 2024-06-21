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
