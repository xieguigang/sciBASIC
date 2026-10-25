Imports System.Drawing
Imports System.IO
Imports Microsoft.VisualBasic.ApplicationServices.Debugging
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.MIME.application.pdf
Imports Microsoft.VisualBasic.MIME.Html.CSS
Imports Microsoft.VisualBasic.Net.Http

' ============================================================================
'  PdfData.vb - PDF 格式的绘图数据
'
'  PDF 驱动是「栅格嵌入」式的：绘制阶段仍然使用 GDI 画布（因为图表里有大量
'  抗锯齿文本与半透明填充，矢量化的收益很低，反而容易丢细节），导出时把
'  位图按当前 dpi 换算成物理尺寸后整页嵌入 PDF。
'  需要真正的矢量输出时请使用 SVG 驱动。
' ============================================================================

''' <summary>
''' PDF 绘图数据：把栅格画布上的图表按物理尺寸嵌入到一页 PDF 文档之中。
''' </summary>
''' <remarks>
''' 页面尺寸由画布像素尺寸与 dpi 共同决定：``页面(pt) = 像素 / dpi * 72``，
''' 因此用高 dpi 出图可以得到与屏幕上一致大小、但打印时依然清晰的插图。
''' </remarks>
Public Class PdfData : Inherits GraphicsData
    Implements IVisualStudioPreviews

    ''' <summary>底层的栅格图形数据（用于先落地一张 png）</summary>
    ReadOnly raster As ImageData
    ''' <summary>本次出图所使用的分辨率</summary>
    ReadOnly dotsPerInch As Integer

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="raster">栅格图形数据</param>
    ''' <param name="size">画布尺寸（像素）</param>
    ''' <param name="padding">画布边距</param>
    ''' <param name="dpi">分辨率，小于等于 0 时按 300 处理</param>
    Sub New(raster As ImageData, size As Size, padding As Padding, Optional dpi As Integer = 300)
        MyBase.New(raster, size, padding)

        Me.raster = raster
        Me.dotsPerInch = If(dpi <= 0, 300, dpi)
    End Sub

    Public Overrides ReadOnly Property Driver As Drivers
        Get
            Return Drivers.PDF
        End Get
    End Property

    Public Overrides ReadOnly Property Previews As String Implements IVisualStudioPreviews.Previews
        Get
            Return "PDF"
        End Get
    End Property

    ''' <summary>
    ''' 把图表写成 PDF 文件
    ''' </summary>
    ''' <param name="path">*.pdf 文件路径</param>
    ''' <returns></returns>
    Public Overrides Function Save(path As String) As Boolean
        Dim png As String = TempPng()

        Try
            Call raster.Save(png)
            Call BuildPdf(png, path)
        Finally
            DeleteTemp(png)
        End Try

        Return System.IO.File.Exists(path)
    End Function

    ''' <summary>
    ''' 把 PDF 数据写入目标流
    ''' </summary>
    ''' <param name="out"></param>
    ''' <returns></returns>
    Public Overrides Function Save(out As Stream) As Boolean
        Dim png As String = TempPng()
        Dim pdf As String = Path.Combine(Path.GetTempPath(), Guid.NewGuid.ToString("N") & ".pdf")

        Try
            Call raster.Save(png)
            Call BuildPdf(png, pdf)

            Using fs As Stream = System.IO.File.OpenRead(pdf)
                Call fs.CopyTo(out)
            End Using

            Call out.Flush()
        Finally
            DeleteTemp(png)
            DeleteTemp(pdf)
        End Try

        Return True
    End Function

    ''' <summary>
    ''' 把 PDF 数据编码为 data uri
    ''' </summary>
    ''' <returns></returns>
    Public Overrides Function GetDataURI() As DataURI
        Using ms As New MemoryStream()
            Call Save(ms)
            Call ms.Seek(0, SeekOrigin.Begin)

            Return New DataURI(ms, "application/pdf")
        End Using
    End Function

    ''' <summary>生成一张临时 png 的文件路径</summary>
    Private Shared Function TempPng() As String
        Return Path.Combine(Path.GetTempPath(), Guid.NewGuid.ToString("N") & ".png")
    End Function

    Private Shared Sub DeleteTemp(file As String)
        Try
            If System.IO.File.Exists(file) Then System.IO.File.Delete(file)
        Catch
            ' 临时文件清理失败不影响出图结果
        End Try
    End Sub

    ''' <summary>按像素尺寸与 dpi 换算物理页面尺寸，并把 png 整页嵌入</summary>
    Private Sub BuildPdf(png As String, pdf As String)
        Dim widthPt As Double = Me.Width / dotsPerInch * 72.0
        Dim heightPt As Double = Me.Height / dotsPerInch * 72.0
        Dim doc As New PdfDocument()

        ' PageSetup 使用 twips（1pt = 20twips），Image 使用 pt
        Call doc.PageSetup(CInt(widthPt * 20), CInt(heightPt * 20), 0, 0, 0, 0).Image(png, widthPt, heightPt)
        Call doc.Save(pdf)
    End Sub
End Class
