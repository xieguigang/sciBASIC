#Region "Microsoft.VisualBasic::56f355f7f7a06e26dadd9efe7a81816b, mime\text%html\Render\TextString.vb"

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

    '   Total Lines: 392
    '    Code Lines: 272 (69.39%)
    ' Comment Lines: 61 (15.56%)
    '    - Xml Docs: 40.98%
    ' 
    '   Blank Lines: 59 (15.05%)
    '     File Size: 15.27 KB


    '     Class TextString
    ' 
    '         Properties: color, font, text, weight
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: GetWeightedFont, ToString
    '         Enum WeightStyles
    ' 
    '             [sub], [sup]
    ' 
    ' 
    ' 
    '  
    ' 
    ' 
    ' 
    '     Module TextAPI
    ' 
    '         Function: __nextTag, GetCssFont, GetFontColor, getFontStyle, getLocalScopeFontStyle
    '                   htmlParser, nextEndTag, popAttrName, popAttrValue, popTagName
    '                   setFont, (+2 Overloads) TryParse
    ' 
    '         Sub: SkipWhiteSpace
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.Emit.Marshal
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.MIME.Html.CSS
Imports Microsoft.VisualBasic.MIME.Html.Document
Imports Microsoft.VisualBasic.MIME.Html.Language.CSS
Imports Microsoft.VisualBasic.Text.Xml

