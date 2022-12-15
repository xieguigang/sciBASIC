Imports System.Drawing
Imports Microsoft.VisualBasic.Data.GraphTheory

Public Class GridBox(Of T)

    ReadOnly grid As Grid(Of T)
    ReadOnly gridSize As Size

    Sub New(grid As Grid(Of T), boxWidth As Integer, boxHeight As Integer)
        Me.grid = grid
        Me.gridSize = New Size(boxWidth, boxHeight)
    End Sub

    Public Iterator Function Gridding() As IEnumerable(Of T())
        Dim rect As Rectangle = grid.rectangle
        Dim center As Point
        Dim dw As Integer = gridSize.Width / 2
        Dim dh As Integer = gridSize.Height / 2
        Dim block As T()

        ' [x,y] is top left
        For x As Integer = rect.Left To rect.Right Step gridSize.Width
            For y As Integer = rect.Top To rect.Bottom Step gridSize.Height
                center = New Point(x + dw, y + dh)
                block = grid.Query(center.X, center.Y, gridSize).ToArray

                Yield block
            Next
        Next
    End Function

End Class