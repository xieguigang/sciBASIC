#Region "Microsoft.VisualBasic::5a60bcc8648e8bf46dece4035ae11c98, Microsoft.VisualBasic.Core\src\Drawing\GDI+\GraphicsExtensions.vb"

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


    ' Code Statistics:

    '   Total Lines: 543
    '    Code Lines: 345 (63.54%)
    ' Comment Lines: 133 (24.49%)
    '    - Xml Docs: 96.24%
    ' 
    '   Blank Lines: 65 (11.97%)
    '     File Size: 20.53 KB


    '     Module GraphicsExtensions
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: (+2 Overloads) Clone, ColorBrush, (+2 Overloads) ColorReplace, EntireImage, GetBrush
    '                   GetBrushes, GetStreamBuffer, (+2 Overloads) GraphicsPath, ImageAddFrame, IsValidGDIParameter
    '                   (+3 Overloads) LoadImage, (+2 Overloads) Opacity, ParseImageFormat, (+2 Overloads) PointF, SaveAs
    '                   SizeF, ToFloat, ToPoint, ToPoints, ToStream
    '                   (+2 Overloads) X, (+2 Overloads) Y
    ' 
    '         Sub: (+4 Overloads) DrawCircle, FillPolygon
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Drawing.Imaging
Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Runtime.InteropServices
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.Algorithm.base
Imports Microsoft.VisualBasic.Imaging.BitmapImage
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Net.Http
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.Text

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

        ReadOnly enumFormats As New Dictionary(Of String, ImageFormats)

        Sub New()
            For Each flag As ImageFormats In [Enums](Of ImageFormats)()
                enumFormats(flag.ToString.ToLower) = flag
                enumFormats(flag.Description.ToLower) = flag
            Next
        End Sub

        ''' <summary>
        ''' 不存在的名称会返回<see cref="ImageFormats.Png"/>类型
        ''' </summary>
        ''' <param name="format$">大小写不敏感</param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function ParseImageFormat(format$) As ImageFormats
            Return enumFormats.TryGetValue(LCase(format), [default]:=ImageFormats.Png)
        End Function

#If NET8_0_OR_GREATER Then
        ''' <summary>
        ''' Saves this <see cref="Image"/> to the specified file in the specified format.
        ''' (这个函数可以很容易的将图像对象保存为tiff文件)
        ''' </summary>
        ''' <param name="res">
        ''' The image resource data that will be saved to the disk.
        ''' (因为这个函数可能会被<see cref="GdiRasterGraphics.ImageResource"/>所调用，
        ''' 由于该属性的Set方法是不公开可见的，所以将会不兼容这个方法，如果这个
        ''' 参数被设置为ByRef的话)
        ''' </param>
        ''' <param name="path">path string</param>
        ''' <param name="format">Image formats enumeration.</param>
        ''' <returns></returns>
        <Extension>
        Public Function SaveAs(res As Image,
                           path$,
                           Optional format As ImageFormats = ImageFormats.Png,
                           Optional autoDispose As Boolean = False) As Boolean

            If res Is Nothing Then
                Return False
            End If

            Try
                Call path.ParentPath.MakeDir

                If format = ImageFormats.Tiff Then
                    Throw New NotImplementedException
                ElseIf format = ImageFormats.Base64 Then
                    Return res _
                        .ToBase64String _
                        .SaveTo(path, Encodings.ASCII.CodePage)
                Else
                    Using s As Stream = path.Open(FileMode.OpenOrCreate, doClear:=True, [readOnly]:=False)
                        Call res.Save(s, format)
                    End Using
                End If
            Catch ex As Exception
                ex = New Exception(path, ex)
                Call App.LogException(ex)
                Call ex.PrintException
                Return False
            Finally
                If autoDispose Then
                    Call res.Dispose()
                    Call GC.SuppressFinalize(res)
                    Call GC.Collect()
                End If
            End Try

            Return True
        End Function
#End If

        <Extension>
        Public Sub FillPolygon(g As IGraphics, color As Brush, x As Double(), y As Double())
            If x.TryCount <> y.TryCount Then
                Throw New SafeArrayRankMismatchException("dimension of axis x and axis y is mis-matched!")
            Else
                Call g.FillPolygon(color, x.Select(Function(xi, i) New PointF(xi, y(i))).ToArray)
            End If
        End Sub

        ''' <summary>
        ''' Make batch convert of <see cref="Point"/> to <see cref="System.Drawing.PointF"/> object
        ''' </summary>
        ''' <param name="polygon"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function PointF(polygon As IEnumerable(Of Point)) As IEnumerable(Of PointF)
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
        <Extension>
        Public Function ToPoints(ps As IEnumerable(Of PointF)) As Point()
            Return ps.Select(Function(x) New Point(x.X, x.Y)).ToArray
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
        Public Sub DrawCircle(ByRef g As IGraphics, centra As PointF, r!, color As SolidBrush)
            Dim d = r * 2

            With centra
                Call g.FillPie(color, .X - r, .Y - r, d, d, 0, 360)
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
        Public Sub DrawCircle(ByRef g As IGraphics, color As Pen, x!, y!, r!, Optional fill As Boolean = True)
            Call g.DrawCircle(New PointF(x, y), r, color, fill)
        End Sub

        ''' <summary>
        ''' a helper method for fill pie
        ''' </summary>
        ''' <param name="g"></param>
        ''' <param name="centra"></param>
        ''' <param name="r!"></param>
        ''' <param name="color"></param>
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
            Return New Rectangle(New Point, size:=img.Size)
        End Function

        ''' <summary>
        ''' get x axis of the input geometry data
        ''' </summary>
        ''' <param name="pts"></param>
        ''' <returns></returns>
        <Extension>
        Public Function X(pts As IEnumerable(Of Point)) As Integer()
            Return (From pt As Point In pts Select xi = pt.X).ToArray
        End Function

        ''' <summary>
        ''' get y axis of the input geometry data
        ''' </summary>
        ''' <param name="pts"></param>
        ''' <returns></returns>
        <Extension>
        Public Function Y(pts As IEnumerable(Of Point)) As Integer()
            Return (From pt As Point In pts Select yi = pt.Y).ToArray
        End Function

        <Extension>
        Public Function X(Of Point As Layout2D)(pts As IEnumerable(Of Point)) As Double()
            Return (From pt As Point In pts Select xi = pt.X).ToArray
        End Function

        <Extension>
        Public Function Y(Of Point As Layout2D)(pts As IEnumerable(Of Point)) As Double()
            Return (From pt As Point In pts Select yi = pt.Y).ToArray
        End Function

        ''' <summary>
        ''' Load image from a file and then close the file handle.
        ''' (使用<see cref="Image.FromStream(Stream)"/>函数在加载完成图像到Dispose这段之间内都不会释放文件句柄，
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
                Dim base64String = path.SolveStream
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
                        Return App.LogException(New Exception(path, ex))
                    End If
                End Try
            End If
        End Function

        ''' <summary>
        ''' cast the memory stream buffer as the gdi+ image
        ''' </summary>
        ''' <param name="rawStream"></param>
        ''' <param name="throwEx"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function LoadImage(rawStream As Byte(), Optional throwEx As Boolean = True) As Image
            Try
                Return Image.FromStream(New MemoryStream(rawStream))
            Catch ex As Exception
                If throwEx Then
                    Throw
                Else
                    Return App.LogException(ex)
                End If
            End Try
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function LoadImage(stream As Stream, Optional throwEx As Boolean = True) As Image
            Try
                Return Image.FromStream(stream)
            Catch ex As Exception
                If throwEx Then
                    Throw
                Else
                    Return App.LogException(ex)
                End If
            End Try
        End Function

        ''' <summary>
        ''' 将图片对象转换为原始的字节流
        ''' </summary>
        ''' <param name="image"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <ExportAPI("Get.RawStream")>
        <Extension>
        Public Function GetStreamBuffer(image As Image) As Byte()
            Return image.ToStream.ToArray
        End Function

        <Extension>
        Public Function ToStream(image As Image) As MemoryStream
            With New MemoryStream
