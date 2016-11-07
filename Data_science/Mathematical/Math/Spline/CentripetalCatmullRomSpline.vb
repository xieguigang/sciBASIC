Imports System.Drawing
Imports Microsoft.VisualBasic.Mathematical.BasicR

Namespace Interpolation

    ''' <summary>
    ''' ###### Centripetal Catmull–Rom spline
    ''' 
    ''' In computer graphics, centripetal Catmull–Rom spline is a variant form of 
    ''' Catmull-Rom spline formulated by Edwin Catmull and Raphael Rom according 
    ''' to the work of Barry and Goldman. It is a type of interpolating spline 
    ''' (a curve that goes through its control points) defined by four control points
    ''' P0, P1, P2, P3, with the curve drawn only from P1 to P2.
    ''' </summary>
    Public Class CentripetalCatmullRomSpline

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="raw"></param>
        ''' <param name="alpha!">set from 0-1</param>
        ''' <param name="amountOfPoints!">How many points you want on the curve</param>
        ''' <returns>points on the Catmull curve so we can visualize them</returns>
        Public Function CatmulRom(raw As IEnumerable(Of PointF), Optional alpha! = 0.5F, Optional amountOfPoints! = 10.0F) As List(Of PointF)
            Dim points As PointF() = raw.ToArray
            Dim newPoints As New List(Of PointF)

            Dim p0 As New Vector(points(0).X, points(0).Y)
            Dim p1 As New Vector(points(1).X, points(1).Y)
            Dim p2 As New Vector(points(2).X, points(2).Y)
            Dim p3 As New Vector(points(3).X, points(3).Y)

            Dim t0! = 0.0F
            Dim t1! = GetT(t0, alpha, p0, p1)
            Dim t2! = GetT(t1, alpha, p1, p2)
            Dim t3! = GetT(t2, alpha, p2, p3)

            For t! = t1 To t2 Step ((t2 - t1) / amountOfPoints)
                Dim A1 = (t1 - t) / (t1 - t0) * p0 + (t - t0) / (t1 - t0) * p1
                Dim A2 = (t2 - t) / (t2 - t1) * p1 + (t - t1) / (t2 - t1) * p2
                Dim A3 = (t3 - t) / (t3 - t2) * p2 + (t - t2) / (t3 - t2) * p3

                Dim B1 = (t2 - t) / (t2 - t0) * A1 + (t - t0) / (t2 - t0) * A2
                Dim B2 = (t3 - t) / (t3 - t1) * A2 + (t - t1) / (t3 - t1) * A3
                Dim C = (t2 - t) / (t2 - t1) * B1 + (t - t1) / (t2 - t1) * B2

                Call newPoints.Add(C)
            Next

            Return newPoints
        End Function

        Public Function GetT(t!, alpha!, p0 As PointF, p1 As PointF) As Single
            Dim A! = (p1.X - p0.X) ^ 2.0F + (p1.Y - p0.Y) ^ 2.0F
            Dim b! = A ^ 0.5F
            Dim c! = b ^ alpha

            Return c + t
        End Function
    End Class
End Namespace