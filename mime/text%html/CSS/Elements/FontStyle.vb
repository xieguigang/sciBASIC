#Region "Microsoft.VisualBasic::b0286eaf191bd7d8346bf4b7002aa87b, mime\text%html\CSS\FontStyle.vb"

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

    '   Total Lines: 310
    '    Code Lines: 181 (58.39%)
    ' Comment Lines: 95 (30.65%)
    '    - Xml Docs: 89.47%
    ' 
    '   Blank Lines: 34 (10.97%)
    '     File Size: 14.26 KB


    '     Class CSSFont
    ' 
    '         Properties: [variant], color, CSSValue, family, size
    '                     style, weight
    ' 
    '         Constructor: (+3 Overloads) Sub New
    '         Function: (+2 Overloads) GetFontStyle, GetStyle, parseInner, SetFontColor, (+2 Overloads) ToString
    '                   TryParse
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.ComponentModel
Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.MIME.Html.Language.CSS

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
    Public Class CSSFont : Inherits ICSSValue

        Public Const Win10Normal$ = "font-style: normal; font-size: 12; font-family: " & FontFace.SegoeUI & ";"
        Public Const Win10NormalLarger$ = "font-style: normal; font-size: 16; font-family: " & FontFace.SegoeUI & ";"
        Public Const Win10NormalLarge$ = "font-style: normal; font-size: 20; font-family: " & FontFace.SegoeUI & ";"

        Public Const Win7Small$ = "font-style: normal; font-size: 10; font-family: " & FontFace.MicrosoftYaHei & ";"
        Public Const Win7Normal$ = "font-style: normal; font-size: 12; font-family: " & FontFace.MicrosoftYaHei & ";"
        Public Const Win7Bold$ = "font-style: strong; font-size: 12; font-family: " & FontFace.MicrosoftYaHei & ";"
        Public Const Win7LargerBold$ = "font-style: strong; font-size: 16; font-family: " & FontFace.MicrosoftYaHei & ";"
        Public Const Win7LargerNormal$ = "font-style: normal; font-size: 16; font-family: " & FontFace.MicrosoftYaHei & ";"
        Public Const Win7LargeBold$ = "font-style: strong; font-size: 24; font-family: " & FontFace.MicrosoftYaHei & ";"
        Public Const Win7LittleLarge$ = "font-style: normal; font-size: 20; font-family: " & FontFace.MicrosoftYaHei & ";"
        Public Const Win7Large$ = "font-style: normal; font-size: 24; font-family: " & FontFace.MicrosoftYaHei & ";"
        Public Const Win7VeryLarge$ = "font-style: normal; font-size: 36; font-family: " & FontFace.MicrosoftYaHei & ";"
        Public Const Win7VeryVeryLarge$ = "font-style: strong; font-size: 56; font-family: " & FontFace.MicrosoftYaHei & ";"
        Public Const Win7VeryVeryLargeNormal$ = "font-style: normal; font-size: 56; font-family: " & FontFace.MicrosoftYaHei & ";"
        Public Const Win7UltraLarge$ = "font-style: strong; font-size: 72; font-family: " & FontFace.MicrosoftYaHei & ";"
        Public Const Win7UltraLargeNormal$ = "font-style: normal; font-size: 72; font-family: " & FontFace.MicrosoftYaHei & ";"

        Public Const PlotTitle$ = "font-style: strong; font-size: 24; font-family: " & FontFace.BookmanOldStyle & ";"
        Public Const PlotTitleNormal$ = "font-style: normal; font-size: 24; font-family: " & FontFace.BookmanOldStyle & ";"
        Public Const PlotSubTitle$ = "font-style: normal; font-size: 20; font-family: " & FontFace.BookmanOldStyle & ";"
        Public Const PlotSmallTitle$ = "font-style: normal; font-size: 16; font-family: " & FontFace.BookmanOldStyle & ";"
        Public Const PlotLabelNormal$ = "font-style: normal; font-size: 12; font-family: " & FontFace.BookmanOldStyle & ";"

        Public Const UbuntuLarge$ = "font-style: normal; font-size: 20; font-family: " & FontFace.Ubuntu & ";"
        Public Const UbuntuNormal$ = "font-style: normal; font-size: 12; font-family: " & FontFace.Ubuntu & ";"
        Public Const UbuntuBold$ = "font-style: strong; font-size: 12; font-family: " & FontFace.Ubuntu & ";"

        ''' <summary>
        ''' The <see cref="FontStyle"/> of the new font.
        ''' </summary>
        ''' <returns></returns>
        <Description("font-style")> Public Property style As FontStyle = FontStyle.Regular

        ''' <summary>
        ''' 该属性设置元素的字体大小。注意，实际上它设置的是字体中字符框的高度；实际的字符字形可能比这些框高或矮（通常会矮）。
        ''' 各关键字对应的字体必须比一个最小关键字相应字体要高，并且要小于下一个最大关键字对应的字体。
        ''' 
        ''' + ``xx-small``: 把字体的尺寸设置为不同的尺寸，从 xx-small 到 xx-large。
        ''' + ``x-small``
        ''' + ``small``
        ''' + ``medium``:  默认值：medium。
        ''' + ``large``
        ''' + ``x-large``
        ''' + ``xx-large``
        ''' + ``smaller``: 把 font-size 设置为比父元素更小的尺寸。
        ''' + ``larger``:  把 font-size 设置为比父元素更大的尺寸。
        ''' + ``length``:  把 font-size 设置为一个固定的值。
        ''' + ``%``:       把 font-size 设置为基于父元素的一个百分比值。
        ''' + ``inherit``: 规定应该从父元素继承字体尺寸。
        ''' </summary>
        ''' <returns></returns>
        <Description("font-size")> Public Property size As String
        ''' <summary>
        ''' A string representation of the <see cref="FontFamily"/> for the new System.Drawing.Font.
        ''' </summary>
        ''' <returns></returns>
        <Description("font-family")> Public Property family As String

        ''' <summary>
        ''' 该属性用于设置显示元素的文本中所用的字体加粗。数字值 400 相当于 关键字 normal，700 等价于 bold。
        ''' 每个数字值对应的字体加粗必须至少与下一个最小数字一样细，而且至少与下一个最大数字一样粗。
        ''' 
        ''' + ``normal``:  默认值。定义标准的字符。
        ''' + ``bold``:    定义粗体字符。
        ''' + ``bolder``:  定义更粗的字符。
        ''' + ``lighter``: 定义更细的字符。
        ''' + ``100``: 定义由粗到细的字符。400 等同于 normal，而 700 等同于 bold。
        ''' + ``200``: 同上
        ''' + ``300``: 同上
        ''' + ``400``: 同上
        ''' + ``500``: 同上
        ''' + ``600``: 同上
        ''' + ``700``: 同上
        ''' + ``800``: 同上
        ''' + ``900``: 同上
        ''' + ``inherit``: 规定应该从父元素继承字体的粗细。
        ''' </summary>
        ''' <returns></returns>
        <Description("font-weight")> Public Property weight As Single

        ''' <summary>
        ''' ``font-variant``属性设置小型大写字母的字体显示文本，这意味着所有的小写字母均会被转换为大写，
        ''' 但是所有使用小型大写字体的字母与其余文本相比，其字体尺寸更小。
        ''' 
        ''' + ``normal``:     默认值。浏览器会显示一个标准的字体。
        ''' + ``small-caps``: 浏览器会显示小型大写字母的字体。
        ''' + ``inherit``:    规定应该从父元素继承 font-variant 属性的值。
        ''' </summary>
        ''' <returns></returns>
        <Description("font-variant")> Public Property [variant] As String

        Public Property color As String = "black"

        ''' <summary>
        ''' Css string for set font style
        ''' </summary>
        ''' <returns></returns>
        Public Overrides ReadOnly Property CSSValue As String
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return ToString()
            End Get
        End Property

        Sub New()
        End Sub

        Sub New(font As Font, fontSize As Single)
            Call Me.New(font)
            size = fontSize
        End Sub

        Sub New(font As Font)
            family = font.Name
            style = font.Style
            size = font.Size
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function GetFontStyle(family$, style As FontStyle, size$) As String
            Return $"font-style: {ToString(style)}; font-size: {size}; font-family: {family}; color: black;"
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function GetFontStyle(font As Font) As String
            Return GetFontStyle(font.Name, font.Style, font.Size & "px")
        End Function

        Public Function SetFontColor(newColor As String) As CSSFont
            color = newColor
            Return Me
        End Function

        Public Overloads Shared Function ToString(style As FontStyle) As String
            Select Case style
                Case FontStyle.Bold
                    Return strong
                Case FontStyle.Italic
                    Return italic
                Case FontStyle.Regular
                    Return normal
                Case FontStyle.Strikeout
                    Return strikeout
                Case FontStyle.Underline
                    Return underline
                Case Else
                    Return normal
            End Select
        End Function

        Public Shared Function GetStyle(style As String) As String
            Select Case LCase(style)
                Case normal
                    Return FontStyle.Regular
                Case italic
                    Return FontStyle.Italic
                Case strong
                    Return FontStyle.Bold
                Case strikeout
                    Return FontStyle.Strikeout
                Case underline
                    Return FontStyle.Underline
                Case Else
                    Return FontStyle.Regular
            End Select
        End Function

        ''' <summary>
        ''' Parsing font style data from the css expression string.
        ''' (假若你在使用这个函数在Linux环境之中无缘无故的出现空引用错误，需要考虑一下
        ''' 是不是因为Linux服务器之中没有按照所指定的字体文件？)
        ''' </summary>
        ''' <param name="css"></param>
        ''' <param name="[default]">On failure return this default value</param>
        ''' <param name="hasValue">
        ''' 所输入到这个函数进行解析的<paramref name="css"/>样式字符串之中是否包含有字体的定义?
        ''' </param>
        ''' <returns></returns>
        Public Shared Function TryParse(css$, Optional [default] As CSSFont = Nothing, Optional ByRef hasValue As Boolean = False) As CSSFont
            hasValue = False

            If css.StringEmpty Then
                hasValue = False
                Return [default]
            End If

            Try
                Return parseInner(css, hasValue)
            Catch ex As Exception
                Call App.LogException(ex)
                Return [default]
            End Try
        End Function

        Private Shared Function parseInner(css$, ByRef hasValue As Boolean) As CSSFont
            Dim font As New CSSFont
            Dim styles As Selector = CssParser.ParseStyle(css)

            If styles.HasProperty("font-style") Then
                hasValue = True
                font.style = GetStyle(styles("font-style"))
            End If
            If styles.HasProperty("font-size") Then
                hasValue = True
                font.size = styles("font-size")
            Else
                font.size = 12
            End If
            If styles.HasProperty("font-family") Then
                hasValue = True
                font.family = styles("font-family")
            Else
                font.family = FontFace.MicrosoftYaHei
            End If
            If styles.HasProperty("font-weight") Then
                hasValue = True
                font.weight = CSng(Val(styles("font-weight")))
            End If
            If styles.HasProperty("font-variant") Then
                hasValue = True
                font.variant = styles("font-variant")
            End If
            If styles.HasProperty("color") Then
                hasValue = True
                font.color = styles("color")
            Else
                font.color = "black"
            End If

            Return font
        End Function

        ''' <summary>
        ''' To CSS style text
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Function ToString() As String
            Dim sb As New StringBuilder

            Call sb.Append($"font-style: {ToString(style)};")

            If style = FontStyle.Bold Then
                Call sb.Append("font-weight:bold;")
            End If

            If size > 0 Then
                sb.Append($"font-size: {size}px;")
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
            If Not String.IsNullOrEmpty(color) Then
                sb.Append($"color: {color};")
            End If

            Return sb.ToString
        End Function

        Public Const strong = "strong"
        Public Const italic = "italic"
        Public Const normal = "normal"
        Public Const strikeout = "strikeout"
        Public Const underline = "underline"

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Widening Operator CType(font As Font) As CSSFont
            Return New CSSFont(font)
        End Operator

        '<MethodImpl(MethodImplOptions.AggressiveInlining)>
        'Public Shared Narrowing Operator CType(font As CSSFont) As Font
        '    Return font.GDIObject
        'End Operator
    End Class
End Namespace
