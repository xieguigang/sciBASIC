Imports System.Drawing
Imports Microsoft.VisualBasic.Data.GraphTheory

Public Class CellEntity(Of T As Individual) : Inherits GridCell(Of T)

    Protected adjacents As CellEntity(Of T)()

    Sub New(i As Integer, j As Integer)
        index = New Point(i, j)
    End Sub

    Friend Sub config(grid As Simulator(Of T))
        Me.adjacents = New CellEntity(Of T)(4 - 1) {}

        adjacents(0) = grid(index.X, index.Y - 1)
        adjacents(1) = grid(index.X, index.Y + 1)
        adjacents(2) = grid(index.X - 1, index.Y)
        adjacents(3) = grid(index.X + 1, index.Y)
    End Sub

    Sub Tick()
        Call data.Tick(adjacents)
    End Sub
End Class

Public Interface Individual

    Sub Tick(Of T As Individual)(adjacents As CellEntity(Of T)())

End Interface