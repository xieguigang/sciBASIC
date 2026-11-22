Imports System.IO

''' <summary>
''' PPT 文档对象：管理多张幻灯片页面，并将其导出为 .pptx 文件。
''' 
''' 典型用法：
''' <code>
''' Dim doc As New PresentationDocument()
''' Dim page = doc.AddPage()
''' Call page.DrawString("Hello", New Font("Arial", 24), Brushes.Black, 50, 50)
''' Call doc.Save("output.pptx")
''' </code>
''' </summary>
Public Class PresentationDocument

    Private ReadOnly _pages As New List(Of PresentationPageGraphic)
    Private ReadOnly _defaultSize As Drawing.Size

    ''' <summary>默认 16:9 页面尺寸。</summary>
    Public Sub New()
        Me.New(New Drawing.Size(1000, 563))
    End Sub

    ''' <summary>以像素（100 dpi）指定默认页面尺寸。</summary>
    Public Sub New(size As Drawing.Size)
        _defaultSize = size
    End Sub

    ''' <summary>以英寸指定默认页面尺寸。</summary>
    Public Sub New(widthInches As Single, heightInches As Single)
        Me.New(New Drawing.Size(CInt(widthInches * 100), CInt(heightInches * 100)))
    End Sub

    ''' <summary>所有页面的只读视图。</summary>
    Public ReadOnly Property Pages As IReadOnlyList(Of PresentationPageGraphic)
        Get
            Return _pages
        End Get
    End Property

    ''' <summary>文档默认页面尺寸。</summary>
    Public ReadOnly Property PageSize As Drawing.Size
        Get
            Return _defaultSize
        End Get
    End Property

    ''' <summary>新增一张幻灯片（使用文档默认尺寸）。</summary>
    Public Function AddPage() As PresentationPageGraphic
        Return AddPage(_defaultSize)
    End Function

    ''' <summary>新增一张自定义尺寸的幻灯片。</summary>
    Public Function AddPage(size As Drawing.Size) As PresentationPageGraphic
        Dim page As New PresentationPageGraphic(size)
        _pages.Add(page)
        Return page
    End Function

    ''' <summary>新增一张以英寸指定尺寸的幻灯片。</summary>
    Public Function AddPage(widthInches As Single, heightInches As Single) As PresentationPageGraphic
        Return AddPage(New Drawing.Size(CInt(widthInches * 100), CInt(heightInches * 100)))
    End Function

    ''' <summary>移除指定页面。</summary>
    Public Function RemovePage(page As PresentationPageGraphic) As Boolean
        Return _pages.Remove(page)
    End Function

    ''' <summary>保存为 .pptx 文件。</summary>
    Public Sub Save(filePath As String)
        Call New PptxPackager().Save(Me, filePath)
    End Sub

    ''' <summary>保存到流。</summary>
    Public Sub SaveAs(destination As Stream, Optional leaveOpen As Boolean = False)
        Call New PptxPackager().SaveAs(Me, destination, leaveOpen)
    End Sub
End Class
