#Region "Microsoft.VisualBasic::178d03dd7ec1ccdc6c96f65ca2c4a791, Data_science\Visualization\Plots-statistics\PCA\Ellipse.vb"

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

    '   Total Lines: 105
    '    Code Lines: 83 (79.05%)
    ' Comment Lines: 10 (9.52%)
    '    - Xml Docs: 90.00%
    ' 
    '   Blank Lines: 12 (11.43%)
    '     File Size: 4.23 KB


    '     Class Ellipse
    ' 
    '         Properties: cx, cy, rx, ry, theta
    ' 
    '         Function: BuildPath, ConfidenceEllipse, Covariance
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging.Math2D
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Math.LinearAlgebra.Matrix
Imports Microsoft.VisualBasic.Math.Statistics
Imports std = System.Math

#If NET48 Then
Imports Pen = System.Drawing.Pen
Imports Pens = System.Drawing.Pens
Imports Brush = System.Drawing.Brush
Imports Font = System.Drawing.Font
Imports Brushes = System.Drawing.Brushes
Imports SolidBrush = System.Drawing.SolidBrush
Imports DashStyle = System.Drawing.Drawing2D.DashStyle
Imports Image = System.Drawing.Image
Imports Bitmap = System.Drawing.Bitmap
Imports GraphicsPath = System.Drawing.Drawing2D.GraphicsPath
Imports TextureBrush = System.Drawing.TextureBrush
#Else
Imports Pen = Microsoft.VisualBasic.Imaging.Pen
Imports Pens = Microsoft.VisualBasic.Imaging.Pens
Imports Brush = Microsoft.VisualBasic.Imaging.Brush
Imports Font = Microsoft.VisualBasic.Imaging.Font
Imports Brushes = Microsoft.VisualBasic.Imaging.Brushes
Imports SolidBrush = Microsoft.VisualBasic.Imaging.SolidBrush
Imports DashStyle = Microsoft.VisualBasic.Imaging.DashStyle
Imports Image = Microsoft.VisualBasic.Imaging.Image
Imports Bitmap = Microsoft.VisualBasic.Imaging.Bitmap
Imports GraphicsPath = Microsoft.VisualBasic.Imaging.GraphicsPath
Imports TextureBrush = Microsoft.VisualBasic.Imaging.TextureBrush
#End If

Namespace PCA

    Public Class Ellipse

        Public Property cx As Double
        Public Property cy As Double
        Public Property rx As Double
        Public Property ry As Double
        ''' <summary>
        ''' angle
        ''' </summary>
        ''' <returns></returns>
        Public Property theta As Double

        Public Function BuildPath(Optional k As Double = 0.1) As GraphicsPath
            Dim ellipse As New GraphicsPath
            Dim a = cx
            Dim b = cy
            Dim meanX = rx
            Dim meanY = ry
            Dim points As New List(Of PointF)

            For t As Double = 0 To 2 * std.PI Step k
                Dim x = a * std.Cos(t) * std.Cos(theta) - b * std.Sin(t) * std.Sin(theta) + meanX
                Dim y = a * std.Cos(t) * std.Sin(theta) + b * std.Sin(t) * std.Cos(theta) + meanY

                Call points.Add(New PointF(x, y))
            Next

            Call ellipse.AddPolygon(points.ToArray)
            Call ellipse.CloseAllFigures()

            Return ellipse
        End Function

        Private Shared Function Covariance(x As Double(), meanX As Double, y As Double(), meanY As Double) As Double
            Dim diff As Vector = (New Vector(x) - meanX) * (New Vector(y) - meanY)
            Dim cov As Double = diff.Sum / (x.Length - 1)
            Return cov
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="data"></param>
        ''' <param name="level"></param>
        ''' <returns></returns>
        Public Shared Function ConfidenceEllipse(data As Polygon2D, Optional level As ChiSquareTest.ConfidenceLevels = ChiSquareTest.ConfidenceLevels.C95) As Ellipse
            Dim pc1 = data.xpoints
            Dim pc2 = data.ypoints
            Dim meanX As Double = pc1.Average
            Dim meanY As Double = pc2.Average
            Dim covXX = Covariance(pc1, meanX, pc1, meanX)
            Dim covYY = Covariance(pc2, meanY, pc2, meanY)
            Dim covXY = Covariance(pc1, meanX, pc2, meanY)
            Dim lambda1 = 0.5 * (covXX + covYY + std.Sqrt((covXX - covYY) * (covXX - covYY) + 4 * covXY * covXY))
            Dim lambda2 = 0.5 * (covXX + covYY - std.Sqrt((covXX - covYY) * (covXX - covYY) + 4 * covXY * covXY))
            Dim chiSquareValue As Double = ChiSquareTest.ChiSquareValue(level)
            Dim a = std.Sqrt(chiSquareValue * lambda1)
            Dim b = std.Sqrt(chiSquareValue * lambda2)
            Dim theta = 0.5 * std.Atan(2 * covXY / (covXX - covYY))

            Return New Ellipse With {
                .cx = a,
                .cy = b,
                .theta = theta,
                .rx = meanX,
                .ry = meanY
            }
        End Function
    End Class
End Namespace
