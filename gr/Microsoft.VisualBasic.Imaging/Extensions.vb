Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Linq

Public Module Extensions

    ''' <summary>
    ''' 假若目标图像是svg类型，则会被合成为gdi图像，如果是gdi图像，则会被直接转换
    ''' </summary>
    ''' <param name="img"></param>
    ''' <returns></returns>
    <Extension> Public Function AsGDIImage(img As GraphicsData) As Image
        If img.Driver = Drivers.GDI Then
            Return DirectCast(img, ImageData).Image
        Else
            ' 将svg矢量图合成为gdi图像
            Return DirectCast(img, SVGData).Render
        End If
    End Function

    <Extension>
    Public Sub FillCircles(ByRef g As IGraphics, brush As Brush, points As Point(), radius#)
        Dim size As New Size(radius * 2, radius * 2)
        Dim offset = -radius
        For Each point As Point In points
            Dim rect As New Rectangle(point.OffSet2D(offset, offset), size)
            Call g.FillEllipse(brush, rect)
        Next
    End Sub

    <Extension>
    Public Sub FillCircles(ByRef g As IGraphics, points As Point(), fill As Func(Of Integer, Point, Brush), radius#)
        Dim size As New Size(radius * 2, radius * 2)
        Dim offset = -radius

        For Each point As SeqValue(Of Point) In points.SeqIterator
            Dim rect As New Rectangle((+point).OffSet2D(offset, offset), size)
            Call g.FillEllipse(fill(point, point), rect)
        Next
    End Sub
End Module
