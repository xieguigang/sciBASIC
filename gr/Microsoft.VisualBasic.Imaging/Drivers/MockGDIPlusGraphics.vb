#Region "Microsoft.VisualBasic::7a3a1e79ea425c36cbd7c94998c3d40f, gr\Microsoft.VisualBasic.Imaging\Drivers\MockGDIPlusGraphics.vb"

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

    '   Total Lines: 209
    '    Code Lines: 158 (75.60%)
    ' Comment Lines: 18 (8.61%)
    '    - Xml Docs: 33.33%
    ' 
    '   Blank Lines: 33 (15.79%)
    '     File Size: 8.41 KB


    '     Class MockGDIPlusGraphics
    ' 
    '         Properties: CompositingMode, CompositingQuality, DpiX, DpiY, InterpolationMode
    '                     IsClipEmpty, IsVisibleClipEmpty, PageScale, PageUnit, PixelOffsetMode
    '                     RenderingOrigin, Size, SmoothingMode, TextContrast, TextRenderingHint
    ' 
    '         Constructor: (+3 Overloads) Sub New
    '         Function: FontMetrics, MeasureCharacterRanges, (+7 Overloads) MeasureString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Drawing.Text
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Text

Namespace Driver

    ''' <summary>
    ''' A fake gdi+ graphics object that used for vector image rendering
    ''' </summary>
    Public MustInherit Class MockGDIPlusGraphics : Inherits IGraphics

        ''' <summary>
        ''' 主要是需要进行字体的大小计算所需要使用的一个内部gdi+对象
        ''' </summary>
        Friend ReadOnly gdi As Graphics

        Public Overrides ReadOnly Property Size As Size
        Public Overrides ReadOnly Property DpiX As Single
        Public Overrides ReadOnly Property DpiY As Single

        Public Overrides Property InterpolationMode As InterpolationMode
            Get
                Return gdi.InterpolationMode
            End Get
            Set(value As InterpolationMode)
                gdi.InterpolationMode = value
            End Set
        End Property

        Public Overrides Property CompositingQuality As CompositingQuality
            Get
                Return gdi.CompositingQuality
            End Get
            Set(value As CompositingQuality)
                gdi.CompositingQuality = value
            End Set
        End Property

        Public Overrides Property CompositingMode As CompositingMode
            Get
                Return gdi.CompositingMode
            End Get
            Set(value As CompositingMode)
                gdi.CompositingMode = value
            End Set
        End Property

        Public Overrides ReadOnly Property IsClipEmpty As Boolean
            Get
                Return gdi.IsClipEmpty
            End Get
        End Property

        Public Overrides ReadOnly Property IsVisibleClipEmpty As Boolean
            Get
                Return gdi.IsVisibleClipEmpty
            End Get
        End Property

        Public Overrides Property PageScale As Single
            Get
                Return gdi.PageScale
            End Get
            Set(value As Single)
                gdi.PageScale = value
            End Set
        End Property

        Public Overrides Property PageUnit As GraphicsUnit
            Get
                Return gdi.PageUnit
            End Get
            Set(value As GraphicsUnit)
                gdi.PageUnit = value
            End Set
        End Property


        Public Overrides Property PixelOffsetMode As PixelOffsetMode
            Get
                Return gdi.PixelOffsetMode
            End Get
            Set(value As PixelOffsetMode)
                gdi.PixelOffsetMode = value
            End Set
        End Property

        Public Overrides Property RenderingOrigin As Point
            Get
                Return gdi.RenderingOrigin
            End Get
            Set(value As Point)
                gdi.RenderingOrigin = value
            End Set
        End Property

        Public Overrides Property SmoothingMode As SmoothingMode
            Get
                Return gdi.SmoothingMode
            End Get
            Set(value As SmoothingMode)
                gdi.SmoothingMode = value
            End Set
        End Property

        Public Overrides Property TextContrast As Integer
            Get
                Return gdi.TextContrast
            End Get
            Set(value As Integer)
                gdi.TextContrast = value
            End Set
        End Property

        Public Overrides Property TextRenderingHint As TextRenderingHint
            Get
                Return gdi.TextRenderingHint
            End Get
            Set(value As TextRenderingHint)
                gdi.TextRenderingHint = value
            End Set
        End Property

        Sub New(dpiX As Integer, dpiY As Integer)
            Dim null As New Bitmap(10, 10)

            Me.DpiX = dpiX
            Me.DpiY = dpiY
            ' null.SetResolution(dpiX, dpiY)
            gdi = Graphics.FromImage(null)
        End Sub

        Sub New(size As Size, dpi As Size)
            Call Me.New(dpi.Width, dpi.Height)

            Me.Size = size
        End Sub

        Sub New(size As Size, dpiX As Single, dpiY As Single)
            Call Me.New(dpiX, dpiY)

            Me.Size = size
        End Sub

        '<MethodImpl(MethodImplOptions.AggressiveInlining)>
        'Protected Overridable Function FontScale(font As Font) As Font
        '    Return New Font(font, FontFace.SVGPointSize(font.Size, Dpi))
        'End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function FontMetrics(font As Font) As FontMetrics
            Return New FontMetrics(font, gdi)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function MeasureCharacterRanges(text As String, font As Font, layoutRect As RectangleF, stringFormat As StringFormat) As Region()
            Return gdi.MeasureCharacterRanges(text, font, layoutRect, stringFormat)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function MeasureString(text As String, font As Font) As SizeF
            'Return FontFace.SVGPointSize(gdi.MeasureString(text, FontScale(font)), Dpi)
            Return FontFace.SVGPointSize(gdi.MeasureString(text, font), Dpi)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function MeasureString(text As String, font As Font, width As Integer) As SizeF
            'Return FontFace.SVGPointSize(gdi.MeasureString(text, FontScale(font), width), Dpi)
            Return FontFace.SVGPointSize(gdi.MeasureString(text, font, width), Dpi)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function MeasureString(text As String, font As Font, layoutArea As SizeF) As SizeF
            'Return FontFace.SVGPointSize(gdi.MeasureString(text, FontScale(font), layoutArea), Dpi)
            Return FontFace.SVGPointSize(gdi.MeasureString(text, font, layoutArea), Dpi)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function MeasureString(text As String, font As Font, width As Integer, format As StringFormat) As SizeF
            'Return FontFace.SVGPointSize(gdi.MeasureString(text, FontScale(font), width, format), Dpi)
            Return FontFace.SVGPointSize(gdi.MeasureString(text, font, width, format), Dpi)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function MeasureString(text As String, font As Font, origin As PointF, stringFormat As StringFormat) As SizeF
            'Return FontFace.SVGPointSize(gdi.MeasureString(text, FontScale(font), origin, stringFormat), Dpi)
            Return FontFace.SVGPointSize(gdi.MeasureString(text, font, origin, stringFormat), Dpi)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function MeasureString(text As String, font As Font, layoutArea As SizeF, stringFormat As StringFormat) As SizeF
            'Return FontFace.SVGPointSize(gdi.MeasureString(text, FontScale(font), layoutArea, stringFormat), Dpi)
            Return FontFace.SVGPointSize(gdi.MeasureString(text, font, layoutArea, stringFormat), Dpi)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function MeasureString(text As String,
                                                font As Font,
                                                layoutArea As SizeF,
                                                stringFormat As StringFormat,
                                                ByRef charactersFitted As Integer,
                                                ByRef linesFilled As Integer) As SizeF
            'Return FontFace.SVGPointSize(gdi.MeasureString(text, FontScale(font), layoutArea, stringFormat, charactersFitted, linesFilled), Dpi)
            Return FontFace.SVGPointSize(gdi.MeasureString(text, font, layoutArea, stringFormat, charactersFitted, linesFilled), Dpi)
        End Function
    End Class
End Namespace
