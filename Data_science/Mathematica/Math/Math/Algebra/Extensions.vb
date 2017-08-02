Imports System.Drawing

Namespace LinearAlgebra

    Public Module HelperExtensions

        ''' <summary>
        ''' 返回一元一次方程
        ''' </summary>
        ''' <param name="p1"></param>
        ''' <param name="p2"></param>
        ''' <returns>``y=ax+b``</returns>
        Public Function PrimitiveLinearEquation(p1 As PointF, p2 As PointF) As Func(Of Double, Double)
            Dim x1 = p1.X
            Dim x2 = p2.X
            Dim y1 = p1.Y
            Dim y2 = p2.Y
            Dim a# = (y2 - y1) / (x2 - x1)
            Dim b# = y1 - a * x1

            Return Function(x)
                       Return a * x + b
                   End Function
        End Function
    End Module
End Namespace