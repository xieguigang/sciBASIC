#Region "Microsoft.VisualBasic::640ac23b9e04bf13472a1ffb38ae143c, sciBASIC#\gr\Microsoft.VisualBasic.Imaging\Drivers\MockGDIPlusGraphics.vb"

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

    '   Total Lines: 85
    '    Code Lines: 65
    ' Comment Lines: 3
    '   Blank Lines: 17
    '     File Size: 3.94 KB


    '     Class MockGDIPlusGraphics
    ' 
    '         Properties: DpiX, DpiY, Size
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: FontMetrics, FontScale, MeasureCharacterRanges, (+7 Overloads) MeasureString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Text

Namespace Driver

    Public MustInherit Class MockGDIPlusGraphics : Inherits IGraphics

        ''' <summary>
        ''' 主要是需要进行字体的大小计算所需要使用的一个内部gdi+对象
        ''' </summary>
        ReadOnly gdi As Graphics = Graphics.FromImage(New Bitmap(10, 10))

        Public Overrides ReadOnly Property Size As Size
        Public Overrides ReadOnly Property DpiX As Single
        Public Overrides ReadOnly Property DpiY As Single

        Sub New(size As Size, dpi As Size)
            Me.DpiX = dpi.Width
            Me.DpiY = dpi.Height
            Me.Size = size
        End Sub

        Sub New(size As Size, dpiX As Single, dpiY As Single)
            Me.DpiX = dpiX
            Me.DpiY = dpiY
            Me.Size = size
        End Sub

        Private Shared Function FontScale(font As Font) As Font
            Return New Font(font, font.Size * 2)
        End Function

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
            Return gdi.MeasureString(text, FontScale(font))
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function MeasureString(text As String, font As Font, width As Integer) As SizeF
            Return gdi.MeasureString(text, FontScale(font), width)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function MeasureString(text As String, font As Font, layoutArea As SizeF) As SizeF
            Return gdi.MeasureString(text, FontScale(font), layoutArea)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function MeasureString(text As String, font As Font, width As Integer, format As StringFormat) As SizeF
            Return gdi.MeasureString(text, FontScale(font), width, format)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function MeasureString(text As String, font As Font, origin As PointF, stringFormat As StringFormat) As SizeF
            Return gdi.MeasureString(text, FontScale(font), origin, stringFormat)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function MeasureString(text As String, font As Font, layoutArea As SizeF, stringFormat As StringFormat) As SizeF
            Return gdi.MeasureString(text, FontScale(font), layoutArea, stringFormat)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function MeasureString(text As String,
                                                font As Font,
                                                layoutArea As SizeF,
                                                stringFormat As StringFormat,
                                                ByRef charactersFitted As Integer,
                                                ByRef linesFilled As Integer) As SizeF

            Return gdi.MeasureString(text, FontScale(font), layoutArea, stringFormat, charactersFitted, linesFilled)
        End Function
    End Class
End Namespace
