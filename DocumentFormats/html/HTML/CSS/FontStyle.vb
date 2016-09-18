Imports System.Drawing
Imports System.Text
Imports Microsoft.VisualBasic.Imaging

Namespace CSS

    ''' <summary>
    ''' ```CSS
    ''' font-style: style; font-size: size; font-family: Name;
    ''' ```
    ''' 
    ''' 这个简写属性用于一次设置元素字体的两个或更多方面。使用 ``icon`` 等关键字可以适当地设置元素的字体，使之与用户计算机环境中的某个方面一致。
    ''' 注意，如果没有使用这些关键词，至少要指定字体大小和字体系列。
    ''' 可以按顺序设置如下属性：
    '''
    ''' + font-style
    ''' + font-variant
    ''' + font-weight
    ''' + font-size/line-height
    ''' + font-family
    '''
    ''' 可以不设置其中的某个值，比如 ``font:100% verdana;`` 也是允许的。未设置的属性会使用其默认值。
    ''' </summary>
    Public Class CSSFont

        ''' <summary>
        ''' The System.Drawing.FontStyle of the new font.
        ''' </summary>
        ''' <returns></returns>
        Public Property style As FontStyle = FontStyle.Regular
        Public Property size As Single
        ''' <summary>
        ''' A string representation of the System.Drawing.FontFamily for the new System.Drawing.Font.
        ''' </summary>
        ''' <returns></returns>
        Public Property family As String
        Public Property weight As Single
        Public Property [variant] As String

        ''' <summary>
        ''' Initializes a new <see cref="System.Drawing.Font"/> using a specified size and style.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property GDIObject As Font
            Get
                Return New Font(family, size, style)
            End Get
        End Property

        Public Overloads Shared Function ToString(style As Drawing.FontStyle) As String
            Select Case style
                Case Drawing.FontStyle.Bold
                    Return strong
                Case Drawing.FontStyle.Italic
                    Return italic
                Case Drawing.FontStyle.Regular
                    Return normal
                Case Drawing.FontStyle.Strikeout
                    Return strikeout
                Case Drawing.FontStyle.Underline
                    Return underline
                Case Else
                    Return normal
            End Select
        End Function

        Public Shared Function GetStyle(style As String) As String
            Select Case LCase(style)
                Case normal
                    Return Drawing.FontStyle.Regular
                Case italic
                    Return Drawing.FontStyle.Italic
                Case strong
                    Return Drawing.FontStyle.Bold
                Case strikeout
                    Return Drawing.FontStyle.Strikeout
                Case underline
                    Return Drawing.FontStyle.Underline
                Case Else
                    Return Drawing.FontStyle.Regular
            End Select
        End Function

        ''' <summary>
        ''' Parsing font style data from the css expression string.
        ''' </summary>
        ''' <param name="css"></param>
        ''' <param name="[default]">On failure return this default value</param>
        ''' <returns></returns>
        Public Shared Function TryParse(css As String, Optional [default] As CSSFont = Nothing) As CSSFont
            Try
                Dim tokens As String() = css.Split(";"c)
                Dim styles As Dictionary(Of String, String) = tokens _
                    .Select(Function(s) s.GetTagValue(":", True)) _
                    .ToDictionary(Function(x) x.Name.ToLower,
                                  Function(x) x.x)
                Dim font As New CSSFont

                If styles.ContainsKey("font-style") Then font.style = GetStyle(styles("font-style"))
                If styles.ContainsKey("font-size") Then font.size = CSng(Val(styles("font-size"))) Else font.size = 12
                If styles.ContainsKey("font-family") Then font.family = styles("font-family") Else font.family = FontFace.MicrosoftYaHei
                If styles.ContainsKey("font-weight") Then font.weight = CSng(Val(styles("font-weight")))
                If styles.ContainsKey("font-variant") Then font.variant = styles("font-variant")

                Return font
            Catch ex As Exception
                Call App.LogException(ex)
                Return [default]
            End Try
        End Function

        Public Overrides Function ToString() As String
            Dim sb As New StringBuilder

            Call sb.Append($"font-style: {ToString(style)};")

            If size > 0 Then
                sb.Append($"font-size: {size};")
            End If
            If Not String.IsNullOrEmpty(family) Then
                sb.Append($"font-family: {family};")
            End If
            If weight > 0 Then
                sb.Append($"font-weight: {weight};")
            End If
            If Not String.IsNullOrEmpty([variant]) Then
                sb.Append($"font-variant: {[variant]};")
            End If

            Return sb.ToString
        End Function

        Public Const strong = "strong"
        Public Const italic = "italic"
        Public Const normal = "normal"
        Public Const strikeout = "strikeout"
        Public Const underline = "underline"

    End Class
End Namespace