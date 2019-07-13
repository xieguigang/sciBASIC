#Region "Microsoft.VisualBasic::422192766447956499018257821163cb, gr\Microsoft.VisualBasic.Imaging\Extensions.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    ' Module Extensions
    ' 
    '     Function: AsGDIImage, AsSVG
    ' 
    '     Sub: (+4 Overloads) FillCircles
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Imaging.Math2D
Imports Microsoft.VisualBasic.Linq

Public Module Extensions

    <Extension>
    Public Function AsSVG(img As GraphicsData, Optional comment$ = Nothing) As SVGData
        Dim svg As SVGData = DirectCast(img, SVGData)
        svg.XmlComment = comment
        Return svg
    End Function

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

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Sub FillCircles(ByRef g As IGraphics, brush As Brush, points As Point(), radius#)
        Call g.FillCircles(brush, points.Select(Function(p) p.PointF).ToArray, radius)
    End Sub

    <Extension>
    Public Sub FillCircles(ByRef g As IGraphics, brush As Brush, points As PointF(), radius#)
        Dim size As New Size(radius * 2, radius * 2)
        Dim offset = -radius

        For Each point As PointF In points
            Dim rect As New RectangleF(point.OffSet2D(offset, offset), size)
            Call g.FillEllipse(brush, rect)
        Next
    End Sub

    <Extension>
    Public Sub FillCircles(ByRef g As IGraphics, points As Point(), fill As Func(Of Integer, Point, Brush), radius#)
        Dim fillHandler As Func(Of Integer, PointF, Brush) = Function(i%, p As PointF) fill(i, p.ToPoint)
        Call g.FillCircles(points.PointF, fillHandler, radius)
    End Sub

    <Extension>
    Public Sub FillCircles(ByRef g As IGraphics, points As PointF(), fill As Func(Of Integer, PointF, Brush), radius#)
        Dim size As New Size(radius * 2, radius * 2)
        Dim offset = -radius

        For Each point As SeqValue(Of PointF) In points.SeqIterator
            Dim rect As New RectangleF((+point).OffSet2D(offset, offset), size)
            Call g.FillEllipse(fill(point, point), rect)
        Next
    End Sub
End Module
