#Region "Microsoft.VisualBasic::66063545d289f827f86f5b16255b203f, Microsoft.VisualBasic.Core\src\Extensions\Image\GDI+\Graphics2D.vb"

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

    '     Class Graphics2D
    ' 
    '         Properties: Center, Height, ImageResource, Size, Width
    ' 
    '         Constructor: (+4 Overloads) Sub New
    ' 
    '         Function: CreateDevice, CreateObject, Open, (+2 Overloads) Save, ToString
    ' 
    '         Sub: __save, DrawCircle
    '         Structure Context
    ' 
    '             Function: Create
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Drawing.Imaging
Imports System.Reflection
Imports Microsoft.VisualBasic.Language

Namespace Imaging

    ''' <summary>
    ''' GDI+ device handle for encapsulates a GDI+ drawing surface.
    ''' (GDI+绘图设备句柄，这个对象其实是为了将gdi+绘图与图形模块的SVG绘图操作统一起来的)
    ''' </summary>
    ''' <remarks></remarks>
    Public Class Graphics2D : Inherits GDICanvas
        Implements IDisposable

        ''' <summary>
        ''' GDI+ device handle memory.(GDI+设备之中的图像数据)
        ''' </summary>
        ''' <remarks></remarks>
        Public Property ImageResource As Image
            Get
                Return innerImage
            End Get
            Protected Friend Set(value As Image)
                innerImage = value

                If Not value Is Nothing Then
                    _Size = value.Size
                    _Center = New Point(Size.Width / 2, Size.Height / 2)
                Else
                    _Size = Nothing
                    _Center = Nothing
                End If
            End Set
        End Property

        Dim innerImage As Image

        Protected Sub New()
        End Sub

        Sub New(size As Size, fill As Color)
            Dim base = size.CreateGDIDevice(fill)

            Stroke = base.Stroke
            Font = base.Font
            ImageResource = base.ImageResource
            Graphics = base.Graphics
        End Sub

        Sub New(context As Context)
            Call Me.New(context.size, context.color.TranslateColor)
        End Sub

        Sub New(base As Image)
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
            Dim color$

            Public Function Create() As Graphics2D
                Return size.CreateGDIDevice(color.TranslateColor)
            End Function
        End Structure

        Public ReadOnly Property Width As Integer
            Get
                Return Size.Width
            End Get
        End Property

        Public ReadOnly Property Height As Integer
            Get
                Return Size.Height
            End Get
        End Property

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

        ''' <summary>
        ''' 将GDI+设备之中的图像数据保存到指定的文件路径之中，默认的图像文件的格式为PNG格式
        ''' </summary>
        ''' <param name="Path"></param>
        ''' <param name="Format">默认为png格式</param>
        ''' <returns></returns>
        Public Overloads Function Save(path$, Optional Format As ImageFormats = ImageFormats.Png) As Boolean
            Return Save(path, Format.GetFormat)
        End Function

        ''' <summary>
        ''' 将GDI+设备之中的图像数据保存到指定的文件路径之中，默认的图像文件的格式为PNG格式
        ''' </summary>
        ''' <param name="Path"></param>
        ''' <param name="Format">默认为png格式</param>
        ''' <returns></returns>
        Public Overloads Function Save(path$, Optional Format As ImageFormat = Nothing) As Boolean
            Try
                Call __save(path, Format Or Png)
            Catch ex As Exception
                Return App.LogException(ex, MethodBase.GetCurrentMethod.GetFullName)
            End Try

            Return True
        End Function

        Private Sub __save(path As String, format As ImageFormat)
            Call path.ParentPath.MkDIR
            Call ImageResource.Save(path, format)
        End Sub

        Public Overrides Function ToString() As String
            Return Size.ToString
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="r"></param>
        ''' <param name="filled">所填充的颜色</param>
        ''' <returns></returns>
        Public Shared Function CreateDevice(r As Size, Optional filled As Color = Nothing) As Graphics2D
            Return r.CreateGDIDevice(filled)
        End Function

        ''' <summary>
        ''' Get the internal <see cref="ImageResource"/>
        ''' </summary>
        ''' <param name="g2D"></param>
        ''' <returns></returns>
        Public Shared Narrowing Operator CType(g2D As Graphics2D) As Image
            Return g2D.ImageResource
        End Operator

        Public Shared Widening Operator CType(img As Image) As Graphics2D
            Dim g As Graphics = Graphics.FromImage(img)
            Return CreateObject(g, res:=img)
        End Operator

        Public Shared Widening Operator CType(img As Bitmap) As Graphics2D
            Dim g As Graphics = Graphics.FromImage(img)
            Return CreateObject(g, img)
        End Operator

        ''' <summary>
        ''' Internal create gdi device helper.(这个函数不会克隆原来的图像对象<paramref name="res"/>)
        ''' </summary>
        ''' <param name="g"></param>
        ''' <param name="res">绘图的基础图像对象</param>
        ''' <returns></returns>
        Friend Shared Function CreateObject(g As Graphics, res As Image) As Graphics2D
            With New Graphics2D With {
                .ImageResource = res,
                .g = g,
                .Font = New Font(FontFace.MicrosoftYaHei, 12),
                .Stroke = Pens.Black,
                .InterpolationMode = Drawing2D.InterpolationMode.HighQualityBicubic,
                .PixelOffsetMode = Drawing2D.PixelOffsetMode.HighQuality,
                .CompositingQuality = Drawing2D.CompositingQuality.HighQuality,
                .SmoothingMode = Drawing2D.SmoothingMode.HighQuality,
                .TextRenderingHint = Drawing.Text.TextRenderingHint.SingleBitPerPixelGridFit
            }
                ' .Clear(Color.Transparent)
                Return .ByRef
            End With
        End Function

        ''' <summary>
        ''' Creates a new <see cref="System.Drawing.Graphics"/> from the specified <see cref="Image"/>.
        ''' </summary>
        ''' <param name="image">
        ''' <see cref="Image"/> from which to create the new System.Drawing.Graphics.
        ''' </param>
        ''' <returns>
        ''' This method returns a new <see cref="System.Drawing.Graphics"/> for the specified <see cref="Image"/>.
        ''' </returns>
        Public Shared Function Open(image As Image) As Graphics2D
            Dim g As Graphics = Graphics.FromImage(image)
            Return Graphics2D.CreateObject(g, image)
        End Function

        Public Overrides Sub DrawCircle(center As PointF, fill As Color, stroke As Pen, radius As Single)
            Call Me.DrawCircle(center, radius, New SolidBrush(fill))
            Call Me.DrawCircle(center, radius, stroke, fill:=False)
        End Sub
    End Class
End Namespace
