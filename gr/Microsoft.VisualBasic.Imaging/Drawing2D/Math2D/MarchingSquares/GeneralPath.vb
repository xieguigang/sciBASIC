Imports System.Drawing
Imports Microsoft.VisualBasic.Language

Namespace Drawing2D.Math2D.MarchingSquares

    Public Class GeneralPath

        Dim polygons As New List(Of PointF())
        Dim temp As New List(Of PointF)

        Friend Sub moveTo(x As Double, y As Double)
            temp.Add(New PointF(x, y))
        End Sub

        Friend Sub lineTo(x As Double, y As Double)
            temp.Add(New PointF(x, y))
        End Sub

        Friend Sub closePath()
            polygons.Add(temp.PopAll)
        End Sub
    End Class
End Namespace