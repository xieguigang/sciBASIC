Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing3D
Imports Microsoft.VisualBasic.Imaging.Drawing3D.Math3D

Namespace Plot3D.Device

    ''' <summary>
    ''' 因为先绘制坐标轴再绘制系列点，会没有太多层次感，所以在这里首先需要将这些需要绘制的原件转换为这个元素对象，然后做一次Z排序生成绘图顺序
    ''' 最后再调用<see cref="Draw"/>方法进行3D图表的绘制
    ''' </summary>
    Public MustInherit Class Element3D

        Public Property Location As Point3D

        Public MustOverride Sub Draw(g As IGraphics)

        Protected Function GetPosition(g As IGraphics) As PointF
            Return Location.PointXY(g.Size)
        End Function
    End Class

    Public Class Label : Inherits Element3D

        Public Property Text As String
        Public Property Font As Font
        Public Property Color As Brush

        Public Overrides Sub Draw(g As IGraphics)
            Call g.DrawString(Text, Font, Color, GetPosition(g))
        End Sub
    End Class
End Namespace