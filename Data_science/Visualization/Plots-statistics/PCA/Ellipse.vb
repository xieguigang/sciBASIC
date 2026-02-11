#Region "Microsoft.VisualBasic::41275b729b68bd3e1c7ef9fcc3535d3b, Data_science\Visualization\Plots-statistics\PCA\Ellipse.vb"

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

    '   Total Lines: 158
    '    Code Lines: 89 (56.33%)
    ' Comment Lines: 45 (28.48%)
    '    - Xml Docs: 71.11%
    ' 
    '   Blank Lines: 24 (15.19%)
    '     File Size: 6.32 KB


    '     Class Ellipse
    ' 
    '         Properties: CenterX, CenterY, RotationAngle, SemiMajorAxis, SemiMinorAxis
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

    ''' <summary>
    ''' PCA 统计椭圆类，用于表示基于协方差的置信椭圆
    ''' </summary>
    Public Class Ellipse

        ''' <summary>
        ''' 椭圆的中心点 X 坐标
        ''' </summary>
        Public Property CenterX As Double

        ''' <summary>
        ''' 椭圆的中心点 Y 坐标
        ''' </summary>
        Public Property CenterY As Double

        ''' <summary>
        ''' 长半轴长度 (对应较大的特征值)
        ''' </summary>
        Public Property SemiMajorAxis As Double

        ''' <summary>
        ''' 短半轴长度 (对应较小的特征值)
        ''' </summary>
        Public Property SemiMinorAxis As Double

        ''' <summary>
        ''' 旋转角度（弧度），表示长轴相对于 X 轴的旋转角
        ''' </summary>
        Public Property RotationAngle As Double

        ''' <summary>
        ''' 构建椭圆的绘图路径
        ''' </summary>
        ''' <param name="k">步长精度，默认0.1</param>
        ''' <returns></returns>
        Public Function BuildPath(Optional k As Double = 0.1) As GraphicsPath
            Dim path As New GraphicsPath
            Dim points As New List(Of PointF)

            ' 使用语义清晰的属性名称
            Dim a As Double = Me.SemiMajorAxis
            Dim b As Double = Me.SemiMinorAxis
            Dim theta As Double = Me.RotationAngle
            Dim meanX As Double = Me.CenterX
            Dim meanY As Double = Me.CenterY

            For t As Double = 0 To 2 * std.PI Step k
                ' 参数方程：先在未旋转坐标系下计算，再应用旋转矩阵，最后平移
                Dim x_local As Double = a * std.Cos(t)
                Dim y_local As Double = b * std.Sin(t)

                ' 旋转矩阵变换
                Dim x_rot As Double = x_local * std.Cos(theta) - y_local * std.Sin(theta)
                Dim y_rot As Double = x_local * std.Sin(theta) + y_local * std.Cos(theta)

                ' 平移到中心点
                Call points.Add(New PointF(x_rot + meanX, y_rot + meanY))
            Next

            Call path.AddPolygon(points.ToArray)
            Call path.CloseAllFigures()

            Return path
        End Function

        ''' <summary>
        ''' 计算协方差
        ''' </summary>
        Private Shared Function Covariance(x As Double(), meanX As Double, y As Double(), meanY As Double) As Double
            Dim diff As Vector = (New Vector(x) - meanX) * (New Vector(y) - meanY)
            Dim cov As Double = diff.Sum / (x.Length - 1)
            Return cov
        End Function

        ''' <summary>
        ''' 计算置信椭圆（静态工厂方法）
        ''' </summary>
        ''' <param name="data">二维点数据（Polygon2D）</param>
        ''' <param name="level">置信水平</param>
        ''' <returns></returns>
        Public Shared Function ConfidenceEllipse(data As Polygon2D, Optional level As ChiSquareTest.ConfidenceLevels = ChiSquareTest.ConfidenceLevels.C95) As Ellipse
            Dim pc1 = data.xpoints
            Dim pc2 = data.ypoints

            ' 1. 计算均值
            Dim meanX As Double = pc1.Average
            Dim meanY As Double = pc2.Average

            ' 2. 计算协方差
            Dim covXX = Covariance(pc1, meanX, pc1, meanX)
            Dim covYY = Covariance(pc2, meanY, pc2, meanY)
            Dim covXY = Covariance(pc1, meanX, pc2, meanY)

            ' 3. 计算特征值 (2x2 对称矩阵的特征值解析解)
            ' trace = covXX + covYY
            ' determinant = covXX * covYY - covXY^2
            Dim trace As Double = covXX + covYY
            Dim det As Double = covXX * covYY - covXY * covXY
            Dim delta As Double = std.Sqrt(trace * trace - 4 * det)

            Dim lambda1 As Double = 0.5 * (trace + delta) ' 较大特征值 (主轴方向)
            Dim lambda2 As Double = 0.5 * (trace - delta) ' 较小特征值

            ' 4. 计算缩放因子 (基于卡方分布)
            Dim chiSquareValue As Double = ChiSquareTest.ChiSquareValue(level)

            ' 5. 计算半轴长度
            Dim semiMajor As Double = std.Sqrt(chiSquareValue * lambda1)
            Dim semiMinor As Double = std.Sqrt(chiSquareValue * lambda2)

            ' 6. 计算旋转角度 (使用 Atan2 修正象限问题)
            ' 对应最大特征值 lambda1 的特征向量方向
            Dim theta As Double = 0.5 * std.Atan2(2 * covXY, covXX - covYY)

            Return New Ellipse With {
                .CenterX = meanX,
                .CenterY = meanY,
                .SemiMajorAxis = semiMajor,
                .SemiMinorAxis = semiMinor,
                .RotationAngle = theta
            }
        End Function
    End Class
End Namespace
