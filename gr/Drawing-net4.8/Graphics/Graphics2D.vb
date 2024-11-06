#Region "Microsoft.VisualBasic::5cbcb0f263106a23a5107a20d9f501f5, gr\Drawing-net4.8\Graphics\Graphics2D.vb"

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

'   Total Lines: 214
'    Code Lines: 129 (60.28%)
' Comment Lines: 54 (25.23%)
'    - Xml Docs: 92.59%
' 
'   Blank Lines: 31 (14.49%)
'     File Size: 7.37 KB


' Class Graphics2D
' 
'     Properties: Center, Driver, ImageResource, Size
' 
'     Constructor: (+5 Overloads) Sub New
' 
'     Function: CreateDevice, CreateObject, Open, (+2 Overloads) Save, ToString
' 
'     Sub: DrawCircle, DrawString, saveFile
'     Structure Context
' 
'         Function: Create
' 
' 
' 
' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.IO
Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.BitmapImage
Imports Microsoft.VisualBasic.Imaging.Driver
Imports std = System.Math

#If NET8_0_OR_GREATER Then
Imports Font = Microsoft.VisualBasic.Imaging.Font
Imports Brush = Microsoft.VisualBasic.Imaging.Brush
Imports Pen = Microsoft.VisualBasic.Imaging.Pen
Imports Image = Microsoft.VisualBasic.Imaging.Image
Imports Bitmap = Microsoft.VisualBasic.Imaging.Bitmap
Imports GraphicsPath = Microsoft.VisualBasic.Imaging.GraphicsPath
Imports SolidBrush = Microsoft.VisualBasic.Imaging.SolidBrush
#End If

''' <summary>
''' GDI+ device handle for encapsulates a GDI+ drawing surface.
''' </summary>
''' <remarks>
''' (GDI+绘图设备句柄，这个对象其实是为了将gdi+绘图与图形模块的SVG绘图操作统一起来的)
''' </remarks>
Public Class Graphics2D : Inherits GDICanvas
    Implements IDisposable
    Implements SaveGdiBitmap
    Implements GdiRasterGraphics

    ''' <summary>
    ''' GDI+ device handle memory.
    ''' </summary>
    ''' <remarks>(GDI+设备之中的图像数据)</remarks>
#If NET48 Then
    Public Property ImageResource As System.Drawing.Image Implements GdiRasterGraphics.ImageResource
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Get
            Return innerImage
        End Get
        Protected Friend Set(value As System.Drawing.Image)
            innerImage = value
            innerSet()
        End Set
    End Property
#Else
    Public Property ImageResource As Image Implements GdiRasterGraphics.ImageResource
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Get
            Throw New NotImplementedException
        End Get
        Protected Friend Set(value As Image)
            innerImage = value.CTypeImage
            innerSet()
        End Set
    End Property
