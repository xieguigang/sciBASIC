Imports System.Drawing
Imports Microsoft.VisualBasic.Data.GraphTheory

Public Class CellEntity(Of T As Individual) : Inherits GridCell(Of T)

    Protected adjacents As CellEntity(Of T)()

    Sub New(i As Integer, j As Integer, obj As T)
        data = obj
        index = New Point(i, j)
    End Sub

    Friend Sub config(grid As Simulator(Of T))
        Me.adjacents = New CellEntity(Of T)(4 - 1) {}

        adjacents(0) = grid(index.X, index.Y - 1)
        adjacents(1) = grid(index.X, index.Y + 1)
        adjacents(2) = grid(index.X - 1, index.Y)
        adjacents(3) = grid(index.X + 1, index.Y)
    End Sub

    Private Iterator Function getAdjacents() As IEnumerable(Of CellEntity(Of Individual))
        For Each cell As CellEntity(Of T) In adjacents
            Yield DirectCast(cell, Object)
        Next
    End Function

    Sub Tick()
        Call data.Tick(getAdjacents)
    End Sub
End Class

Public Interface Individual

    Sub Tick(adjacents As IEnumerable(Of CellEntity(Of Individual)))

End Interface