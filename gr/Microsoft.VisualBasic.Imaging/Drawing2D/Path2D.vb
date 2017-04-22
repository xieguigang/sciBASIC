Imports System.Drawing
Imports System.Drawing.Drawing2D

Namespace Drawing2D

    Public Class Path2D

        Public ReadOnly Property Path As New GraphicsPath

        Dim last As PointF

        Public Sub MoveTo(x!, y!)
            last = New PointF(x, y)
        End Sub

        Public Sub LineTo(x!, y!)
            Dim p2 As New PointF(x, y)
            Call Path.AddLine(last, p2)
            last = p2
        End Sub

        Public Sub Rewind()
            Call Path.Reset()
        End Sub

        Public Sub CloseAllFigures()
            Call Path.CloseAllFigures()
        End Sub
    End Class
End Namespace