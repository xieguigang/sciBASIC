Imports Microsoft.VisualBasic.Imaging.SVG.XML

Namespace SVG

    Public Module ModelBuilder

        Public Function PiePath(x!, y!, width!, height!, startAngle!, sweepAngle!) As path
            Dim endAngle! = startAngle + sweepAngle
            Dim rX = width / 2
            Dim rY = height / 2
            Dim x1 = x + rX * Math.Cos(Math.PI * startAngle / 180)
            Dim y1 = y + rY * Math.Sin(Math.PI * startAngle / 180)
            Dim x2 = x + rX * Math.Cos(Math.PI * endAngle / 180)
            Dim y2 = y + rY * Math.Sin(Math.PI * endAngle / 180)
            Dim d = $"M{x},{y}  L{x1},{y1}  A{rX},{rY} 0 0,1 {x2},{y2} z" ' 1 means clockwise

            Return New path With {
                .d = d
            }
        End Function
    End Module
End Namespace