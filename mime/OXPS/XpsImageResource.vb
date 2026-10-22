' ============================================================================
' XpsImageResource.vb - 图片资源
'
' XPS 的页面以 <Path> 的 <ImageBrush ImageSource="/Resources/Images/1.png"> 引用
' 包内图片。本类负责：判定 PNG / JPEG、探测像素尺寸（自带文件头解析，避免为
' 一个尺寸探测而引用 docx 工程的 ImageHelper）、分配包内部件名。
'
' 文件缺失或格式不支持时返回 Nothing，由上层降级为文本占位段落。
' ============================================================================

Imports System.IO

''' <summary>XPS 图片资源：PNG / JPEG 的像素尺寸探测与包内部件名分配。</summary>
Public Class XpsImageResource

    ''' <summary>包内图片部件所在目录。</summary>
    Public Const ImagesFolder As String = "Resources/Images"

    ''' <summary>图片部件名（如 ``Resources/Images/1.png``）。</summary>
    Public Property PartName As String = ""

    ''' <summary>图片内容类型（image/png 或 image/jpeg）。</summary>
    Public Property ContentType As String = ""

    ''' <summary>图片原始字节。</summary>
    Public Property Data As Byte()

    ''' <summary>像素宽度。</summary>
    Public Property PixelWidth As Integer = 0

    ''' <summary>像素高度。</summary>
    Public Property PixelHeight As Integer = 0

    ''' <summary>ImageBrush/@ImageSource 使用值（``/Resources/Images/1.png``）。</summary>
    ''' <returns></returns>
    Public ReadOnly Property Uri As String
        Get
            If String.IsNullOrEmpty(PartName) Then
                Return ""
            End If

            Return "/" & PartName
        End Get
    End Property

    ''' <summary>
    ''' 按原始像素尺寸换算出显示尺寸（磅）：按 96 dpi 换算，1 px = 0.75 pt。
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property DisplayWidth As Double
        Get
            Return PixelWidth * 0.75
        End Get
    End Property

    ''' <summary>按 96 dpi 换算的显示高度（磅）。</summary>
    Public ReadOnly Property DisplayHeight As Double
        Get
            Return PixelHeight * 0.75
        End Get
    End Property

    ''' <summary>
    ''' 读取图片文件并创建资源。
    ''' </summary>
    ''' <param name="imageFile">png / jpg / jpeg 文件路径。</param>
    ''' <param name="index">资源序号（用于生成部件名，从 1 开始）。</param>
    ''' <returns>资源实例；文件不存在或格式不支持时返回 Nothing。</returns>
    Public Shared Function Create(imageFile As String, index As Integer) As XpsImageResource
        ' 注意：参数不能命名为 file —— VB 大小写不敏感，会遮蔽 File 类型
        If String.IsNullOrEmpty(imageFile) OrElse Not File.Exists(imageFile) Then
            Return Nothing
        End If

        Dim ext As String = Path.GetExtension(imageFile).TrimStart("."c).ToLower()

        Select Case ext
            Case "png", "jpg", "jpeg"
                ' 支持
            Case Else
                Return Nothing
        End Select

        Dim data As Byte()

        Try
            data = File.ReadAllBytes(imageFile)
        Catch ex As Exception
            Call Console.Error.WriteLine($"[警告] 图片读取失败，已跳过: {imageFile} ({ex.Message})")
            Return Nothing
        End Try

        Dim size As (Width As Integer, Height As Integer)

        If ext = "png" Then
            size = PngSize(data)
        Else
            size = JpegSize(data)
        End If

        If size.Width <= 0 OrElse size.Height <= 0 Then
            Return Nothing
        End If

        Dim resource As New XpsImageResource() With {
            .PartName = $"{ImagesFolder}/{index}.{(If(ext = "png", "png", "jpg"))}",
            .ContentType = If(ext = "png", "image/png", "image/jpeg"),
            .Data = data,
            .PixelWidth = size.Width,
            .PixelHeight = size.Height
        }

        Return resource
    End Function

    ''' <summary>解析 PNG 的 IHDR 得到像素尺寸。</summary>
    ''' <param name="data"></param>
    ''' <returns></returns>
    Public Shared Function PngSize(data As Byte()) As (Width As Integer, Height As Integer)
        ' 8 字节签名 + 4 字节块长度 + "IHDR" + 宽度(4) + 高度(4)
        If data Is Nothing OrElse data.Length < 24 Then
            Return (0, 0)
        End If
        If data(0) <> &H89 OrElse data(1) <> &H50 OrElse data(2) <> &H4E OrElse data(3) <> &H47 Then
            Return (0, 0)
        End If

        Dim width As Integer = ReadBigEndian(data, 16)
        Dim height As Integer = ReadBigEndian(data, 20)

        Return (width, height)
    End Function

    ''' <summary>解析 JPEG 的 SOF 段得到像素尺寸。</summary>
    ''' <param name="data"></param>
    ''' <returns></returns>
    Public Shared Function JpegSize(data As Byte()) As (Width As Integer, Height As Integer)
        If data Is Nothing OrElse data.Length < 4 Then
            Return (0, 0)
        End If
        If data(0) <> &HFF OrElse data(1) <> &HD8 Then
            Return (0, 0)
        End If

        Dim i As Integer = 2

        While i + 9 < data.Length
            If data(i) <> &HFF Then
                i += 1
                Continue While
            End If

            Dim marker As Byte = data(i + 1)

            If marker = &HD8 OrElse marker = &HD9 Then
                i += 2
                Continue While
            End If
            ' SOF0 / SOF1 / SOF2 / SOF3 / SOF5..SOF7 / SOF9..SOF11 / SOF13..SOF15
            If (marker >= &HC0 AndAlso marker <= &HC3) OrElse
               (marker >= &HC5 AndAlso marker <= &HC7) OrElse
               (marker >= &HC9 AndAlso marker <= &HCB) OrElse
               (marker >= &HCD AndAlso marker <= &HCF) Then

                Dim height As Integer = (CInt(data(i + 5)) << 8) + CInt(data(i + 6))
                Dim width As Integer = (CInt(data(i + 7)) << 8) + CInt(data(i + 8))

                Return (width, height)
            End If

            Dim length As Integer = (CInt(data(i + 2)) << 8) + CInt(data(i + 3))

            If length <= 0 Then
                Return (0, 0)
            End If

            i += 2 + length
        End While

        Return (0, 0)
    End Function

    Private Shared Function ReadBigEndian(data As Byte(), offset As Integer) As Integer
        Return (CInt(data(offset)) << 24) Or (CInt(data(offset + 1)) << 16) Or
               (CInt(data(offset + 2)) << 8) Or CInt(data(offset + 3))
    End Function

End Class
