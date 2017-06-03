#Region "Microsoft.VisualBasic::4af52c025303783ea6cd80e92c2ec0e7, ..\sciBASIC#\mime\text%html\HTML\TextString.vb"

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
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Emit.Marshal
Imports Microsoft.VisualBasic.Language

Namespace HTML

    Public Class TextString

        Public Property Font As Font
        Public Property Text As String

        Public Overrides Function ToString() As String
            Return Text
        End Function
    End Class

    Public Module TextAPI

        ' html -->  <font face="Microsoft YaHei" size="1.5"><strong>text</strong><b><i>value</i></b></font> 
        ' 解析上述的表达式会产生一个栈，根据html标记来赋值字符串的gdi+属性

        ''' <summary>
        ''' 执行html栈空间解析
        ''' </summary>
        ''' <param name="html"></param>
        ''' <returns></returns>
        Public Function TryParse(html As String, Optional defaultFont As Font = Nothing) As TextString()
            Dim chars As New List(Of Char)
            Dim str As New Pointer(Of Char)(html.ToCharArray)
            Dim tokens As New List(Of TextString)

            If defaultFont Is Nothing Then
                defaultFont = New Font(FontFace.MicrosoftYaHei, 12, FontStyle.Regular)
            End If

            ' 在这里假设所有的文本之中的<符号已经被转义为&lt;

            Dim c As Char
            Dim bold As Boolean = False
            Dim italic As Boolean = False
            Dim curFont As Font = defaultFont

            Do While Not str.EndRead
                c = +str

                If c = "<"c Then  ' 遇到了一个html标签的起始符号
                    c = +str

                    If chars.Count > 0 Then
                        tokens += New TextString With {
                        .Font = curFont,
                        .Text = New String(chars.PopAll)
                    }
                    End If

                    If c = "/"c Then  ' 这个是一个结束的标记
                        Dim tag As String = str.__nextEndTag

                        Select Case tag.ToLower
                            Case "font"
                                curFont = defaultFont
                            Case "b", "strong"
                                bold = False
                                curFont = curFont.__setFontStyle(bold, italic)
                            Case "i"
                                italic = False
                                curFont = curFont.__setFontStyle(bold, italic)
                        End Select
                    Else ' 这个是一个html标记的开始
                        Dim tag As HtmlElement = str.__nextTag(c)
                        Dim tagName As String = tag.Name.ToLower

                        Select Case tagName
                            Case "font"
                                curFont = tag.__setFont(bold, italic, defaultFont)
                            Case "strong", "b"
                                bold = True
                                curFont = curFont.__setFontStyle(bold, italic)
                            Case "i"
                                italic = True
                                curFont = curFont.__setFontStyle(bold, italic)
                            Case "br"
                                chars += vbLf
                            Case Else

                        End Select
                    End If

                    str.MoveNext()
                Else
                    chars += c
                End If
            Loop

            If chars.Count > 0 Then
                tokens += New TextString With {
                .Font = curFont,
                .Text = New String(chars.PopAll)
            }
            End If

            ' 在这里处理转义
            For Each x In tokens
                x.Text = x.Text.Replace("&lt;", "<")
            Next

            Return tokens
        End Function

        Const FontFaceTag As String = "face"
        Const FontSizeTag As String = "size"

        <Extension>
        Private Function __setFontStyle(font As Font, bold As Boolean, italic As Boolean) As Font
            Return New Font(font, __getFontStyle(bold, italic))
        End Function

        Private Function __getFontStyle(bold As Boolean, italic As Boolean) As FontStyle
            Dim style As FontStyle

            If bold Then
                style += FontStyle.Bold
            End If
            If italic Then
                style += FontStyle.Italic
            End If
            If Not bold AndAlso Not italic Then
                style = FontStyle.Regular
            End If

            Return style
        End Function

        <Extension>
        Private Function __setFont(font As HtmlElement, bold As Boolean, italic As Boolean, [default] As Font) As Font
            Dim name As String = font(FontFaceTag).Value
            Dim size As String = font(FontSizeTag).Value

            If String.IsNullOrEmpty(name) Then
                name = [default].Name
            End If
            If String.IsNullOrEmpty(size) Then
                size = [default].Size
            End If

            Dim style As FontStyle = __getFontStyle(bold, italic)
            Dim sz As Single = Scripting.CTypeDynamic(Of Single)(size)

            Return New Font(name, sz, style)
        End Function

        <Extension>
        Private Function __nextEndTag(str As Pointer(Of Char)) As String
            Dim chars As New List(Of Char)

            Do While Not str.EndRead AndAlso str.Current <> ">"c
                chars += (+str)
            Loop

            Return New String(chars)
        End Function

        <Extension>
        Private Function __nextTag(str As Pointer(Of Char), c As Char) As HtmlElement
            Dim chars As New List(Of Char) From {c}
            Dim tag As New HtmlElement

            Do While Not str.EndRead AndAlso str.Current <> " "c AndAlso str.Current <> ">"c
                chars += +str
            Loop

            tag.Name = New String(chars.PopAll)

            Dim name As String
            Dim stacked As Boolean

            Do While Not str.EndRead
                If str.Current = ">"c Then
                    Exit Do
                End If

                Do While Not str.EndRead AndAlso str.Current <> "="c
                    If str.Current = " "c Then
                        If chars.Count > 0 Then
                            Do While Not str.EndRead AndAlso +str <> "="c
                            Loop
                            Exit Do   ' 在这里进行解析的是属性的名称，不允许有空格
                        Else
                            Call str.MoveNext()
                        End If
                    Else
                        chars += +str
                    End If
                Loop
                name = New String(chars.PopAll)
                str.MoveNext()

                Do While Not str.EndRead
                    If str.Current = """"c Then
                        If chars.Count = 0 AndAlso stacked = False Then
                            stacked = True
                            str.MoveNext()
                        Else ' 这里是一个结束的标志，准备开始下一个token
                            stacked = False
                            str.MoveNext()
                            Exit Do
                        End If
                    Else
                        If str.Current = " "c Then
                            If Not chars.Count = 0 Then
                                chars += " "c
                            End If
                            str.MoveNext()
                        Else
                            chars += +str
                        End If
                    End If
                Loop

                Call tag.Add(New ValueAttribute(name, New String(chars.PopAll)))
            Loop

            Return tag
        End Function
    End Module
End Namespace
