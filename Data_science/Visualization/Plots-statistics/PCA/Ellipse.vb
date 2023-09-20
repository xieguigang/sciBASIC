Imports System.Drawing.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Math2D
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.Correlations
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
            Dim xData = data.xpoints
            Dim xDataDev = xData.StandardDeviation
            Dim xMean = xData.Average
            Dim yData = data.ypoints
            Dim yDataDev = yData.StandardDeviation
            Dim yMean = yData.Average
            Dim cor As Double = Correlations.GetPearson(xData, yData)
            Dim cov As Double = cor * xDataDev * yDataDev
            Dim covmat = {
                {xDataDev ^ 2, cov}, {cov, yDataDev ^ 2}
            }
            Dim eig = New NumericMatrix(covmat).Eigen
            Dim scale = Distribution.ChiSquareInverse(level, 2)
            ' return { lambda:R.getDiag(), E:E };
            Dim eigLambdaX As Double() = eig.RealEigenvalues
            Dim maxLambdaI As Integer = which.Max(eigLambdaX)
            Dim minLambdaI As Integer = which.Min(eigLambdaX)
            Dim rx As Double = If(xDataDev > yDataDev,
                std.Sqrt(eigLambdaX(maxLambdaI)) * scale,
                std.Sqrt(eigLambdaX(minLambdaI)) * scale)
            Dim ry As Double = If(yDataDev > xDataDev,
                std.Sqrt(eigLambdaX(maxLambdaI)) * scale,
                std.Sqrt(eigLambdaX(minLambdaI)) * scale)
            Dim v1 As Double() = eig.V.X(maxLambdaI)
            Dim theta = std.Atan2(v1(1), v1(0))

            If theta < 0 Then
                theta += 2 * std.PI
            End If

            Return New Ellipse With {
                .rx = rx, .ry = ry,
                .cx = xMean, .cy = yMean,
                .orient = -(theta * 180 / std.PI),
                .theta = theta
            }
        End Function
    End Class
End Namespace