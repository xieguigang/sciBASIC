' ============================================================================
'  Mapper.vb - Canvas 兼容层的数据-绘图坐标映射器
'
'  从旧 Plots 项目的 g/Mapper.vb 迁移而来的最小化版本：
'  保留 xrange/yrange 值域与 ScallingWidth 映射，供条形图等布局计算使用。
' ============================================================================

Imports System.Drawing
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Math.LinearAlgebra

Namespace Canvas

    ''' <summary>
    ''' 将数据坐标转换为绘图坐标（宽度/高度比例）
    ''' </summary>
    Public Class Mapper

        ''' <summary>
        ''' 坐标轴的数据
        ''' </summary>
        Public ReadOnly xAxis, yAxis As Vector

        ''' <summary>
        ''' x,y轴分别的最大值和最小值的差值
        ''' </summary>
        Public ReadOnly dx#, dy#
        Public ReadOnly xmin, ymin As Single

        ''' <summary>
        '''Create a new mapper from a given data range.
        ''' </summary>
        Sub New(xrange As DoubleRange, yrange As DoubleRange)
            xmin = xrange.Min
            dx = xrange.Max - xrange.Min
            ymin = yrange.Min
            dy = yrange.Max - yrange.Min
        End Sub

        ''' <summary>
        ''' 将数据值 <paramref name="x"/> 映射为在总宽度 <paramref name="width"/> 中的像素宽度
        ''' </summary>
        Public Function ScallingWidth(x As Double, width As Integer) As Single
            If dx = 0 Then
                Return 0
            End If

            Return width * (x - xmin) / dx
        End Function
    End Class
End Namespace