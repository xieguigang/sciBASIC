Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports Microsoft.VisualBasic.Imaging

Namespace HTML.CSS

    ''' <summary>
    ''' ```css
    ''' stroke: color/image; stroke-width: width(px); stroke-dash: dash_style;
    ''' ```
    ''' </summary>
    Public Class Stroke

        Public Const AxisStroke$ = "stroke: black; stroke-width: 2px; stroke-dash: solid;"

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

        Public Shared Function TryParse(css$) As Stroke
            Dim t As Dictionary(Of String, String) = css.Split(";"c) _
                .Select(AddressOf Trim) _
                .Select(Function(s) s.GetTagValue(":", trim:=True)) _
                .ToDictionary(Function(x) x.Name,
                              Function(x) x.Value)

            Return New Stroke With {
                .dash = GetDashStyle(t.TryGetValue("stroke-dash")),
                .fill = t.TryGetValue("stroke"),
                .width = Val(t.TryGetValue("stroke-width"))
            }
        End Function

        Public Shared Widening Operator CType(css$) As Stroke
            Return TryParse(css)
        End Operator
    End Class
End Namespace