#Region "Microsoft.VisualBasic::8db3579c9eae6938f825568adfadabe0, mime\text%html\CSS\CSSEnvirnment.vb"

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

    '   Total Lines: 235
    '    Code Lines: 147 (62.55%)
    ' Comment Lines: 54 (22.98%)
    '    - Xml Docs: 96.30%
    ' 
    '   Blank Lines: 34 (14.47%)
    '     File Size: 8.43 KB


    '     Class CSSEnvirnment
    ' 
    '         Properties: baseFont, baseLine, canvas, dpi
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: Empty, GetDashStyle, (+2 Overloads) GetFont, GetFontByScale, GetFontFamily
    '                   GetFontStyle, GetHeight, GetLineWidth, (+2 Overloads) GetPen, GetSize
    '                   GetValue, GetWidth, SetBaseStyles
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.MIME.Html.Render.CSS

#If NET48 Then
Imports Font = System.Drawing.Font
Imports Pen = System.Drawing.Pen
Imports DashStyle = System.Drawing.Drawing2D.DashStyle
Imports FontStyle = System.Drawing.FontStyle
#Else
Imports Font = Microsoft.VisualBasic.Imaging.Font
Imports Pen = Microsoft.VisualBasic.Imaging.Pen
Imports DashStyle = Microsoft.VisualBasic.Imaging.DashStyle
Imports FontStyle = Microsoft.VisualBasic.Imaging.FontStyle
#End If

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
        Public ReadOnly Property dpi As Single = 100

        ''' <summary>
        ''' the canvas size [width,height].
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property canvas As Size

        Sub New(canvas As Size, Optional dpi As Single = 100)
            Me.canvas = canvas
            Me.dpi = dpi
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Sub New(width As Integer, height As Integer, Optional dpi As Single = 100)
            Call Me.New(New Size(width, height), dpi)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Sub New(canvas As SizeF, Optional dpi As Single = 100)
            Call Me.New(canvas.ToSize, dpi)
        End Sub

        ''' <summary>
        ''' set css base styles
        ''' </summary>
        ''' <param name="font"></param>
        ''' <param name="stroke"></param>
        ''' <returns></returns>
        Public Function SetBaseStyles(Optional font As Font = Nothing, Optional stroke As Pen = Nothing) As CSSEnvirnment
            _baseFont = font
            _baseLine = stroke

            Return Me
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetSize(size As CSSsize) As SizeF
            Return New SizeF(GetWidth(size.width), GetHeight(size.height))
        End Function

        Public Function GetValue(size As CssLength, base As Single) As Single
            If size.IsPercentage Then
                Return size.Number * base
            End If

            Select Case size.Unit
                Case CssUnit.None, CssUnit.Pixels, CssUnit.Points
                    Return size.Number

                Case Else
                    Throw New NotImplementedException(size.ToString)
            End Select
        End Function

        ''' <summary>
        ''' get css length value, usually be padding/margin width calculation
        ''' </summary>
        ''' <param name="len"></param>
        ''' <returns></returns>
        Public Function GetWidth(len As String) As Single
            Return GetValue(New CssLength(len), canvas.Width)
        End Function

        ''' <summary>
        ''' get css length value, usually be padding/margin width calculation
        ''' </summary>
        ''' <param name="len"></param>
        ''' <returns></returns>
        Public Function GetHeight(len As String) As Single
            Return GetValue(New CssLength(len), canvas.Height)
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="stroke"></param>
        ''' <param name="allowNull">
        ''' unlike the function <see cref="GetPen(Stroke)"/> may returns the value of <see cref="baseLine"/> 
        ''' if the given stroke value is nothing, this function will returns nothing directly if 
        ''' this parameter value set to TRUE.
        ''' </param>
        ''' <returns></returns>
        Public Function GetPen(stroke As Stroke, allowNull As Boolean) As Pen
            If allowNull Then
                If stroke Is Nothing Then
                    Return Nothing
                Else
                    Return GetPen(stroke)
                End If
            Else
                Return GetPen(stroke)
            End If
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="stroke"></param>
        ''' <returns>
        ''' this function may handling of the <paramref name="stroke"/> null value 
        ''' as the default <see cref="baseLine"/> style.
        ''' </returns>
        Public Function GetPen(stroke As Stroke) As Pen
            If stroke Is Nothing Then
                Return baseLine
            End If

            Dim style As DashStyle = GetDashStyle(stroke)
            Dim width As Single = GetLineWidth(stroke) * dpi / 96

            Return New Pen(stroke.fill.GetBrush, width) With {
                .DashStyle = style
            }
        End Function

        Public Function GetLineWidth(stroke As Stroke) As Single
            Dim size As New CssLength(stroke.width)
            Dim width As Single

            Select Case size.Unit
                Case CssUnit.Ems : width = baseLine.Width * size.Number
                Case CssUnit.None, CssUnit.Pixels, CssUnit.Points : width = size.Number
                Case Else
                    Throw New NotImplementedException(stroke.width)
            End Select

            Return width
        End Function

        Public Function GetDashStyle(css As Stroke) As DashStyle
            If css Is Nothing AndAlso baseLine Is Nothing Then
                Return Nothing
            ElseIf baseLine Is Nothing Then
                Return css.dash
            ElseIf css Is Nothing Then
                Return baseLine.DashStyle
            Else
                Return css.dash
            End If
        End Function

        Public Function GetFontByScale(em As Single) As Font
            Dim newSize As Single = em * baseFont.Size
            Dim newFont As New Font(baseFont.Name, newSize, baseFont.Style)

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
        Public Function GetFont(css As CSSFont, Optional scale As Single = 1.0) As Font
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
            size_val = size_val * scale

            Return New Font(familyName, size_val, style)
        End Function

        Public Shared Function Empty(Optional ppi As Integer = 100) As CSSEnvirnment
            Return New CSSEnvirnment(Nothing, ppi)
        End Function
    End Class
End Namespace
