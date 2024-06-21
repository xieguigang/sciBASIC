Imports Microsoft.VisualBasic.Parallel

''' <summary>
''' 
''' </summary>
Friend Class RPTree : Inherits VectorTask

    Public data As Double()()
    Public currentGraph As Heap
    Public wrap As NNDescent

    ReadOnly leafArray As Integer()()

    Public Sub New(leafArray As Integer()())
        MyBase.New(leafArray.Length)
        Me.leafArray = leafArray
    End Sub

    Protected Overrides Sub Solve(start As Integer, ends As Integer, cpu_id As Integer)
        For n As Integer = start To ends
            ' nth leaf in projection tree
            Call rpTreeInit(n)
        Next
    End Sub

    Private Sub rpTreeInit(n As Integer)
        Dim d As Double

        For i As Integer = 0 To leafArray(n).Length - 1
            If leafArray(n)(i) < 0 Then
                Exit For
            End If

            For j = i + 1 To leafArray(n).Length - 1
                If leafArray(n)(j) < 0 Then
                    Exit For
                Else
                    d = wrap.distanceFn(data(leafArray(n)(i)), data(leafArray(n)(j)))
                End If

                Call Heaps.HeapPush(currentGraph, leafArray(n)(i), d, leafArray(n)(j), 1)
                Call Heaps.HeapPush(currentGraph, leafArray(n)(j), d, leafArray(n)(i), 1)
            Next
        Next
    End Sub
End Class
