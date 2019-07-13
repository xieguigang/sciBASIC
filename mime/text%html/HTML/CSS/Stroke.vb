#Region "Microsoft.VisualBasic::cf82a88b484df116111383a454930959, mime\text%html\HTML\CSS\Stroke.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    '     Class Stroke
    ' 
    '         Properties: CSSValue, dash, fill, GDIObject, width
    ' 
    '         Constructor: (+3 Overloads) Sub New
    '         Function: GetDashStyle, ToString, TryParse
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Imaging

Namespace HTML.CSS

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

        Public Property fill As String
        Public Property width As Single
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

        Sub New(width!)
            Me.width = width
            fill = "black"
            dash = DashStyle.Solid
        End Sub

        Sub New(style As Pen)
            width = style.Width
            fill = style.Color.ToHtmlColor
            dash = style.DashStyle
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

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Narrowing Operator CType(stroke As Stroke) As Pen
            Return stroke.GDIObject
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Widening Operator CType(css$) As Stroke
            Return TryParse(css)
        End Operator
    End Class
End Namespace
