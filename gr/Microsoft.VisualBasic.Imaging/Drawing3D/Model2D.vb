Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Drawing3D.IsoMetric

Namespace Drawing3D

    Public Class Model2D

        ''' <summary>
        ''' 实际的模型数据
        ''' </summary>
        Friend path As Path3D
        Friend baseColor As Color
        Friend paint As SolidBrush
        Friend drawn As Integer
        Friend transformedPoints As Point3D()

        ''' <summary>
        ''' 经过模型数据<see cref="path"/>转换之后所得到的绘图所使用的对象模型
        ''' </summary>
        Friend drawPath As Path2D

        Friend Sub New(item As Model2D)
            transformedPoints = item.transformedPoints
            drawPath = item.drawPath
            drawn = item.drawn
            Me.paint = item.paint
            Me.path = item.path
            Me.baseColor = item.baseColor
        End Sub

        Friend Sub New(___path As Path3D, baseColor As Color)
            drawPath = New Path2D
            drawn = 0
            Me.baseColor = baseColor
            Me.paint = New SolidBrush(Color.FromArgb(CInt(Fix(baseColor.A)), CInt(Fix(baseColor.R)), CInt(Fix(baseColor.G)), CInt(Fix(baseColor.B))))
            Me.path = ___path
        End Sub
    End Class
End Namespace