#If NET48 Then
                Call image.Save(.ByRef, ImageFormat.Png)
#Else
                Call image.Save(.ByRef, ImageFormats.Png)
#End If

                Return .ByRef
            End With
        End Function

        ''' <summary>
        ''' Color replace using memory pointer
        ''' </summary>
        ''' <param name="image"></param>
        ''' <param name="subject"></param>
        ''' <param name="replaceAs"></param>
        ''' <returns></returns>
        <Extension>
        Public Function ColorReplace(image As Bitmap, subject As Color, replaceAs As Color, Optional tolerance% = 3) As Bitmap
            Using bitmap As BitmapBuffer = BitmapBuffer.FromBitmap(image)
                Dim byts As BitmapBuffer = bitmap

                For x As Integer = 0 To byts.Width - 1
                    For y As Integer = 0 To byts.Height - 1
                        If GDIColors.Equals(byts.GetPixel(x, y), subject, tolerance) Then
                            Call byts.SetPixel(x, y, replaceAs)
                        End If
                    Next
                Next
            End Using

            Return image
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function ColorReplace(image As Image, subject As Color, replaceAs As Color, Optional tolerance% = 3) As Bitmap
            Return New Bitmap(image).ColorReplace(subject, replaceAs, tolerance)
        End Function

        ''' <summary>
        ''' Adding a frame box to the target image source.(为图像添加边框)
        ''' </summary>
        ''' <param name="canvas"></param>
        ''' <param name="pen">Default pen width is 1px and with color <see cref="Color.Black"/>.(默认的绘图笔为黑色的1个像素的边框)</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        <Extension>
        Public Function ImageAddFrame(canvas As IGraphics, Optional pen As Pen = Nothing, Optional offset% = 0) As IGraphics
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
        ''' both width and height in current size object must be greater than zero
        ''' </summary>
        ''' <param name="size"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function IsValidGDIParameter(size As Size) As Boolean
            Return size.Width > 0 AndAlso size.Height > 0
        End Function

        <Extension>
        Public Function Clone(res As Bitmap) As Bitmap
            If res Is Nothing Then Return Nothing
            Return DirectCast(res.Clone, Bitmap)
        End Function

        <Extension>
        Public Function Clone(res As Image) As Image
            If res Is Nothing Then Return Nothing
            Return DirectCast(res.Clone, Image)
        End Function
    End Module
End Namespace