Namespace Render

    Public Class TextString

        Public Property font As Font
        Public Property text As String
        Public Property weight As WeightStyles
        Public Property color As String

        Sub New()
        End Sub

        Sub New(copy As TextString)
            font = copy.font.Clone
            text = copy.text
            weight = copy.weight
            color = copy.color
        End Sub

        Public Function GetWeightedFont() As Font
            Select Case weight
                Case WeightStyles.sub, WeightStyles.sup
                    Return New Font(font.Name, font.Size / 2)
                Case Else
                    Return font
            End Select
        End Function

        Public Overrides Function ToString() As String
            Return text
        End Function

        Public Enum WeightStyles As Integer
            normal = 0
            ''' <summary>
            ''' 上标
            ''' </summary>
            [sup]
            ''' <summary>
            ''' 下标
            ''' </summary>
            [sub]
        End Enum

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Narrowing Operator CType(text As TextString) As String
            Return text.text
        End Operator
    End Class

    ''' <summary>
    ''' 用于简单的HTML文档渲染
    ''' </summary>
    Public Module TextAPI

        ''' <summary>
        ''' 从当前的这个html节点之中的style属性获取得到字体样式的定义数据
        ''' </summary>
        ''' <param name="node"></param>
        ''' <returns></returns>
        <Extension>
        Public Function GetCssFont(node As HtmlElement, ppi As Integer) As Font
            With node("style")
                If .IsEmpty Then
                    Return Nothing
                Else
                    Dim hasValue As Boolean = False
                    Dim css As CSSFont = CSSFont.TryParse(.Value, hasValue:=hasValue)

                    ' 因为解析函数无论是否存在数据都会返回一个cssfont实例
                    ' 所以会需要借助这个hasValue变量来判断
                    If hasValue Then
                        Return css.GDIObject(ppi)
                    Else
                        Return Nothing
                    End If
                End If
            End With
        End Function

        <Extension>
        Public Function GetFontColor(node As HtmlElement) As String
            With node("style")
                If .IsEmpty Then
                    Return Nothing
                Else
                    Return CssParser.GetProperty(.Value) _
                        .Where(Function(p) p.key = "color") _
                        .FirstOrDefault _
                        .value
                End If
            End With
        End Function

        ' html -->  <font face="Microsoft YaHei" size="1.5"><strong>text</strong><b><i>value</i></b></font> 
        ' 解析上述的表达式会产生一个栈，根据html标记来赋值字符串的gdi+属性

        ''' <summary>
        ''' 执行html栈空间解析
        ''' 
        ''' > 在这里假设所有的文本之中的``&lt;``符号已经被转义为``&amp;lt;``
        ''' </summary>
        ''' <param name="html">假设所传递进入这个函数参数的html文本字符串都是完全正确的格式的</param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function TryParse(html$,
                                 Optional defaultFont$ = CSSFont.Win7Normal,
                                 Optional defaultColor$ = NameOf(Color.Black),
                                 Optional ppi As Integer = 100) As IEnumerable(Of TextString)

            Return TryParse(html, CSSFont.TryParse(defaultFont).GDIObject(ppi), defaultColor.TranslateColor)
        End Function

        Public Iterator Function TryParse(html$, defaultFont As Font, defaultColor As Color,
                                          Optional ppi As Integer = 100) As IEnumerable(Of TextString)

            Dim buffer As New Pointer(Of Char)(html.ToCharArray)
            Dim currentStyle As New TextString With {
                .font = defaultFont,
                .weight = TextString.WeightStyles.normal,
                .color = defaultColor.ToHtmlColor
            }
            Dim blanks As New Regex("\s+")

            For Each part As TextString In htmlParser(buffer, currentStyle, ppi)
                ' 忽略掉多余的空白
                ' 因为转义操作会产真正所需要的的空白符, 所以去除多余的空白符要先于转义操作之前进行
                part.text = blanks.Replace(part.text, " ")
                ' 在这里处理转义
                part.text = XmlEntity.UnescapeHTML(part.text)

                Yield part
            Next
        End Function

        Private Iterator Function htmlParser(html As Pointer(Of Char), defaultStyle As TextString, ppi As Integer) As IEnumerable(Of TextString)
            Dim charsbuffer As New List(Of Char)
            Dim c As Char
            Dim bold As Boolean = False
            Dim italic As Boolean = False
            Dim currentStyle As TextString = defaultStyle

            ' 每遇到一个新的标签就进栈
            ' 遇到结束标记则退栈
            Dim styleStack As New Stack(Of TextString)

            Do While Not html.EndRead
                c = ++html

                ' 遇到了一个html标签的起始符号
                If c = "<"c Then
                    ' 查看一下下一个字符是什么
                    ' 因为前面已经移动到下一个位置了
                    ' 所以在这里直接读取current
                    c = html.Current

                    If charsbuffer.Count > 0 Then
                        Yield New TextString With {
                            .font = currentStyle.font,
                            .text = New String(charsbuffer.PopAll),
                            .color = currentStyle.color,
                            .weight = currentStyle.weight
                        }
                    End If

                    ' 这个是一个结束的标记
                    ' 退出当前的栈
                    If c = "/"c Then
                        ' 需要调用下面的方法在html指针上产生位移
                        Dim tag As String = html.nextEndTag.CharString
                        currentStyle = styleStack.Pop()
                    Else
                        ' 这个是一个html标记的开始
                        ' 当前的字体样式需要圧栈
                        Dim tag As HtmlElement = html.__nextTag()
                        Dim tagName As String = tag.TagName.ToLower
                        Dim localScopeStyle As Font = tag.GetCssFont(ppi)
                        Dim localFontColor$ = tag.GetFontColor Or currentStyle.color.AsDefault

                        styleStack.Push(currentStyle)

                        ' 没有在style之中定义字体样式
                        ' 则任然使用原来的字体样式
                        If localScopeStyle Is Nothing Then
                            ' make a copy from previous stack
                            ' to avoid class object modify
                            currentStyle = New TextString(currentStyle)
                        Else
                            ' 更换新的字体样式
                            currentStyle = New TextString(currentStyle) With {
                                .font = localScopeStyle
                            }
                        End If

                        currentStyle.color = localFontColor

                        Select Case tagName
                            Case "font"
                               ' currentFont = tag.setFont(bold, italic, defaultFont)
                            Case "strong", "b"
                                bold = True
                                currentStyle.font = currentStyle.font.getLocalScopeFontStyle(bold, italic)
                            Case "i"
                                italic = True
                                currentStyle.font = currentStyle.font.getLocalScopeFontStyle(bold, italic)
                            Case "br"
                                charsbuffer += vbLf
                            Case "sup"
                                currentStyle.weight = TextString.WeightStyles.sup
                            Case "sub"
                                currentStyle.weight = TextString.WeightStyles.sub
                        End Select
                    End If

                    html.MoveNext()
                Else
                    charsbuffer += c
                End If
            Loop

            If charsbuffer.Count > 0 Then
                Yield New TextString With {
                    .font = currentStyle.font,
                    .text = New String(charsbuffer.PopAll),
                    .color = currentStyle.color,
                    .weight = currentStyle.weight
                }
            End If
        End Function

        Const FontFaceTag As String = "face"
        Const FontSizeTag As String = "size"

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Private Function getLocalScopeFontStyle(font As Font, bold As Boolean, italic As Boolean) As Font
            Return New Font(font, getFontStyle(bold, italic))
        End Function

        Private Function getFontStyle(bold As Boolean, italic As Boolean) As FontStyle
            Dim style As FontStyle

            If Not bold AndAlso Not italic Then
                style = FontStyle.Regular
            Else
                If bold Then
                    style += FontStyle.Bold
                End If
                If italic Then
                    style += FontStyle.Italic
                End If
            End If

            Return style
        End Function

        <Extension>
        Private Function setFont(font As HtmlElement, bold As Boolean, italic As Boolean, [default] As Font) As Font
            Dim name As String = font(FontFaceTag).Value
            Dim size As String = font(FontSizeTag).Value

            If String.IsNullOrEmpty(name) Then
                name = [default].Name
            End If
            If String.IsNullOrEmpty(size) Then
                size = [default].Size
            End If

            Dim style As FontStyle = getFontStyle(bold, italic)
            Dim sz As Single = Scripting.CTypeDynamic(Of Single)(size)

            Return New Font(name, sz, style)
        End Function

        <Extension>
        Private Iterator Function nextEndTag(buffer As Pointer(Of Char)) As IEnumerable(Of Char)
            ' 跳过当前的/符号
            Call buffer.MoveNext()

            Do While Not buffer.EndRead AndAlso buffer.Current <> ">"c
                Yield ++buffer
            Loop
        End Function

        ''' <summary>
        ''' 当遇到空格或者>符号的时候, 说明得到了一个html标签
        ''' </summary>
        ''' <param name="buffer"></param>
        ''' <returns></returns>
        ''' 
        <Extension>
        Private Iterator Function popTagName(buffer As Pointer(Of Char)) As IEnumerable(Of Char)
            Do While Not buffer.EndRead AndAlso buffer.Current <> " "c AndAlso buffer.Current <> ">"c
                Yield ++buffer
            Loop
        End Function

        <Extension>
        Private Iterator Function popAttrName(buffer As Pointer(Of Char)) As IEnumerable(Of Char)
            Dim nameBegin As Boolean = False

            Do While Not buffer.EndRead AndAlso buffer.Current <> "="c
                If buffer.Current = " "c Then
                    If nameBegin Then
                        Do While Not buffer.EndRead AndAlso ++buffer <> "="c
                        Loop

                        Exit Do
                    Else
                        Call buffer.MoveNext()
                    End If
                Else
                    nameBegin = True

                    ' 取出当前的字符,直到遇到空格或者等号,即取完了当前的属性名称
                    Yield ++buffer
                End If
            Loop
        End Function

        <Extension>
        Private Sub SkipWhiteSpace(buffer As Pointer(Of Char))
            If buffer.Current <> " "c Then
                ' 已经不是空格了,则不需要下一步了
                Return
            End If

            Do While Not buffer.EndRead AndAlso ++buffer <> " "c
            Loop
        End Sub

        <Extension>
        Private Iterator Function popAttrValue(buffer As Pointer(Of Char)) As IEnumerable(Of Char)
            Dim quot As Char = ++buffer

            If quot <> "'"c AndAlso quot <> """"c Then
                Throw New SyntaxErrorException("Invalid attribute value string!")
            End If

            Do While Not buffer.EndRead
                ' 判断是否结束当前的字符串
                If buffer.Current = quot Then
                    ' 是的,结束当前的字符串
                    ' 在结束前需要跳过当前的quot,否则后面会出错
                    quot = ++buffer
                    Exit Do
                Else
                    Yield ++buffer
                End If
            Loop
        End Function

        <Extension>
        Private Function __nextTag(buffer As Pointer(Of Char)) As HtmlElement
            Dim name, value As String
            Dim tag As New HtmlElement With {
                .TagName = buffer.popTagName.CharString
            }

            Do While Not buffer.EndRead
                If buffer.Current = ">"c Then
                    Exit Do
                End If

                ' 解析完标签之后,尝试解析出标签的属性
                ' 在这里进行解析的是属性的名称，不允许有空格
                name = buffer.popAttrName.CharString

                Call buffer.MoveNext()
                ' 字符串的起始可能是一串空格,跳过这些空格
                Call buffer.SkipWhiteSpace

                value = buffer.popAttrValue.CharString
                tag(name) = New ValueAttribute(name, value)
            Loop

            Return tag
        End Function
    End Module
End Namespace
