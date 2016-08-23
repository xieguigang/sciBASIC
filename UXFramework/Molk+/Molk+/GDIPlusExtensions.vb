#Region "Microsoft.VisualBasic::2bd011d174371989f0b9a0ac2829cfa6, ..\visualbasic_App\UXFramework\Molk+\Molk+\GDIPlusExtensions.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports System.Runtime.CompilerServices
Imports System.Drawing
Imports System.Text.RegularExpressions
Imports System.Text

Public Module GDIPlusExtensions

    ''' <summary>
    ''' GDI+ device handle
    ''' </summary>
    ''' <remarks></remarks>
    Public Structure GDIPlusDeviceHandle

        ''' <summary>
        ''' GDI+ device handle
        ''' </summary>
        ''' <remarks></remarks>
        Dim Gr_Device As Graphics
        ''' <summary>
        ''' GDI+ device handle memory
        ''' </summary>
        ''' <remarks></remarks>
        Friend InternalBitmapResources As Image

        ''' <summary>
        ''' GDI+ device handle memory
        ''' </summary>
        ''' <remarks></remarks>
        Public ReadOnly Property ImageResource As Image
            Get
                Return InternalBitmapResources
            End Get
        End Property

        Public ReadOnly Property Width As Integer
            Get
                Return InternalBitmapResources.Width
            End Get
        End Property

        Public ReadOnly Property Height As Integer
            Get
                Return InternalBitmapResources.Height
            End Get
        End Property

        Public ReadOnly Property Size As Size
            Get
                Return InternalBitmapResources.Size
            End Get
        End Property

        Friend _PointCenter As Point

        Public ReadOnly Property Center As Point
            Get
                Return _PointCenter
            End Get
        End Property

        Public Function Save(Path As String) As Boolean
            Try
                Call InternalBitmapResources.Save(Path, System.Drawing.Imaging.ImageFormat.Png)
            Catch ex As Exception
                Call Console.WriteLine(ex.ToString)
                Return False
            End Try

            Return True
        End Function

        Public Overrides Function ToString() As String
            Return InternalBitmapResources.Size.ToString
        End Function

        Public Shared Function InternalCreateDevice(r As Drawing.Size, Optional filled As Color = Nothing) As GDIPlusDeviceHandle
            Return r.CreateGDIDevice(filled)
        End Function

        Public Shared Narrowing Operator CType(obj As GDIPlusDeviceHandle) As Image
            Return obj.InternalBitmapResources
        End Operator

        Public Shared Widening Operator CType(obj As Image) As GDIPlusDeviceHandle
            Dim Gr As Graphics = Graphics.FromImage(obj)
            Return New GDIPlusDeviceHandle With {.InternalBitmapResources = obj, .Gr_Device = Gr, ._PointCenter = New Point(obj.Width / 2, obj.Height / 2)}
        End Operator

        Public Shared Widening Operator CType(obj As Bitmap) As GDIPlusDeviceHandle
            Dim Gr As Graphics = Graphics.FromImage(obj)
            Return New GDIPlusDeviceHandle With {.InternalBitmapResources = obj, .Gr_Device = Gr, ._PointCenter = New Point(obj.Width / 2, obj.Height / 2)}
        End Operator
    End Structure

    Private ReadOnly GrCommon As Graphics = Graphics.FromImage(New Bitmap(64, 64))

    <Extension> Public Function MeasureString(s As String, Font As Font) As Size
        Dim Size = GrCommon.MeasureString(s, Font)
        Return New Size(Size.Width, Size.Height)
    End Function

    Public Const FONT_FAMILY_MICROSOFT_YAHEI As String = "Microsoft YaHei"
    Public Const FONT_FAMILY_UBUNTU As String = "Ubuntu"
    Public Const FONT_FAMILY_SEGOE_UI As String = "Segoe UI"

    ''' <summary>
    ''' 为图像添加边框
    ''' </summary>
    ''' <param name="Handle"></param>
    ''' <param name="pen">默认的绘图笔为黑色的1个像素的边框</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension> Public Function ImageAddFrame(Handle As GDIPlusDeviceHandle, Optional pen As Drawing.Pen = Nothing, Optional offset As Integer = 0) As GDIPlusDeviceHandle
        Dim TopLeft As New Point(0 + offset, 0 + offset)
        Dim TopRight As New Point(Handle.Width - offset, 0 + offset)
        Dim BottomLeft As New Point(0 + offset, Handle.Height - offset)
        Dim BottomRight As New Point(Handle.Width - offset, Handle.Height - offset)

        If pen Is Nothing Then
            pen = Drawing.Pens.Black
        End If

        Call Handle.Gr_Device.DrawLine(pen, TopLeft, TopRight)
        Call Handle.Gr_Device.DrawLine(pen, TopRight, BottomRight)
        Call Handle.Gr_Device.DrawLine(pen, BottomRight, BottomLeft)
        Call Handle.Gr_Device.DrawLine(pen, BottomLeft, TopLeft)

        Return Handle
    End Function

    ''' <summary>
    ''' 创建一个GDI+的绘图设备
    ''' </summary>
    ''' <param name="r"></param>
    ''' <param name="filled">默认的背景填充颜色为白色</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension> Public Function CreateGDIDevice(r As Drawing.SizeF, Optional filled As Color = Nothing) As GDIPlusDeviceHandle
        Return (New Size(r.Width, r.Height)).CreateGDIDevice(filled)
    End Function

    <Extension> Public Function GDIPlusDeviceHandleFromImageFile(path As String) As GDIPlusDeviceHandle
        Dim Image As Image = Image.FromFile(path)
        Dim GrDevice As Graphics = Graphics.FromImage(Image)
        GrDevice.CompositingQuality = Drawing2D.CompositingQuality.HighQuality
        GrDevice.TextRenderingHint = Drawing.Text.TextRenderingHint.ClearTypeGridFit
        Return New GDIPlusDeviceHandle With {.InternalBitmapResources = Image, .Gr_Device = GrDevice, ._PointCenter = New Point(Image.Width / 2, Image.Height / 2)}
    End Function

    <Extension> Public Function GDIPlusDeviceHandleFromImageResource(image As Image) As GDIPlusDeviceHandle
        Dim GrDevice As Graphics = Graphics.FromImage(image)
        GrDevice.CompositingQuality = Drawing2D.CompositingQuality.HighQuality
        GrDevice.TextRenderingHint = Drawing.Text.TextRenderingHint.ClearTypeGridFit
        Return New GDIPlusDeviceHandle With {.InternalBitmapResources = image, .Gr_Device = GrDevice, ._PointCenter = New Point(image.Width / 2, image.Height / 2)}
    End Function

    <Extension> Public Function OffSet(p As Point, x As Integer, y As Integer) As Point
        Return New Point(x + p.X, y + p.Y)
    End Function

    ''' <summary>
    ''' 创建一个GDI+的绘图设备
    ''' </summary>
    ''' <param name="r"></param>
    ''' <param name="filled">默认的背景填充颜色为白色</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension> Public Function CreateGDIDevice(r As Drawing.Size, Optional filled As Color = Nothing) As GDIPlusDeviceHandle
        Dim Bitmap As Bitmap

        Try
            Bitmap = New Bitmap(r.Width, r.Height)
        Catch ex As Exception
            Throw New Exception(String.Format("[{0}]  -----> ", r.ToString) & vbCrLf & vbCrLf & ex.ToString)
        End Try

        Dim GrDevice As Graphics = Graphics.FromImage(Bitmap)

        If filled = Nothing Then
            filled = Color.White
        End If

        Call GrDevice.FillRectangle(New SolidBrush(filled), New Rectangle(New Point, Bitmap.Size))

        GrDevice.CompositingQuality = Drawing2D.CompositingQuality.HighQuality
        GrDevice.TextRenderingHint = Drawing.Text.TextRenderingHint.ClearTypeGridFit

        Return New GDIPlusDeviceHandle With {.InternalBitmapResources = Bitmap, .Gr_Device = GrDevice, ._PointCenter = New Point(Bitmap.Width / 2, Bitmap.Height / 2)}
    End Function

    ''' <summary>
    ''' 图片剪裁小方块区域
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension> Public Function ImageCrop(source As Image, Location As Point, Size As Size) As Image
        Dim CloneRect As New Rectangle(Location, Size)
        Dim BmpSource As New Bitmap(source.Width, source.Height)

        Using Gr As Graphics = Graphics.FromImage(BmpSource)
            Call Gr.DrawImage(source, New Point)
        End Using

        Dim CloneBitmap As Bitmap = BmpSource.Clone(CloneRect, source.PixelFormat)
        Return CloneBitmap
    End Function

    ''' <summary>
    ''' 图片剪裁为圆形的头像
    ''' </summary>
    ''' <param name="Head">要求为正方形或者近似正方形</param>
    ''' <param name="OutSize"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension> Public Function TrimRoundHead(Head As Image, OutSize As Integer) As Image
        Dim Bitmap As Bitmap = New Bitmap(OutSize, OutSize)

        Using Gr = Graphics.FromImage(Bitmap)

            Gr.CompositingQuality = Drawing2D.CompositingQuality.HighQuality
            Gr.InterpolationMode = Drawing2D.InterpolationMode.HighQualityBicubic
            Gr.SmoothingMode = Drawing2D.SmoothingMode.HighQuality
            Gr.TextRenderingHint = Drawing.Text.TextRenderingHint.ClearTypeGridFit

            Dim rH As Drawing.Brush = New Drawing.TextureBrush(Head)
            Call Gr.FillPie(rH, New Rectangle(0, 0, OutSize, OutSize), 0, 360)
            Return Bitmap
        End Using
    End Function

    Const RGB_EXPRESSION As String = "\d+,\d+,\d+"

    <Extension> Public Function ToColor(str As String) As Color
        Dim s As String = Regex.Match(str, RGB_EXPRESSION).Value
        If String.IsNullOrEmpty(s) Then
            Return Color.FromName(str)
        Else
            Dim Tokens = s.Split(",")
            Dim R As Integer = Val(Tokens(0)), G As Integer = Val(Tokens(1)), B As Integer = Val(Tokens(2))
            Return Color.FromArgb(R, G, B)
        End If
    End Function

    ''' <summary>
    ''' 羽化
    ''' </summary>
    ''' <param name="Image"></param>
    ''' <param name="y1"></param>
    ''' <param name="y2"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension> Public Function Vignette(Image As Image, y1 As Integer, y2 As Integer, Optional RenderColor As Color = Nothing) As Image
        Dim Gr = Image.GDIPlusDeviceHandleFromImageResource
        Dim Alpha As Integer = 0
        Dim delta = (Math.PI / 2) / Math.Abs(y1 - y2)
        Dim offset As Double = 0

        If RenderColor = Nothing OrElse RenderColor.IsEmpty Then
            RenderColor = Color.White
        End If

        For y As Integer = y1 To y2
            Dim Color = System.Drawing.Color.FromArgb(Alpha, RenderColor.R, RenderColor.G, RenderColor.B)
            Call Gr.Gr_Device.DrawLine(New Pen(Color), New Point(0, y), New Point(Gr.Width, y))

            Alpha = 255 * Math.Sin(offset) ^ 2
            offset += delta
        Next

        Dim Rect = New Rectangle(New Point(0, y2), New Size(Image.Width, Image.Height - y2))
        Call Gr.Gr_Device.FillRectangle(New SolidBrush(RenderColor), Rect)

        Return Gr.ImageResource
    End Function
End Module
