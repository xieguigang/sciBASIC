#Region "Microsoft.VisualBasic::ad184ecd54193d91a6987291c62d39b9, Microsoft.VisualBasic.Core\src\Extensions\Image\GDI+\GraphicsExtensions.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
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



    ' /********************************************************************************/

    ' Summaries:

    '     Module GraphicsExtensions
    ' 
    '         Function: CanvasCreateFromImageFile, (+2 Overloads) Clone, ColorBrush, CreateCanvas2D, (+4 Overloads) CreateGDIDevice
    '                   CreateObject, EntireImage, GetBrush, GetBrushes, (+2 Overloads) GetIcon
    '                   GetStreamBuffer, GetStringPath, (+2 Overloads) GraphicsPath, ImageAddFrame, IsValidGDIParameter
    '                   (+3 Overloads) LoadImage, (+2 Overloads) Opacity, (+2 Overloads) PointF, PointSizeScale, SaveIcon
    '                   SizeF, ToFloat, ToPoint, ToPoints, ToStream
    ' 
    '         Sub: (+5 Overloads) DrawCircle
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.ComponentModel
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Drawing.Imaging
Imports System.Drawing.Text
Imports System.IO
Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.Algorithm.base
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Net.Http
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.Scripting.Runtime

Namespace Imaging

    ''' <summary>
    ''' GDI+
    ''' </summary>
    '''
    <Package("GDI+", Description:="GDI+ GDIPlus Extensions Module to provide some useful interface.",
                  Publisher:="xie.guigang@gmail.com",
                  Revision:=58,
                  Url:="http://gcmodeller.org")>
    <HideModuleName>
    Public Module GraphicsExtensions

        ''' <summary>
        ''' Internal create gdi device helper.(这个函数不会克隆原来的图像对象<paramref name="res"/>)
        ''' </summary>
        ''' <param name="g"></param>
        ''' <param name="res">绘图的基础图像对象</param>
        ''' <returns></returns>
        Friend Function CreateObject(g As Graphics, res As Image) As Graphics2D
            g.InterpolationMode = InterpolationMode.HighQualityBicubic
            g.PixelOffsetMode = PixelOffsetMode.HighQuality
            g.CompositingQuality = CompositingQuality.HighQuality
            g.SmoothingMode = SmoothingMode.HighQuality
            g.TextRenderingHint = TextRenderingHint.SingleBitPerPixelGridFit

            With New Graphics2D With {
                .ImageResource = res,
                .g = g,
                .Font = New Font(FontFace.MicrosoftYaHei, 12),
                .Stroke = Pens.Black
            }
                ' .Clear(Color.Transparent)
                Return .ByRef
            End With
        End Function

        ''' <summary>
        ''' fix for dpi bugs on unix mono platform when create a font object.
        ''' 
        ''' https://github.com/dotnet/runtime/issues/28361
        ''' </summary>
        ''' <param name="pointSize"></param>
        ''' <param name="dpiResolution"></param>
        ''' <returns></returns>
        Public Function PointSizeScale(pointSize As Single, dpiResolution As Single) As Single
#If netcore5 = 1 Then
            Return pointSize
#Else
            ' fix for running on unix mono 
            Return If(App.IsMicrosoftPlatform, pointSize, pointSize * dpiResolution / 96)
