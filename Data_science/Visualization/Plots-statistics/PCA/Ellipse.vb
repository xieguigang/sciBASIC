#Region "Microsoft.VisualBasic::782c80c02f80a9c7abf7bcef8fcfba92, Data_science\Visualization\Plots-statistics\PCA\Ellipse.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 90
    '    Code Lines: 65
    ' Comment Lines: 14
    '   Blank Lines: 11
    '     File Size: 3.26 KB


    '     Class Ellipse
    ' 
    '         Properties: cx, cy, orient, rx, ry
    '                     theta
    ' 
    '         Function: BuildPath, ConfidenceEllipse, cov
    ' 
    ' 
    ' /********************************************************************************/

#End Region

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
