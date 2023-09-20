Imports System.Drawing.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Drawing2D

Namespace PCA

    Public Class Ellipse

        Public Property cx As Double
        Public Property cy As Double
        Public Property rx As Double
        Public Property ry As Double

        Public Function BuildPath(Optional k As Double = 0.5522848) As GraphicsPath
            Dim x = cx
            Dim y = cy
            Dim a = rx
            Dim b = ry
            Dim ox = a * k ' 水平控制点偏移量
            Dim oy = b * k ' 垂直控制点偏移量
            Dim ctx As New Path2D

            ctx.MoveTo(x - a, y)
            ctx.CurveTo(x - a, y - oy, x - ox, y - b, x, y - b)
            ctx.CurveTo(x + ox, y - b, x + a, y - oy, x + a, y)
            ctx.CurveTo(x + a, y + oy, x + ox, y + b, x, y + b)
            ctx.CurveTo(x - ox, y + b, x - a, y + oy, x - a, y)
            ctx.CloseAllFigures()

            Return ctx.Path
        End Function

    End Class
End Namespace