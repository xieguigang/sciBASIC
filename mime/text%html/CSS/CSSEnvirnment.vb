#Region "Microsoft.VisualBasic::cf613f4b927640496e090bcedde9a1e9, mime\text%html\CSS\CSSEnvirnment.vb"

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

    '   Total Lines: 23
    '    Code Lines: 16 (69.57%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 7 (30.43%)
    '     File Size: 606 B


    '     Class CSSEnvirnment
    ' 
    '         Properties: baseFont, baseLine
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: GetFontByScale
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging

Namespace CSS

    Public Class CSSEnvirnment

        ''' <summary>
        ''' the base font style of the canvas
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property baseFont As Font
        ''' <summary>
        ''' the base stroke line style of the canvas
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property baseLine As Pen

        ''' <summary>
        ''' bugs fixed for config dpi value on unix mono platform 
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property dpi As Integer

        Sub New(basefont As Font, baseline As Pen)
            Me.baseFont = basefont
            Me.baseLine = baseline
        End Sub

        Public Function GetFontByScale(em As Single) As Font
            Dim newSize As Single = em * baseFont.Size
            Dim newFont As New Font(baseFont.FontFamily, newSize, baseFont.Style)

            Return newFont
        End Function

        ''' <summary>
        ''' Initializes a new <see cref="Font"/> using a specified size and style.
        ''' </summary>
        ''' <returns></returns>
        Public Function GetFont(css As CSSFont) As Font
            Dim size As Single

            If css.size.IsSimpleNumber Then
                size = Val(css.size)
            Else

            End If

            Return New Font(css.family, FontFace.PointSizeScale(size, dpiResolution:=dpi), css.style)
        End Function
    End Class
End Namespace
