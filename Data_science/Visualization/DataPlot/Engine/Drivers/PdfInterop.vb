Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.MIME.Html.CSS

' ============================================================================
'  PdfInterop.vb - PDF 绘图驱动
'
'  绘制阶段直接使用 GDI 栅格画布（图表里的抗锯齿文本、半透明填充与渐变
'  在矢量设备上的还原度很差），只在取回绘图数据的那一刻把位图嵌入 PDF 页面。
'  因此 PDF 驱动与 GDI 驱动共用同一套绘制代码，不需要为 PDF 单独实现 IGraphics。
' ============================================================================

''' <summary>
''' PDF 绘图驱动：绘制复用 GDI 栅格画布，导出时由 <see cref="PdfData"/> 把位图嵌入 PDF 页面。
''' </summary>
Public Class PdfInterop : Inherits DeviceInterop

    ''' <summary>最近一次 <see cref="CreateGraphic"/> 所使用的分辨率</summary>
    Private dotsPerInch As Integer = 300

    ''' <summary>
    ''' 创建一块 GDI 栅格画布用于绘制（PDF 导出不需要矢量画布）
    ''' </summary>
    ''' <param name="size">画布尺寸</param>
    ''' <param name="fill">背景填充色</param>
    ''' <param name="dpi">分辨率，决定 PDF 页面的物理尺寸换算</param>
    ''' <returns></returns>
    Public Overrides Function CreateGraphic(size As Size, fill As Color, dpi As Integer) As IGraphics
        dotsPerInch = If(dpi <= 0, 300, dpi)

        Return DriverLoad.CreateGraphicsDevice(size, fill, dpi:=dotsPerInch, driver:=Drivers.GDI)
    End Function

    ''' <summary>
    ''' 基于已有的位图创建画布（PDF 驱动不参与二次绘制，这里与 GDI 行为一致）
    ''' </summary>
    ''' <param name="background"></param>
    ''' <param name="direct_access"></param>
    ''' <returns></returns>
    Public Overrides Function CreateCanvas2D(background As Bitmap, direct_access As Boolean) As IGraphics
        Return DriverLoad.CreateGraphicsDevice(background, direct_access, driver:=Drivers.GDI)
    End Function

    ''' <summary>
    ''' 基于已有的位图创建画布（PDF 驱动不参与二次绘制，这里与 GDI 行为一致）
    ''' </summary>
    ''' <param name="background"></param>
    ''' <param name="direct_access"></param>
    ''' <returns></returns>
    Public Overrides Function CreateCanvas2D(background As Image, direct_access As Boolean) As IGraphics
        Return DriverLoad.CreateGraphicsDevice(background, direct_access, driver:=Drivers.GDI)
    End Function

    ''' <summary>
    ''' 取回 PDF 格式的绘图数据
    ''' </summary>
    ''' <param name="g">绘制所用的 GDI 画布</param>
    ''' <param name="padding">画布边距（top, right, bottom, left）</param>
    ''' <returns></returns>
    Public Overrides Function GetData(g As IGraphics, padding As Integer()) As IGraphicsData
        Dim raster As New ImageData(
            DirectCast(g, GdiRasterGraphics).ImageResource,
            g.Size,
            New Padding(padding))

        Return New PdfData(raster, g.Size, New Padding(padding), dpi:=dotsPerInch)
    End Function
End Class
