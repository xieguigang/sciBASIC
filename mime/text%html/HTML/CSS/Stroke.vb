#Region "Microsoft.VisualBasic::918581ba7af4cb0c93b00a9869477a71, ..\sciBASIC#\mime\MIME_Markups\HTML\CSS\Stroke.vb"

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
Imports System.Drawing.Drawing2D
Imports Microsoft.VisualBasic.Imaging

Namespace HTML.CSS

    ''' <summary>
    ''' ```css
    ''' stroke: color/image; stroke-width: width(px); stroke-dash: dash_style;
    ''' ```
    ''' </summary>
    Public Class Stroke : Inherits ICSSValue

        Public Const AxisStroke$ = "stroke: black; stroke-width: 2px; stroke-dash: solid;"
        Public Const AxisGridStroke$ = "stroke: lightgray; stroke-width: 2px; stroke-dash: dash;"

        Public Property fill As String
        Public Property width As Single
        Public Property dash As DashStyle

        Public ReadOnly Property GDIObject As Pen
            Get
                Return New Pen(fill.GetBrush, width) With {
                    .DashStyle = dash
                }
            End Get
        End Property

        Public Overrides ReadOnly Property CSSValue As String
            Get
                Return ToString()
            End Get
        End Property

        Sub New()
        End Sub

        Sub New(width!)
            Me.width = width
            fill = "black"
            dash = DashStyle.Solid
        End Sub

        Sub New(style As Pen)
            width = style.Width
            fill = style.Color.RGB2Hexadecimal
            dash = style.DashStyle
        End Sub

        Public Overrides Function ToString() As String
            Return $"stroke: {fill}; stroke-width: {width}px; stroke-dash: {dash};"
        End Function

        Shared ReadOnly __dashStyles As Dictionary(Of String, DashStyle) =
            Enums(Of DashStyle) _
            .ToDictionary(Function(t) LCase(t.ToString))

        Public Shared Function GetDashStyle(css$) As DashStyle
            If Not css.StringEmpty AndAlso __dashStyles.ContainsKey(css) Then
                Return __dashStyles(css)
            Else
                css = LCase(css)
                If __dashStyles.ContainsKey(css) Then
                    Return __dashStyles(css)
                Else
                    Return DashStyle.Solid
                End If
            End If
        End Function

        Public Shared Function TryParse(css$, Optional [default] As Stroke = Nothing) As Stroke
            Dim t As Dictionary(Of String, String) = css _
                .Trim(";"c) _
                .Split(";"c) _
                .Select(AddressOf Trim) _
                .Select(Function(s) s.GetTagValue(":", trim:=True)) _
                .ToDictionary(Function(x) x.Name,
                              Function(x) x.Value)

            Dim st As New Stroke With {
                .dash = GetDashStyle(t.TryGetValue("stroke-dash")),
                .fill = t.TryGetValue("stroke"),
                .width = Val(t.TryGetValue("stroke-width"))
            }

            If st.fill.StringEmpty Then
                st.fill = "black"
            End If

            Return st
        End Function

        Public Shared Narrowing Operator CType(stroke As Stroke) As Pen
            Return stroke.GDIObject
        End Operator

        Public Shared Widening Operator CType(css$) As Stroke
            Return TryParse(css)
        End Operator
    End Class
End Namespace
