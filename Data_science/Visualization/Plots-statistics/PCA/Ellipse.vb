Imports System.Drawing.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Math2D
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.Correlations
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Math.LinearAlgebra.Matrix
Imports Microsoft.VisualBasic.Math.Statistics.Distributions
Imports Microsoft.VisualBasic.Math.Statistics.Linq
Imports std = System.Math

Namespace PCA

    Public Class Ellipse

        Public Property cx As Double
        Public Property cy As Double
        Public Property rx As Double
        Public Property ry As Double
        Public Property orient As Double
        ''' <summary>
        ''' angle
        ''' </summary>
        ''' <returns></returns>
        Public Property theta As Double

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

        Private Shared Function cov(x As Double(), y As Double()) As Double(,)
            Dim xDataDev = x.StandardDeviation
            Dim yDataDev = y.StandardDeviation
            Dim cor = Correlations.GetPearson(x, y)
            Dim covx = cor * xDataDev * yDataDev

            Return {
                {xDataDev ^ 2, covx}, {covx, yDataDev ^ 2}
            }
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="data"></param>
        ''' <param name="level"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' https://github.com/DoAutumn/confidence-ellipse/blob/main/src/helper.ts
        ''' </remarks>
        Public Shared Function ConfidenceEllipse(data As Polygon2D, Optional level As Double = 0.95) As Ellipse
            Dim cov = Ellipse.cov(data.xpoints, data.ypoints)
            Dim eig = New NumericMatrix(cov).Eigen
            Dim scale = Distribution.ChiSquareInverse(level, 2)
            ' return { lambda:R.getDiag(), E:E };
            Dim lambda As Double() = Vector.Sqrt(eig.RealEigenvalues)
            Dim rx As Double = lambda(0) * scale
            Dim ry As Double = lambda(1) * scale
            Dim v = eig.V
            Dim theta = std.Acos(v(0, 0))

            If theta < 0 Then
                theta += 2 * std.PI
            End If

            Return New Ellipse With {
                .rx = rx, .ry = ry,
                .cx = data.xpoints.Average,
                .cy = data.ypoints.Average,
                .orient = -(theta * 180 / std.PI),
                .theta = theta
            }
        End Function
    End Class
End Namespace