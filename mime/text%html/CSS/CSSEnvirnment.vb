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
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.MIME.Html.Render.CSS

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
        Public ReadOnly Property dpi As Integer = 100

        ''' <summary>
        ''' the canvas size [width,height].
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property canvas As Size

        Sub New(canvas As Size, Optional dpi As Integer = 100)
            Me.canvas = canvas
            Me.dpi = dpi
        End Sub

        Public Function SetBaseStyles(Optional font As Font = Nothing, Optional stroke As Pen = Nothing) As CSSEnvirnment
            _baseFont = font
            _baseLine = stroke

            Return Me
        End Function

        Public Function GetFontByScale(em As Single) As Font
            Dim newSize As Single = em * baseFont.Size
            Dim newFont As New Font(baseFont.FontFamily, newSize, baseFont.Style)

            Return newFont
        End Function

        Public Function GetFontFamily(css As CSSFont) As String
            If css Is Nothing AndAlso baseFont Is Nothing Then
                Return Nothing
            ElseIf baseFont Is Nothing Then
                Return css.family
            ElseIf css Is Nothing OrElse css.family.StringEmpty Then
                Return baseFont.Name
            Else
                Return css.family
            End If
        End Function

        Public Function GetFontStyle(css As CSSFont) As FontStyle
            If css Is Nothing AndAlso baseFont Is Nothing Then
                Return FontStyle.Regular
            ElseIf baseFont Is Nothing Then
                Return css.style
            ElseIf css Is Nothing Then
                Return baseFont.Style
            Else
                Return css.style
            End If
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetFont(css As String) As Font
            Return GetFont(css:=CSSFont.TryParse(css))
        End Function

        ''' <summary>
        ''' Initializes a new <see cref="Font"/> using a specified size and style.
        ''' </summary>
        ''' <returns></returns>
        Public Function GetFont(css As CSSFont) As Font
            Dim size As New CssLength(css.size)
            Dim size_val As Single
            Dim familyName As String = GetFontFamily(css)
            Dim style As FontStyle = GetFontStyle(css)

            Select Case size.Unit
                Case CssUnit.Ems : size_val = size.Number * baseFont.Size
                Case CssUnit.Pixels, CssUnit.Points, CssUnit.None : size_val = size.Number
                Case Else
                    Throw New NotImplementedException($"the given css length({size.ToString}) has not been implemented!")
            End Select

            size_val = FontFace.PointSizeScale(size_val, dpiResolution:=dpi)

            Return New Font(familyName, size_val, style)
        End Function

        Public Shared Function Empty(Optional ppi As Integer = 100) As CSSEnvirnment
            Return New CSSEnvirnment(Nothing, ppi)
        End Function
    End Class
End Namespace
