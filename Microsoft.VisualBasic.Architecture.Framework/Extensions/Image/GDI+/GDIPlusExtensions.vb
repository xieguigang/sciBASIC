#Region "Microsoft.VisualBasic::f7f7f7e7c23e0109e8b962d4cb18714c, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\Extensions\Image\GDI+\GDIPlusExtensions.vb"

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

Imports System.Drawing
Imports System.Drawing.Imaging
Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Scripting.MetaData

Namespace Imaging

    ''' <summary>
    ''' GDI+
    ''' </summary>
    '''
    <PackageNamespace("GDI+", Description:="GDI+ GDIPlus Extensions Module to provide some useful interface.",
                  Publisher:="xie.guigang@gmail.com",
                  Revision:=58,
                  Url:="http://gcmodeller.org")>
    Public Module GDIPlusExtensions

        <Extension>
        Public Sub DrawCircle(ByRef g As Graphics, centra As PointF, r!, color As SolidBrush)
            Dim d = r * 2

            With centra
                Call g.FillPie(color, .X - r, .Y - r, d, d, 0, 360)
            End With
        End Sub

        <Extension>
        Public Sub DrawCircle(ByRef g As Graphics, centra As PointF, r!, color As Color, Optional fill As Boolean = True)
            Dim d = r * 2

            With centra
                If fill Then
                    Call g.FillPie(New SolidBrush(color), .X - r, .Y - r, d, d, 0, 360)
                Else
                    Call g.DrawPie(New Pen(color), .X - r, .Y - r, d, d, 0, 360)
                End If
            End With
        End Sub

        ''' <summary>
        ''' 这个方形区域的面积
        ''' </summary>
        ''' <param name="rect"></param>
        ''' <returns></returns>
        <Extension>
        Public Function Area(rect As Rectangle) As Double
            Return rect.Width * rect.Height
        End Function

        ''' <summary>
        ''' 返回整个图像的区域
        ''' </summary>
        ''' <param name="img"></param>
        ''' <returns></returns>
        <Extension>
        Public Function EntireImage(img As Image) As Rectangle
            Dim size As Size = img.Size
            Return New Rectangle(New Point, size)
        End Function

        <Extension>
        Public Function InRegion(x As Point, rect As Rectangle) As Boolean
            Return New PointF(x.X, x.Y).InRegion(rect)
        End Function

        <Extension>
        Public Function InRegion(x As PointF, rect As Rectangle) As Boolean
            If x.X < rect.Left OrElse x.X > rect.Right Then
                Return False
            End If
            If x.Y < rect.Top OrElse x.Y > rect.Bottom Then
                Return False
            End If

            Return True
        End Function

        <Extension> Public Function GetCenter(size As Size) As Point
            Return New Point(size.Width / 2, size.Height / 2)
        End Function

        <ExportAPI("To.Icon")>
        <Extension> Public Function GetIcon(res As Image) As Icon
            Return Drawing.Icon.FromHandle(New Bitmap(res).GetHicon)
        End Function

        <ExportAPI("To.Icon")>
        <Extension> Public Function GetIcon(res As Bitmap) As Icon
            Return Drawing.Icon.FromHandle(res.GetHicon)
        End Function

        ''' <summary>
        ''' Load image from a file and then close the file handle.
        ''' (使用<see cref="Image.FromFile(String)"/>函数在加载完成图像到Dispose这段之间内都不会释放文件句柄，
        ''' 则使用这个函数则没有这个问题，在图片加载之后会立即释放掉文件句柄)
        ''' </summary>
        ''' <param name="path"></param>
        ''' <returns></returns>
        <ExportAPI("LoadImage"), Extension>
        Public Function LoadImage(path As String) As Image
            Dim stream As Byte() = FileIO.FileSystem.ReadAllBytes(path)
            Dim res = Image.FromStream(stream:=New IO.MemoryStream(stream))
            Return res
        End Function

        <ExportAPI("LoadImage")>
        <Extension> Public Function LoadImage(rawStream As Byte()) As Image
            Dim res = Image.FromStream(stream:=New IO.MemoryStream(rawStream))
            Return res
        End Function

        ''' <summary>
        ''' 将图片对象转换为原始的字节流
        ''' </summary>
        ''' <param name="image"></param>
        ''' <returns></returns>
        '''
        <ExportAPI("Get.RawStream")>
        <Extension> Public Function GetRawStream(image As Image) As Byte()
            Dim stream As New IO.MemoryStream
            Call image.Save(stream, ImageFormat.Png)
            Return stream.ToArray
        End Function

        Private ReadOnly gdiShared As Graphics = Graphics.FromImage(New Bitmap(64, 64))

        ''' <summary>
        ''' Measures the specified string when drawn with the specified System.Drawing.Font.
        ''' </summary>
        ''' <param name="s">String to measure.</param>
        ''' <param name="Font">System.Drawing.Font that defines the text format of the string.</param>
        ''' <param name="XScaleSize"></param>
        ''' <param name="YScaleSize"></param>
        ''' <returns>This method returns a System.Drawing.SizeF structure that represents the size,
        ''' in the units specified by the System.Drawing.Graphics.PageUnit property, of the
        ''' string specified by the text parameter as drawn with the font parameter.
        ''' </returns>
        <Extension> Public Function MeasureString(s As String, Font As Font,
                                                  Optional XScaleSize As Single = 1,
                                                  Optional YScaleSize As Single = 1) As Size
            SyncLock gdiShared
                Call gdiShared.ScaleTransform(XScaleSize, YScaleSize)
                Dim sz As SizeF = gdiShared.MeasureString(s, Font)
                Return New Size(sz.Width, sz.Height)
            End SyncLock
        End Function

        <ExportAPI("GrayBitmap", Info:="Create the gray color of the target image.")>
        <Extension> Public Function CreateGrayBitmap(res As Image) As Image
            Dim Gr = DirectCast(res.Clone, Image).GdiFromImage
            Call System.Windows.Forms.ControlPaint.DrawImageDisabled(Gr.Graphics, res, 0, 0, Color.FromArgb(0, 0, 0, 0))
            Return Gr.ImageResource
        End Function

        ''' <summary>
        ''' Adding a frame box to the target image source.(为图像添加边框)
        ''' </summary>
        ''' <param name="Handle"></param>
        ''' <param name="pen">Default pen width is 1px and with color <see cref="Color.Black"/>.(默认的绘图笔为黑色的1个像素的边框)</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        <Extension> Public Function ImageAddFrame(Handle As GDIPlusDeviceHandle,
                                              Optional pen As Drawing.Pen = Nothing,
                                              Optional offset As Integer = 0) As GDIPlusDeviceHandle

            Dim TopLeft As New Point(offset, offset)
            Dim TopRight As New Point(Handle.Width - offset, 1 + offset)
            Dim BottomLeft As New Point(offset + 1, Handle.Height - offset)
            Dim BottomRight As New Point(Handle.Width - offset, Handle.Height - offset)

            If pen Is Nothing Then
                pen = Drawing.Pens.Black
            End If

            Call Handle.Graphics.DrawLine(pen, TopLeft, TopRight)
            Call Handle.Graphics.DrawLine(pen, TopRight, BottomRight)
            Call Handle.Graphics.DrawLine(pen, BottomRight, BottomLeft)
            Call Handle.Graphics.DrawLine(pen, BottomLeft, TopLeft)

            Call Handle.Graphics.FillRectangle(New SolidBrush(pen.Color), New Rectangle(New Point, New Size(1, 1)))

            Return Handle
        End Function

        ''' <summary>
        ''' 创建一个GDI+的绘图设备
        ''' </summary>
        ''' <param name="r"></param>
        ''' <param name="filled">默认的背景填充颜色为白色</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        '''
        <ExportAPI("GDI+.Create")>
        <Extension> Public Function CreateGDIDevice(r As Drawing.SizeF, Optional filled As Color = Nothing) As GDIPlusDeviceHandle
            Return (New Size(CInt(r.Width), CInt(r.Height))).CreateGDIDevice(filled)
        End Function

        <Extension> Public Function OpenDevice(ctrl As System.Windows.Forms.Control) As GDIPlusDeviceHandle
            Dim ImageRes As Image

            'If ctrl.BackgroundImage Is Nothing Then
            ImageRes = New Bitmap(ctrl.Width, ctrl.Height)
            'Else
            'ImageRes = ctrl.BackgroundImage
            'End If

            Dim Device = ImageRes.GdiFromImage

            If ctrl.BackgroundImage Is Nothing Then
                Call Device.Graphics.FillRectangle(Brushes.White, New Rectangle(New Point, ImageRes.Size))
            End If

            Return Device
        End Function

        ''' <summary>
        ''' 从指定的文件之中加载GDI+设备的句柄
        ''' </summary>
        ''' <param name="path"></param>
        ''' <returns></returns>
        '''
        <ExportAPI("GDI+.Create")>
        <Extension> Public Function GDIPlusDeviceHandleFromImageFile(path As String) As GDIPlusDeviceHandle
            Dim Image As Image = LoadImage(path)
            Dim GrDevice As Graphics = Graphics.FromImage(Image)
            GrDevice.CompositingQuality = Drawing2D.CompositingQuality.HighQuality
            GrDevice.TextRenderingHint = Drawing.Text.TextRenderingHint.ClearTypeGridFit
            Return GDIPlusDeviceHandle.CreateObject(GrDevice, Image)
        End Function

        ''' <summary>
        ''' 无需处理图像数据，这个函数已经自动克隆了该对象，不会影响到原来的对象
        ''' </summary>
        ''' <param name="res"></param>
        ''' <returns></returns>
        '''
        <ExportAPI("GDI+.Create")>
        <Extension> Public Function GdiFromImage(res As Image, <CallerMemberName> Optional caller As String = "") As GDIPlusDeviceHandle
            Try
                ' res = New Bitmap(DirectCast(res.Clone, Image))
            Catch ex As Exception
                ex = New Exception(res.Size.ToString, ex)
                ex = New Exception(caller, ex)
                Throw ex
            End Try
            Dim Gr As GDIPlusDeviceHandle = res.Size.CreateGDIDevice
            Call Gr.Graphics.DrawImage(res, 0, 0, Gr.Width, Gr.Height)
            Return Gr
        End Function

        <Extension> Public Function BackgroundGraphics(ctrl As Control) As GDIPlusDeviceHandle
            If Not ctrl.BackgroundImage Is Nothing Then
                Try
                    Return ctrl.BackgroundImage.GdiFromImage
                Catch ex As Exception
                    Call App.LogException(ex)
                    Return ctrl.Size.CreateGDIDevice(ctrl.BackColor)
                End Try
            Else
                Return ctrl.Size.CreateGDIDevice(ctrl.BackColor)
            End If
        End Function

        <ExportAPI("Offset")>
        <Extension> Public Function OffSet2D(p As Point, x As Integer, y As Integer) As Point
            Return New Point(x + p.X, y + p.Y)
        End Function

        <ExportAPI("Offset")>
        <Extension> Public Function OffSet2D(p As Point, offset As Point) As Point
            Return New Point(offset.X + p.X, offset.Y + p.Y)
        End Function

        <Extension> Public Function IsValidGDIParameter(size As Size) As Boolean
            Return size.Width > 0 AndAlso size.Height > 0
        End Function

        Const InvalidSize As String = "One of the size parameter for the gdi+ device is not valid!"

        ''' <summary>
        ''' 创建一个GDI+的绘图设备
        ''' </summary>
        ''' <param name="r"></param>
        ''' <param name="filled">默认的背景填充颜色为白色</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        '''
        <ExportAPI("GDI+.Create")>
        <Extension> Public Function CreateGDIDevice(r As Size,
                                                    Optional filled As Color = Nothing,
                                                    <CallerMemberName> Optional trace As String = "") As GDIPlusDeviceHandle
            Dim Bitmap As Bitmap

            If r.Width = 0 OrElse r.Height = 0 Then
                Throw New Exception(InvalidSize)
            End If

            Try
                Bitmap = New Bitmap(r.Width, r.Height)
            Catch ex As Exception
                ex = New Exception(r.ToString, ex)
                ex = New Exception(trace, ex)
                Call App.LogException(ex, MethodBase.GetCurrentMethod.GetFullName)
                Throw ex
            End Try

            Dim gdi As Graphics = Graphics.FromImage(Bitmap)
            Dim rect As New Rectangle(New Point, Bitmap.Size)

            If filled.IsNullOrEmpty Then
                filled = Color.White
            End If

            Call gdi.FillRectangle(New SolidBrush(filled), rect)

            gdi.InterpolationMode = Drawing2D.InterpolationMode.HighQualityBicubic
            gdi.PixelOffsetMode = Drawing2D.PixelOffsetMode.HighQuality
            gdi.CompositingQuality = Drawing2D.CompositingQuality.HighQuality
            gdi.SmoothingMode = Drawing2D.SmoothingMode.HighQuality

            Return GDIPlusDeviceHandle.CreateObject(gdi, Bitmap)
        End Function

        ''' <summary>
        ''' 图片剪裁小方块区域
        ''' </summary>
        ''' <param name="pos">左上角的坐标位置</param>
        ''' <param name="size">剪裁的区域的大小</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        '''
        <ExportAPI("Image.Corp")>
        <Extension> Public Function ImageCrop(source As Image, pos As Point, size As Size) As Image
            SyncLock source
                Dim CloneRect As New Rectangle(pos, size)
                Dim CloneBitmap As Bitmap = CType(source.Clone, Bitmap)
                Dim crop As Bitmap = CloneBitmap.Clone(CloneRect, source.PixelFormat)
                Return crop
            End SyncLock
        End Function

        <ExportAPI("Image.Resize")>
        Public Function Resize(Image As Image, newSize As Size) As Image
            SyncLock Image
                Dim Gr As GDIPlusDeviceHandle = newSize.CreateGDIDevice
                Call Gr.Graphics.DrawImage(Image, 0, 0, newSize.Width, newSize.Height)
                Return Gr.ImageResource
            End SyncLock
        End Function

        ''' <summary>
        ''' 图片剪裁为圆形的头像
        ''' </summary>
        ''' <param name="resAvatar">要求为正方形或者近似正方形</param>
        ''' <param name="OutSize"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        <Extension> Public Function TrimRoundAvatar(resAvatar As Image, OutSize As Integer) As Image
            If resAvatar Is Nothing Then
                Return Nothing
            End If

            SyncLock resAvatar
                Dim Bitmap As Bitmap = New Bitmap(OutSize, OutSize)

                resAvatar = DirectCast(resAvatar.Clone, Image)

                Using Gr = Graphics.FromImage(Bitmap)

                    Gr.CompositingQuality = Drawing2D.CompositingQuality.HighQuality
                    Gr.InterpolationMode = Drawing2D.InterpolationMode.HighQualityBicubic
                    Gr.SmoothingMode = Drawing2D.SmoothingMode.HighQuality
                    Gr.TextRenderingHint = Drawing.Text.TextRenderingHint.ClearTypeGridFit

                    resAvatar = Resize(resAvatar, Bitmap.Size)

                    Dim rH As Drawing.Brush = New Drawing.TextureBrush(resAvatar)
                    Call Gr.FillPie(rH, New Rectangle(0, 0, OutSize, OutSize), 0, 360)
                    Return Bitmap
                End Using
            End SyncLock
        End Function

        <Extension> Public Function Clone(res As Bitmap) As Bitmap
            If res Is Nothing Then Return Nothing
            Return DirectCast(res.Clone, Bitmap)
        End Function

        <Extension> Public Function Clone(res As Image) As Image
            If res Is Nothing Then Return Nothing
            Return DirectCast(res.Clone, Image)
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
            Dim Gr = Image.GdiFromImage
            Dim Alpha As Integer = 0
            Dim delta = (Math.PI / 2) / Math.Abs(y1 - y2)
            Dim offset As Double = 0

            If RenderColor = Nothing OrElse RenderColor.IsEmpty Then
                RenderColor = Color.White
            End If

            For y As Integer = y1 To y2
                Dim Color = System.Drawing.Color.FromArgb(Alpha, RenderColor.R, RenderColor.G, RenderColor.B)
                Call Gr.Graphics.DrawLine(New Pen(Color), New Point(0, y), New Point(Gr.Width, y))

                Alpha = CInt(255 * Math.Sin(offset) ^ 2)
                offset += delta
            Next

            Dim Rect = New Rectangle(New Point(0, y2), New Size(Image.Width, Image.Height - y2))
            Call Gr.Graphics.FillRectangle(New SolidBrush(RenderColor), Rect)

            Return Gr.ImageResource
        End Function

        ''' <summary>
        ''' 将图像的多余的空白处给剪裁掉，确定边界，然后进行剪裁
        ''' </summary>
        ''' <param name="res"></param>
        ''' <param name="margin"></param>
        ''' <param name="blankColor">默认白色为空白色</param>
        ''' <returns></returns>
        <ExportAPI("Image.CorpBlank")>
        <Extension> Public Function CorpBlank(res As Image, Optional margin As Integer = 0, Optional blankColor As Color = Nothing) As Image
            If blankColor.IsNullOrEmpty Then
                blankColor = Color.White
            ElseIf blankColor.Name = NameOf(Color.Transparent) Then
                ' 系统的transparent颜色为 0,255,255,255
                ' 但是bitmap之中的transparent为 0,0,0,0
                ' 在这里要变换一下
                blankColor = New Color
            End If

            Dim top As Integer
            Dim left As Integer
            Dim bmp As New Bitmap(res)

            ' top

            For top = 0 To res.Height - 1
                Dim find As Boolean = False

                For left = 0 To res.Width - 1
                    Dim p = bmp.GetPixel(left, top)
                    If Not GDIColors.Equals(p, blankColor) Then
                        ' 在这里确定了左右
                        find = True
                        Exit For
                    End If
                Next

                If find Then
                    Exit For
                End If
            Next

            Dim region As New Rectangle(0, top, res.Width, res.Height - top)
            res = res.ImageCrop(region.Location, region.Size)
            bmp = New Bitmap(res)

            ' left

            For left = 0 To res.Width - 1
                Dim find As Boolean = False

                For top = 0 To res.Height - 1
                    Dim p = bmp.GetPixel(left, top)
                    If Not GDIColors.Equals(p, blankColor) Then
                        ' 在这里确定了左右
                        find = True
                        Exit For
                    End If
                Next

                If find Then
                    Exit For
                End If
            Next

            region = New Rectangle(left, 0, res.Width - left, res.Height)
            res = res.ImageCrop(region.Location, region.Size)
            bmp = New Bitmap(res)

            Dim right As Integer
            Dim bottom As Integer

            ' bottom

            For bottom = res.Height - 1 To 0 Step -1
                Dim find As Boolean = False

                For right = res.Width - 1 To 0 Step -1
                    Dim p = bmp.GetPixel(right, bottom)
                    If Not GDIColors.Equals(p, blankColor) Then
                        ' 在这里确定了左右
                        find = True
                        Exit For
                    End If
                Next

                If find Then
                    Exit For
                End If
            Next

            region = New Rectangle(0, 0, res.Width, bottom)
            res = res.ImageCrop(region.Location, region.Size)
            bmp = New Bitmap(res)

            ' right

            For right = res.Width - 1 To 0 Step -1
                Dim find As Boolean = False

                For bottom = res.Height - 1 To 0 Step -1
                    Dim p = bmp.GetPixel(right, bottom)
                    If Not GDIColors.Equals(p, blankColor) Then
                        ' 在这里确定了左右
                        find = True
                        Exit For
                    End If
                Next

                If find Then
                    Exit For
                End If
            Next

            region = New Rectangle(0, 0, right, res.Height)
            res = res.ImageCrop(region.Location, region.Size)

            If margin > 0 Then
                Dim gr = New Size(res.Width + margin * 2, res.Height + margin * 2).CreateGDIDevice
                Call gr.Graphics.DrawImage(res, New Point(margin, margin))
                res = gr.ImageResource
            End If

            Return res
        End Function
    End Module
End Namespace