#End If

    Dim innerImage As System.Drawing.Image

    Public Overrides ReadOnly Property Driver As Drivers
        Get
            Return Drivers.GDI
        End Get
    End Property

    Protected Friend Sub New()
    End Sub

    Sub New(size As Size, fill As Color)
        Dim base = size.CreateGDIDevice(fill)

        Stroke = base.Stroke
        Font = base.Font
        ImageResource = base.ImageResource
        Graphics = base.Graphics
    End Sub

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Sub New(context As Context)
        Call Me.New(context.size, context.color.TranslateColor)
    End Sub

    Sub New(canvas As Graphics, size As Size)
        Me.Graphics = canvas
        Me.Size = size
    End Sub

    Sub New(base As System.Drawing.Image)
        innerImage = base
        Size = base.Size
        Center = New Point(Size.Width / 2, Size.Height / 2)
        Graphics = Graphics.FromImage(base)
    End Sub

    ''' <summary>
    ''' Can be serialize as a XML file node.
    ''' </summary>
    Public Structure Context

        Dim size As Size
        ''' <summary>
        ''' the background color value
        ''' </summary>
        Dim color$

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Create() As Graphics2D
            Return size.CreateGDIDevice(color.TranslateColor)
        End Function
    End Structure

    ''' <summary>
    ''' Gets the width and height, in pixels, of this <see cref="ImageResource"/>.(图像的大小)
    ''' </summary>
    ''' <returns>A <see cref="System.Drawing.Size"/> structure that represents the width and height, in pixels,
    ''' of this image.</returns>
    Public Overrides ReadOnly Property Size As Size

    ''' <summary>
    ''' 在图象上面的中心的位置点
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property Center As Point

    Private Sub innerSet()
        If Not innerImage Is Nothing Then
            _Size = innerImage.Size
            _Center = New Point(Size.Width / 2, Size.Height / 2)
        Else
            _Size = Nothing
            _Center = Nothing
        End If
    End Sub

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function GetImageResource() As System.Drawing.Image
        Return innerImage
    End Function

    ''' <summary>
    ''' 将GDI+设备之中的图像数据保存到指定的文件路径之中，默认的图像文件的格式为PNG格式
    ''' </summary>
    ''' <param name="Path"></param>
    ''' <param name="Format">默认为png格式</param>
    ''' <returns></returns>
    Public Overloads Function Save(path$, Optional Format As ImageFormats = ImageFormats.Png) As Boolean
        Try
            Call saveFile(path, Format)
        Catch ex As Exception
            Return App.LogException(ex, MethodBase.GetCurrentMethod.GetFullName)
        End Try

        Return True
    End Function

    Private Sub saveFile(path As String, format As ImageFormats)
        Call Save(path.Open(FileMode.OpenOrCreate, doClear:=True), format)
    End Sub

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Overrides Function ToString() As String
        Return Size.ToString
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="r"></param>
    ''' <param name="filled">所填充的颜色</param>
    ''' <returns></returns>
    ''' 
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Shared Function CreateDevice(r As Size, Optional filled As Color = Nothing) As Graphics2D
        Return r.CreateGDIDevice(filled)
    End Function

    ''' <summary>
    ''' Get the internal <see cref="ImageResource"/>
    ''' </summary>
    ''' <param name="g2D"></param>
    ''' <returns></returns>
    ''' 
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Shared Narrowing Operator CType(g2D As Graphics2D) As System.Drawing.Image
        Return g2D.innerImage
    End Operator

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Shared Widening Operator CType(img As System.Drawing.Image) As Graphics2D
        Return CreateObject(Graphics.FromImage(img), res:=img)
    End Operator

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Shared Widening Operator CType(img As System.Drawing.Bitmap) As Graphics2D
        Return CreateObject(Graphics.FromImage(img), img)
    End Operator

    ''' <summary>
    ''' Creates a new <see cref="System.Drawing.Graphics"/> from the specified <see cref="Image"/>.
    ''' </summary>
    ''' <param name="image">
    ''' <see cref="Image"/> from which to create the new System.Drawing.Graphics.
    ''' </param>
    ''' <returns>
    ''' This method returns a new <see cref="System.Drawing.Graphics"/> for the specified <see cref="Image"/>.
    ''' </returns>
    ''' 
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Shared Function Open(image As System.Drawing.Image) As Graphics2D
        Return Graphics2D.CreateObject(Graphics.FromImage(image), image)
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Friend Shared Function CreateObject(g As Graphics, res As System.Drawing.Image) As Graphics2D
        Return Extensions.CreateObject(g, res)
    End Function

    Public Overrides Sub DrawCircle(center As PointF, fill As Color, stroke As Pen, radius As Single)
        Call Me.DrawCircle(center, radius, New SolidBrush(fill))
        Call Me.DrawCircle(center, radius, stroke, fill:=False)
    End Sub

    Public Overloads Function Save(stream As Stream, format As ImageFormats) As Boolean Implements SaveGdiBitmap.Save
        Call innerImage.Save(stream, format.GetFormat)
        Call stream.Flush()

        Return True
    End Function

    Public Overrides Sub DrawString(s As String, font As Font, brush As Brush, ByRef x As Single, ByRef y As Single, angle As Single)
        Dim sfont As System.Drawing.Font = Nothing
        Dim sbrush As System.Drawing.Brush = Nothing

#If NET8_0_OR_GREATER Then
        sfont = font.CTypeFontObject
        sbrush = brush.CTypeBrushObject
#Else
        sfont = font
        sbrush = brush
#End If

        If angle <> 0 Then
            Dim text As New GraphicsText(g)
            Dim sz As SizeF = g.MeasureString(s, sfont)

            If angle > 0 Then
                Call text.DrawString(s, sfont, sbrush, New Point(x, y), angle:=angle)
            Else
                Call text.DrawString(s, sfont, sbrush, New Point(x, y + sz.Height * std.Sin(angle * 180 / std.PI)), angle:=angle)
            End If
        Else
            Call Graphics.DrawString(s, sfont, sbrush, x, y)
        End If
    End Sub
End Class
