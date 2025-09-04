#Region "Microsoft.VisualBasic::6dd3c56751187e45fd6f6fee086b6f65, mime\text%html\CSS\Elements\Stroke.vb"

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

    '   Total Lines: 201
    '    Code Lines: 117 (58.21%)
    ' Comment Lines: 61 (30.35%)
    '    - Xml Docs: 98.36%
    ' 
    '   Blank Lines: 23 (11.44%)
    '     File Size: 6.87 KB


    '     Class Stroke
    ' 
    '         Properties: CSSValue, (+2 Overloads) dash, fill, width
    ' 
    '         Constructor: (+8 Overloads) Sub New
    '         Function: GetDashStyle, ParserImpl, ToString, TryParse
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MIME.Html.Language.CSS

#If NET48 Then
Imports Font = System.Drawing.Font
Imports Pen = System.Drawing.Pen
Imports DashStyle = System.Drawing.Drawing2D.DashStyle
#Else
Imports Font = Microsoft.VisualBasic.Imaging.Font
Imports Pen = Microsoft.VisualBasic.Imaging.Pen
Imports DashStyle = Microsoft.VisualBasic.Imaging.DashStyle
#End If

Namespace CSS

    ''' <summary>
    ''' ```css
    ''' stroke: color/image; stroke-width: width(px); stroke-dash: dash_style;
    ''' ```
    ''' </summary>
    Public Class Stroke : Inherits ICSSValue

        Public Const AxisStroke$ = "stroke: black; stroke-width: 2px; stroke-dash: solid;"
        Public Const AxisGridStroke$ = "stroke: lightgray; stroke-width: 2px; stroke-dash: dash;"
        Public Const AxisGridMinorStroke$ = "stroke: lightgray; stroke-width: 1px; stroke-dash: dash;"
        Public Const HighlightStroke$ = "stroke: gray; stroke-width: 2px; stroke-dash: dash;"
        Public Const StrongHighlightStroke$ = "stroke: black; stroke-width: 2px; stroke-dash: dash;"
        Public Const ScatterLineStroke$ = "stroke: black; stroke-width: 2px; stroke-dash: solid;"
        Public Const WhiteLineStroke$ = "stroke: white; stroke-width: 2px; stroke-dash: solid;"

        ''' <summary>
        ''' the line fill color brush description
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>
        ''' default fill is black color if this value is omit from the css style string
        ''' </remarks>
        Public Property fill As String
        ''' <summary>
        ''' the line drawing width
        ''' </summary>
        ''' <returns></returns>
        Public Property width As String

#If NET48 Then
        ''' <summary>
        ''' the line drawing style
        ''' </summary>
        ''' <returns></returns>
        Public Property dash As System.Drawing.Drawing2D.DashStyle
#Else
        ''' <summary>
        ''' the line drawing style
        ''' </summary>
        ''' <returns></returns>
        Public Property dash As Microsoft.VisualBasic.Imaging.DashStyle
#End If

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

        Sub New(width As Single, color As Color)
            Call Me.New(width)
            fill = color.ToHtmlColor
        End Sub

        Sub New(color As Color, w!)
            Call Me.New(w)
            fill = color.ToHtmlColor
        End Sub

        Sub New(width As Double)
            Call Me.New(CSng(width))
        End Sub

#If NET48 Then
        ''' <summary>
        ''' create css stroke object based on a specific given 
        ''' gdi+ object
        ''' </summary>
        ''' <param name="style"></param>
        Sub New(style As System.Drawing.Pen)
            width = style.Width
            fill = style.Color.ToHtmlColor
            dash = style.DashStyle
        End Sub
#End If

#If NET8_0_OR_GREATER Then
        Sub New(style As Microsoft.VisualBasic.Imaging.Pen)
            width = style.Width
            fill = style.Color.ToHtmlColor
            dash = style.DashStyle
        End Sub
#End If

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
            Return $"stroke: {fill}; stroke-width: {width}; stroke-dash: {dash.ToString.ToLower};"
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

        ''' <summary>
        ''' this css parser function returns nothing if the given <paramref name="css"/> string value is null or empty
        ''' </summary>
        ''' <param name="css$"></param>
        ''' <param name="[default]"></param>
        ''' <returns></returns>
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
            Dim styles As Selector = CssParser.ParseStyle(css)
            Dim st As New Stroke With {
                .dash = GetDashStyle(styles("stroke-dash")),
                .fill = styles("stroke"),
                .width = styles("stroke-width")
            }

            If st.fill.StringEmpty Then
                st.fill = "black"
            End If

            Return st
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Widening Operator CType(css$) As Stroke
            Return TryParse(css)
        End Operator
    End Class
End Namespace