#End If
        End Function

        <Extension>
        Public Function GetStringPath(s$, dpi!, rect As RectangleF, font As Font, format As StringFormat) As GraphicsPath
            Dim path As New GraphicsPath()
            ' Convert font size into appropriate coordinates
            Dim emSize! = dpi * font.SizeInPoints / 72
            path.AddString(s, font.FontFamily, font.Style, emSize, rect, format)
            Return path
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension> Public Function PointF(polygon As IEnumerable(Of Point)) As IEnumerable(Of PointF)
            Return polygon.Select(Function(pt) New PointF(pt.X, pt.Y))
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function SizeF(size As Size) As SizeF
            Return New SizeF(size.Width, size.Height)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension> Public Function ToPoint(pf As PointF) As Point
            Return New Point(pf.X, pf.Y)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension> Public Function ToPoints(ps As IEnumerable(Of PointF)) As Point()
            Return ps.Select(Function(x) New Point(x.X, x.Y)).ToArray
        End Function

        <Extension> Public Function SaveIcon(ico As Icon, path$) As Boolean
            Call path.ParentPath.MakeDir

            Try
                Using file As New FileStream(path, FileMode.OpenOrCreate)
                    Call ico.Save(file)
                    Call file.Flush()
                End Using

                Return True
            Catch ex As Exception
                Call App.LogException(New Exception(path, ex))
            End Try

            Return False
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function ToFloat(rect As Rectangle) As RectangleF
            Return New RectangleF With {
                .Location = rect.Location.PointF,
                .Size = rect.Size.SizeF
            }
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function PointF(pf As Point) As PointF
            Return New PointF(pf.X, pf.Y)
        End Function

        <Extension>
        Public Function GraphicsPath(points As IEnumerable(Of Point)) As GraphicsPath
            Dim path As New GraphicsPath

            For Each pt In points.SlideWindows(2)
                Call path.AddLine(pt(0), pt(1))
            Next

            Return path
        End Function

        <Extension>
        Public Function GraphicsPath(points As IEnumerable(Of PointF)) As GraphicsPath
            Dim path As New GraphicsPath

            For Each pt In points.SlideWindows(2)
                Call path.AddLine(pt(0), pt(1))
            Next

            Return path
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="fill"></param>
        ''' <param name="val">a value in range ``[0, 1]``</param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function Opacity(fill As Color, val#) As Color
            Return Color.FromArgb(val * 255, baseColor:=fill)
        End Function

        ''' <summary>
        ''' adjust the color opacity value of the <see cref="SolidBrush"/>
        ''' </summary>
        ''' <param name="fill"></param>
        ''' <param name="val">
        ''' the alpha value for <see cref="Opacity"/>, value in range ``[0, 1]``.
        ''' </param>
        ''' <returns></returns>
        <Extension>
        Public Function Opacity(fill As Brush, val#) As Brush
            If TypeOf fill Is SolidBrush Then
                Dim color As Color = DirectCast(fill, SolidBrush).Color

                color = color.Opacity(val)
                fill = New SolidBrush(color)

                Return fill
            Else
                Return fill
            End If
        End Function

        ''' <summary>
        ''' 同时兼容颜色以及图片纹理画刷的创建
        ''' </summary>
        ''' <param name="res$"></param>
        ''' <returns></returns>
        <Extension>
        Public Function GetBrush(res As String) As Brush
            Dim bgColor As Color = res.TranslateColor(throwEx:=False)

            If Not bgColor.IsEmpty Then
                Return New SolidBrush(bgColor)
            End If

            Dim img As Image

            If res.FileExists Then
                img = LoadImage(path:=res$)
            Else
                img = Base64Codec.GetImage(res$)
            End If

            If img Is Nothing Then
                Throw New InvalidCastException($"unable to cast expression '{res}' to any brush object!")
            Else
                Return New TextureBrush(img)
            End If
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function ColorBrush(c As Color) As SolidBrush
            Return New SolidBrush(color:=c)
        End Function

        ''' <summary>
        ''' Converts the colors into solidbrushes in batch.
        ''' </summary>
        ''' <param name="colors"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function GetBrushes(colors As IEnumerable(Of Color)) As SolidBrush()
            Return colors _
                .SafeQuery _
                .Select(Function(c) New SolidBrush(c)) _
                .ToArray
        End Function

        <Extension>
        Public Sub DrawCircle(ByRef g As Graphics, centra As PointF, r!, color As SolidBrush)
            Dim d = r * 2

            With centra
                Call g.FillPie(color, .X - r, .Y - r, d, d, 0, 360)
            End With
        End Sub

        <Extension>
        Public Sub DrawCircle(ByRef g As Graphics, centra As PointF, r!, color As Pen, Optional fill As Boolean = True)
            With centra
                Dim d! = r * 2
                Dim rect As New Rectangle(.X - r, .Y - r, d, d)

                If fill Then
                    Call g.FillPie(New SolidBrush(color.Color), rect, 0, 360)
                Else
                    Call g.DrawEllipse(color, rect)
                End If
            End With
        End Sub

        ''' <summary>
        ''' 模仿Java之中的``DrawCircle``方法
        ''' </summary>
        ''' <param name="g"></param>
        ''' <param name="color"></param>
        ''' <param name="x!"></param>
        ''' <param name="y!"></param>
        ''' <param name="r!"></param>
        ''' <param name="fill"></param>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Sub DrawCircle(ByRef g As Graphics, color As Pen, x!, y!, r!, Optional fill As Boolean = True)
            Call g.DrawCircle(New PointF(x, y), r, color, fill)
        End Sub

        <Extension>
        Public Sub DrawCircle(ByRef g As IGraphics, centra As PointF, r!, color As Brush)
            Dim d = r * 2

            With centra
                Call g.FillPie(color, .X - r, .Y - r, d, d, 0, 360)
            End With
        End Sub

        ''' <summary>
        ''' 进行圆的绘制
        ''' </summary>
        ''' <param name="g"></param>
        ''' <param name="centra">圆心的坐标，这个函数之中会自动转换为<see cref="Rectangle"/>的左上角位置坐标</param>
        ''' <param name="r!">圆的半径</param>
        ''' <param name="color">线条的颜色</param>
        ''' <param name="fill">是否进行填充？</param>
        <Extension>
        Public Sub DrawCircle(ByRef g As IGraphics, centra As PointF, r!, color As Pen, Optional fill As Boolean = True)
            Dim d = r * 2

            With centra
                If fill Then
                    Call g.FillPie(New SolidBrush(color.Color), .X - r, .Y - r, d, d, 0, 360)
                Else
                    Call g.DrawEllipse(color, .X - r, .Y - r, d, d)
                End If
            End With
        End Sub

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

        ''' <summary>
        ''' Convert image to icon
        ''' </summary>
        ''' <param name="res"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <ExportAPI("To.Icon")>
        <Extension> Public Function GetIcon(res As Image) As Icon
            Return Icon.FromHandle(New Bitmap(res).GetHicon)
        End Function

        ''' <summary>
        ''' Convert image to icon
        ''' </summary>
        ''' <param name="res"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <ExportAPI("To.Icon")>
        <Extension> Public Function GetIcon(res As Bitmap) As Icon
            Return Icon.FromHandle(res.GetHicon)
        End Function

        ''' <summary>
        ''' Load image from a file and then close the file handle.
        ''' (使用<see cref="Image.FromFile(String)"/>函数在加载完成图像到Dispose这段之间内都不会释放文件句柄，
        ''' 则使用这个函数则没有这个问题，在图片加载之后会立即释放掉文件句柄)
        ''' </summary>
        ''' <param name="path"></param>
        ''' <returns>
        ''' 当参数<paramref name="throwEx"/>为false时候，函数返回空值的话，说明图片文件错误
        ''' 例如文件未下载完成或者发生了二进制移码
        ''' </returns>
        <ExportAPI("LoadImage"), Extension>
        Public Function LoadImage(path$,
                                  Optional base64 As Boolean = False,
                                  Optional throwEx As Boolean = True) As Image
            If base64 Then
                Dim base64String = path.ReadAllText
                Dim img As Image = base64String.GetImage
                Return img
            Else
                Try
                    Return FileIO.FileSystem _
                        .ReadAllBytes(path) _
                        .LoadImage
                Catch ex As Exception
                    If throwEx Then
                        Throw New Exception(path, ex)
                    Else
                        Call App.LogException(New Exception(path, ex))
                        Return Nothing
                    End If
                End Try
            End If
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <ExportAPI("LoadImage")>
        <Extension> Public Function LoadImage(rawStream As Byte()) As Image
            Return Image.FromStream(stream:=New MemoryStream(rawStream))
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function LoadImage(stream As Stream) As Image
            Return Image.FromStream(stream)
        End Function

        ''' <summary>
        ''' 将图片对象转换为原始的字节流
        ''' </summary>
        ''' <param name="image"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <ExportAPI("Get.RawStream")>
        <Extension> Public Function GetStreamBuffer(image As Image) As Byte()
            Return image.ToStream.ToArray
        End Function

        Public Function ToStream(image As Image) As MemoryStream
            With New MemoryStream
                Call image.Save(.ByRef, ImageFormat.Png)
                Return .ByRef
            End With
        End Function

        '<ExportAPI("GrayBitmap")>
        '<Description("Create the gray color of the target image.")>
        '<Extension> Public Function CreateGrayBitmap(res As Image) As Image
        '    Using g As Graphics2D = DirectCast(res.Clone, Image).CreateCanvas2D
        '        With g
        '            Call ControlPaint.DrawImageDisabled(.Graphics, res, 0, 0, Color.FromArgb(0, 0, 0, 0))
        '            Return .ImageResource
        '        End With
        '    End Using
        'End Function

        ''' <summary>
        ''' Adding a frame box to the target image source.(为图像添加边框)
        ''' </summary>
        ''' <param name="canvas"></param>
        ''' <param name="pen">Default pen width is 1px and with color <see cref="Color.Black"/>.(默认的绘图笔为黑色的1个像素的边框)</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        <Extension> Public Function ImageAddFrame(canvas As Graphics2D, Optional pen As Pen = Nothing, Optional offset% = 0) As Graphics2D
            Dim TopLeft As New Point(offset, offset)
            Dim TopRight As New Point(canvas.Width - offset, 1 + offset)
            Dim BtmLeft As New Point(offset + 1, canvas.Height - offset)
            Dim BtmRight As New Point(canvas.Width - offset, canvas.Height - offset)

            If pen Is Nothing Then
                pen = Pens.Black
            End If

            Call canvas.DrawLine(pen, TopLeft, TopRight)
            Call canvas.DrawLine(pen, TopRight, BtmRight)
            Call canvas.DrawLine(pen, BtmRight, BtmLeft)
            Call canvas.DrawLine(pen, BtmLeft, TopLeft)

            Dim color As New SolidBrush(pen.Color)
            Dim region As New Rectangle With {
                .Size = New Size(1, 1)
            }

            Call canvas.FillRectangle(color, region)

            Return canvas
        End Function

        ''' <summary>
        ''' 创建一个GDI+的绘图设备
        ''' </summary>
        ''' <param name="r"></param>
        ''' <param name="filled">默认的背景填充颜色为白色</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <ExportAPI("GDI+.Create")>
        <Extension>
        Public Function CreateGDIDevice(r As SizeF, Optional filled As Color = Nothing) As Graphics2D
            Return (New Size(CInt(r.Width), CInt(r.Height))).CreateGDIDevice(filled)
        End Function

        '<Extension>
        'Public Function OpenDevice(ctrl As Control) As Graphics2D
        '    Dim img As Image = New Bitmap(ctrl.Width, ctrl.Height)
        '    Dim canvas = img.CreateCanvas2D

        '    If ctrl.BackgroundImage Is Nothing Then
        '        Call canvas.FillRectangle(Brushes.White, New Rectangle(New Point, img.Size))
        '    End If

        '    Return canvas
        'End Function

        ''' <summary>
        ''' 从指定的文件之中加载GDI+设备的句柄
        ''' </summary>
        ''' <param name="path"></param>
        ''' <returns></returns>
        '''
        <ExportAPI("GDI+.Create")>
        <Extension> Public Function CanvasCreateFromImageFile(path As String) As Graphics2D
            Dim image As Image = LoadImage(path)
            Dim g As Graphics = Graphics.FromImage(image)

            With g
                .CompositingQuality = CompositingQuality.HighQuality
                .TextRenderingHint = TextRenderingHint.ClearTypeGridFit
            End With

            Return Graphics2D.CreateObject(g, image)
        End Function

        ''' <summary>
        ''' 无需处理图像数据，这个函数默认已经自动克隆了该对象，不会影响到原来的对象，
        ''' 除非你将<paramref name="directAccess"/>参数设置为真，函数才不会自动克隆图像对象
        ''' </summary>
        ''' <param name="res"></param>
        ''' <returns></returns>
        <ExportAPI("GDI+.Create")>
        <Extension> Public Function CreateCanvas2D(res As Image,
                                                   Optional directAccess As Boolean = False,
                                <CallerMemberName> Optional caller$ = "") As Graphics2D

            If directAccess Then
                Return Graphics2D.CreateObject(Graphics.FromImage(res), res)
            Else
                With res.Size.CreateGDIDevice
                    Call .DrawImage(res, 0, 0, .Width, .Height)
                    Return .ByRef
                End With
            End If
        End Function

        '<Extension> Public Function BackgroundGraphics(ctrl As Control) As Graphics2D
        '    If Not ctrl.BackgroundImage Is Nothing Then
        '        Try
        '            Return ctrl.BackgroundImage.CreateCanvas2D
        '        Catch ex As Exception
        '            Call App.LogException(ex)
        '            Return ctrl.Size.CreateGDIDevice(ctrl.BackColor)
        '        End Try
        '    Else
        '        Return ctrl.Size.CreateGDIDevice(ctrl.BackColor)
        '    End If
        'End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension> Public Function IsValidGDIParameter(size As Size) As Boolean
            Return size.Width > 0 AndAlso size.Height > 0
        End Function

        Const InvalidSize As String = "One of the size parameter for the gdi+ device is not valid!"

#If NET_48 = 1 Or netcore5 = 1 Then

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function CreateGDIDevice(t As (width%, height%),
                                        Optional filled As Color = Nothing,
                                        <CallerMemberName>
                                        Optional trace$ = "",
                                        Optional dpi$ = "100,100") As Graphics2D

            Return CreateGDIDevice(t.width, t.height, filled:=filled, dpi:=dpi, trace:=trace)
        End Function

#End If

        ''' <summary>
        ''' 创建一个GDI+的绘图设备，默认的背景填充色为白色
        ''' </summary>
        ''' <param name="r"></param>
        ''' <param name="filled">默认的背景填充颜色为白色</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <ExportAPI("GDI+.Create")>
        <Extension> Public Function CreateGDIDevice(r As Size,
                                                    Optional filled As Color = Nothing,
                                                    <CallerMemberName>
                                                    Optional trace$ = "",
                                                    Optional dpi$ = "100,100") As Graphics2D
            Return CreateGDIDevice(r.Width, r.Height, filled:=filled, dpi:=dpi, trace:=trace)
        End Function

        Public Function CreateGDIDevice(width%, height%,
                                        Optional filled As Color = Nothing,
                                        <CallerMemberName>
                                        Optional trace$ = "",
                                        Optional dpi$ = "100,100") As Graphics2D
            Dim bitmap As Bitmap

            If width = 0 OrElse height = 0 Then
                Throw New Exception(InvalidSize)
            End If

            Try
                bitmap = New Bitmap(width, height)

                With dpi.SizeParser
                    Call bitmap.SetResolution(.Width, .Height)
                End With

                ' Call $"Bitmap size: [{bitmap.Width}, {bitmap.Height}]".__DEBUG_ECHO
                ' Call $"Bitmap dpi: [{bitmap.HorizontalResolution}, {bitmap.VerticalResolution}]".__DEBUG_ECHO
            Catch ex As Exception
                ex = New Exception(New Size(width, height).ToString, ex)
                ex = New Exception(trace, ex)
                Call App.LogException(ex, MethodBase.GetCurrentMethod.GetFullName)
                Throw ex
            End Try

            Dim g As Graphics = Graphics.FromImage(bitmap)
            Dim rect As New Rectangle(New Point, bitmap.Size)

            If filled.IsNullOrEmpty Then
                filled = Color.White
            End If

            Call g.Clear(filled)

            g.InterpolationMode = InterpolationMode.HighQualityBicubic
            g.PixelOffsetMode = PixelOffsetMode.HighQuality
            g.CompositingQuality = CompositingQuality.HighQuality
            g.SmoothingMode = SmoothingMode.HighQuality

            Return Graphics2D.CreateObject(g, bitmap)
        End Function

        <Extension> Public Function Clone(res As Bitmap) As Bitmap
            If res Is Nothing Then Return Nothing
            Return DirectCast(res.Clone, Bitmap)
        End Function

        <Extension> Public Function Clone(res As Image) As Image
            If res Is Nothing Then Return Nothing
            Return DirectCast(res.Clone, Image)
        End Function
    End Module
End Namespace
