#Region "Microsoft.VisualBasic::e92b6f10d4a83c57b0e1f9d2a6c48b31, Data_science\Visualization\Visualization\Multivariate\ScoreEllipseHelper.vb"

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

    ' Class MultivariateEllipseDrawer
    ' 
    '     Properties: ConfidenceLevel, EllipseType, FillAlpha, LineWidth, Mode
    '                 StandardDeviations
    ' 
    '     Function: IsDrawable
    ' 
    '     Sub: Draw
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.Data.Plots
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports std = System.Math

''' <summary>
''' 得分图上的分组椭圆绘制器。
''' 
''' <para>
''' 支持两套算法：
''' </para>
''' 
''' <list type="bullet">
''' <item>
''' <see cref="MultivariateEllipseMode.Confidence"/>：调用 <see cref="PCAEllipseCalculator"/>，
''' 基于协方差矩阵的特征分解 + 卡方/t 分布分位数，得到真正的统计置信椭圆（默认 95%）。
''' </item>
''' <item>
''' <see cref="MultivariateEllipseMode.StandardDeviation"/>：调用 <c>Geometry.EstimateEllipse</c>，
''' 得到 k 倍标准差椭圆（k=2 约等于 95%），对离群点更稳健。
''' </item>
''' </list>
''' 
''' <para>
''' 重要：<b>椭圆必须在像素坐标系下计算</b>。如果先在数据坐标系里算出半轴长度再绘制，
''' 由于 x/y 轴的单位长度通常不同（例如 PC1 的跨度是 PC2 的数倍），椭圆会被拉伸变形。
''' </para>
''' </summary>
Public Class MultivariateEllipseDrawer

    ''' <summary>椭圆计算/绘制模式</summary>
    Public Property Mode As MultivariateEllipseMode = MultivariateEllipseMode.Confidence

    ''' <summary>置信水平（0-1），仅在 <see cref="MultivariateEllipseMode.Confidence"/> 模式下生效</summary>
    Public Property ConfidenceLevel As Single = 0.95F

    ''' <summary>统计分布假设，仅在 <see cref="MultivariateEllipseMode.Confidence"/> 模式下生效</summary>
    Public Property EllipseType As PCAEllipseCalculator.EllipseType = PCAEllipseCalculator.EllipseType.Normal

    ''' <summary>标准差倍数，仅在 <see cref="MultivariateEllipseMode.StandardDeviation"/> 模式下生效</summary>
    Public Property StandardDeviations As Double = 2.0

    ''' <summary>填充色的透明度（0-255）</summary>
    Public Property FillAlpha As Integer = 42

    ''' <summary>描边线宽</summary>
    Public Property LineWidth As Single = 1.2F

    ''' <summary>描边是否用虚线</summary>
    Public Property Dashed As Boolean = False

    ''' <summary>
    ''' 给定的一组点是否足够支撑椭圆绘制（至少 3 个点，且模式不是 <see cref="MultivariateEllipseMode.None"/>）。
    ''' </summary>
    Public Function IsDrawable(points As PointF()) As Boolean
        Return Mode <> MultivariateEllipseMode.None AndAlso points IsNot Nothing AndAlso points.Length >= 3
    End Function

    ''' <summary>
    ''' 在绘图设备上绘制一个分组椭圆。
    ''' </summary>
    ''' <param name="g">绘图设备</param>
    ''' <param name="theme">主题（提供线宽等样式）</param>
    ''' <param name="points">该分组在<b>像素坐标系</b>下的散点</param>
    ''' <param name="color">分组颜色（填充会使用它的半透明版本）</param>
    Public Sub Draw(g As IGraphics, theme As PlotTheme, points As PointF(), color As Color)
        If Not IsDrawable(points) Then
            Return
        End If

        Dim center As PointF
        Dim radiusX As Single
        Dim radiusY As Single
        Dim rotation As Double

        If Mode = MultivariateEllipseMode.StandardDeviation Then
            Dim fit = Geometry.EstimateEllipse(points, StandardDeviations)

            center = fit.Center
            radiusX = fit.RadiusX
            radiusY = fit.RadiusY
            rotation = -fit.Rotation
        Else
            Dim fit = PCAEllipseCalculator.CalculateEllipse(points, ConfidenceLevel, EllipseType)

            center = fit.Center
            radiusX = fit.SemiMajorAxis
            radiusY = fit.SemiMinorAxis
            ' 像素坐标系的 Y 轴是翻转的，因此旋转方向需要取反
            rotation = -fit.RotationAngle
        End If

        If Single.IsNaN(radiusX) OrElse Single.IsNaN(radiusY) Then
            Return
        End If
        If radiusX <= 0.5F OrElse radiusY <= 0.5F Then
            Return
        End If

        ' 数值退化（例如所有点共线）时会算出天文数字的半轴，直接跳过而不是把整张图撑爆
        Dim limit As Single = std.Max(g.Size.Width, g.Size.Height) * 10.0F

        If radiusX > limit OrElse radiusY > limit Then
            Return
        End If

        Try
            g.TranslateTransform(center.X, center.Y)
            g.RotateTransform(CSng(rotation * 180.0 / std.PI))

            Using brush As New SolidBrush(Color.FromArgb(std.Max(0, std.Min(255, FillAlpha)), color))
                g.FillEllipse(brush, -radiusX, -radiusY, radiusX * 2, radiusY * 2)
            End Using

            Using pen As New Pen(Color.FromArgb(210, color), If(LineWidth > 0, LineWidth, theme.LineWidth))
                If Dashed Then
                    pen.DashStyle = DashStyle.Dash
                End If

                g.DrawEllipse(pen, -radiusX, -radiusY, radiusX * 2, radiusY * 2)
            End Using
        Finally
            ' 无论如何都要恢复变换矩阵，否则后续图元会被一起旋转
            g.ResetTransform()
        End Try
    End Sub

End Class
