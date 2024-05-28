#Region "Microsoft.VisualBasic::bd9eda487d04cb49669f88a6ec5004bf, mime\text%html\CSS\Stroke.vb"

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

    '   Total Lines: 184
    '    Code Lines: 110 (59.78%)
    ' Comment Lines: 53 (28.80%)
    '    - Xml Docs: 98.11%
    ' 
    '   Blank Lines: 21 (11.41%)
    '     File Size: 6.43 KB


    '     Class Stroke
    ' 
    '         Properties: CSSValue, dash, fill, GDIObject, width
    ' 
    '         Constructor: (+5 Overloads) Sub New
    '         Function: GetDashStyle, ParserImpl, ToString, TryParse
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Linq

Namespace CSS

    ''' <summary>
    ''' ```css
    ''' stroke: color/image; stroke-width: width(px); stroke-dash: dash_style;
    ''' ```
    ''' </summary>
    Public Class Stroke : Inherits ICSSValue

        Public Const AxisStroke$ = "stroke: black; stroke-width: 5px; stroke-dash: solid;"
        Public Const AxisGridStroke$ = "stroke: lightgray; stroke-width: 2px; stroke-dash: dash;"
        Public Const HighlightStroke$ = "stroke: gray; stroke-width: 5px; stroke-dash: dash;"
        Public Const StrongHighlightStroke$ = "stroke: black; stroke-width: 2px; stroke-dash: dash;"
        Public Const ScatterLineStroke$ = "stroke: black; stroke-width: 2px; stroke-dash: solid;"
        Public Const WhiteLineStroke$ = "stroke: white; stroke-width: 2px; stroke-dash: solid;"

        ''' <summary>
        ''' the line fill color brush description
        ''' </summary>
        ''' <returns></returns>
        Public Property fill As String
        ''' <summary>
        ''' the line drawing width
        ''' </summary>
        ''' <returns></returns>
        Public Property width As Single
        ''' <summary>
        ''' the line drawing style
        ''' </summary>
        ''' <returns></returns>
        Public Property dash As DashStyle

        Public ReadOnly Property GDIObject As Pen
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return New Pen(fill.GetBrush, width) With {
                    .DashStyle = dash
                }
            End Get
        End Property

        Public Overrides ReadOnly Property CSSValue As String
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return ToString()
            End Get
        End Property

        Sub New()
        End Sub

        ''' <summary>
        ''' create a default style css stroke object with 
        ''' specific line width is given.
        ''' </summary>
        ''' <param name="width"></param>
        Sub New(width!)
            Me.width = width
            fill = "black"
            dash = DashStyle.Solid
        End Sub

        Sub New(width As Double)
            Call Me.New(CSng(width))
        End Sub

        ''' <summary>
        ''' create css stroke object based on a specific given 
        ''' gdi+ object
        ''' </summary>
        ''' <param name="style"></param>
        Sub New(style As Pen)
            width = style.Width
            fill = style.Color.ToHtmlColor
            dash = style.DashStyle
        End Sub

        ''' <summary>
        ''' do value copy
        ''' </summary>
        ''' <param name="clone"></param>
        Sub New(clone As Stroke)
            Me.width = clone.width
            Me.fill = clone.fill
            Me.dash = clone.dash
        End Sub

        ''' <summary>
        ''' 生成CSS字符串值
        ''' </summary>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function ToString() As String
            Return $"stroke: {fill}; stroke-width: {width}px; stroke-dash: {dash.ToString.ToLower};"
        End Function

        Shared ReadOnly __dashStyles As Dictionary(Of String, DashStyle) =
            Enums(Of DashStyle) _
            .ToDictionary(Function(t) LCase(t.ToString))

        ''' <summary>
        ''' parse the <see cref="DashStyle"/> enum value from a given term string
        ''' </summary>
        ''' <param name="css">
        ''' should be one of the member name in enum value: <see cref="DashStyle"/>
        ''' </param>
        ''' <returns></returns>
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
            If css.StringEmpty Then
                Return [default]
            Else
                Return css.DoCall(AddressOf ParserImpl)
            End If
        End Function

        ''' <summary>
        ''' parse from a given css string
        ''' </summary>
        ''' <param name="css"></param>
        ''' <returns></returns>
        Private Shared Function ParserImpl(css As String) As Stroke
            Dim t As Dictionary(Of String, String) = css _
                .Trim(";"c) _
                .Split(";"c) _
                .Select(AddressOf Trim) _
                .Select(Function(s) s.GetTagValue(":", trim:=True)) _
                .ToDictionary(Function(x) x.Name,
                              Function(x)
                                  Return x.Value
                              End Function)

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

        ''' <summary>
        ''' 在进行隐式转换的时候，空值的话转换函数会返回空值
        ''' </summary>
        ''' <param name="stroke"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Narrowing Operator CType(stroke As Stroke) As Pen
            If stroke Is Nothing Then
                Return Nothing
            Else
                Return stroke.GDIObject
            End If
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Widening Operator CType(css$) As Stroke
            Return TryParse(css)
        End Operator
    End Class
End Namespace
