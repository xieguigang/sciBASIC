#Region "Microsoft.VisualBasic::0ce49e1fe4202af26f1b5718ca4a56b9, mime\text%html\Parser\CSS\Parser.vb"

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

    '   Total Lines: 206
    '    Code Lines: 131 (63.59%)
    ' Comment Lines: 50 (24.27%)
    '    - Xml Docs: 90.00%
    ' 
    '   Blank Lines: 25 (12.14%)
    '     File Size: 7.72 KB


    '     Module CssParser
    ' 
    '         Function: BuildSelector, GetProperty, GetTagWithCSS, IndivisualTags, IsNullOrEmpty
    '                   ParseStyle, RemoveWhitespace, RemoveWitespaceFormStartAndEnd
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Text
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Language
Imports r = System.Text.RegularExpressions.Regex

Namespace Language.CSS

    ''' <summary>
    ''' Parser css code to add/remove or manage it.
    ''' </summary>
    ''' <remarks>
    ''' #### Jo-CSS-Parser
    ''' 
    ''' https://github.com/rizwan3d/Jo-CSS-Parser
    ''' 
    ''' > Complete Css Parser Writen in C#
    ''' </remarks>
    Public Module CssParser

        <Extension>
        Public Function IsNullOrEmpty(css As CSSFile) As Boolean
            If css Is Nothing Then
                Return True
            Else
                Return css.Selectors.IsNullOrEmpty
            End If
        End Function

        ''' <summary>
        ''' 创建元素选择器表达式
        ''' </summary>
        ''' <param name="name"></param>
        ''' <param name="type"></param>
        ''' <returns></returns>
        <Extension>
        Public Function BuildSelector(name$, type As CSSSelectorTypes) As String
            Select Case type
                Case CSSSelectorTypes.class
                    If name.First <> "."c Then
                        Return "." & name
                    Else
                        Return name
                    End If
                Case CSSSelectorTypes.id
                    If name.First <> "#" Then
                        Return "#" & name
                    Else
                        Return name
                    End If
                Case CSSSelectorTypes.tag
                    Return name
                Case CSSSelectorTypes.expression
                    Return name
                Case Else
                    Return name
            End Select
        End Function

        ''' <summary>
        ''' 主要的CSS解析函数
        ''' </summary>
        ''' <param name="CSS">CSS文本内容</param>
        ''' <returns></returns>
        ''' <remarks>
        ''' 对于从html标签的style属性中解析出来的样式字符串，会被直接赋值命名为``*``。
        ''' </remarks>
        Public Function GetTagWithCSS(CSS$, Optional selectorFilter$ = Nothing) As CSSFile
            Dim tagWithCSSList As New List(Of Selector)
            Dim IndivisualTag As List(Of String) = IndivisualTags(CSS.SolveStream).AsList
            Dim filter As Predicate(Of String)

            If selectorFilter.StringEmpty Then
                filter = Function() True
            Else
                With New Regex(selectorFilter, RegexICSng)
                    filter = Function(selector$) .Match(selector).Success
                End With
            End If

            For Each tag As String In IndivisualTag
                Dim tagname$() = r.Split(tag, "[{]")

                If RemoveWhitespace(tagname(0)) <> "" Then
                    Dim selector$ = RemoveWitespaceFormStartAndEnd(tagname(0))
                    Dim properties = tag _
                        .GetBetween("{", "}") _
                        .GetProperty() _
                        .ToDictionary(Function(prop) prop.key,
                                      Function(prop)
                                          Return prop.value
                                      End Function)

                    If filter(selector) Then
                        tagWithCSSList += New Selector With {
                            .Selector = selector,
                            .Properties = properties
                        }
                    End If
                End If
            Next

            Return New CSSFile With {
                .Selectors = tagWithCSSList.ToDictionary
            }
        End Function

        ''' <summary>
        ''' Parse the css style string for a given selector, example as: ``background-color: lightblue;``
        ''' </summary>
        ''' <param name="style">the style text inside an element node selector</param>
        ''' <returns></returns>
        ''' <remarks>
        ''' all of the css property name is in lower case.
        ''' </remarks>
        Public Function ParseStyle(style As String) As Selector
            Dim properties = Strings.Trim(style) _
                .GetProperty() _
                .ToDictionary(Function(prop) prop.key,
                                Function(prop)
                                    Return prop.value
                                End Function)

            Return New Selector With {
                .Selector = "*",
                .Properties = properties
            }
        End Function

        Const IndivisualTagsPattern$ = "(?<selector>(?:(?:[^,{]+),?)*?)\{(?:(?<name>[^}:]+):?(?<value>[^};]+);?)*?\}"

        ''' <summary>
        ''' CSS的注释总是以/*起始，以*/结束的
        ''' </summary>
        Const CommentBlock$ = "/\*.+?\*/"

        ''' <summary>
        ''' ###### 2017-10-1
        ''' 
        ''' 原来的这个解析函数还没有考虑到注释的问题
        ''' 当CSS之中存在注释的时候就无法正常工作了
        ''' </summary>
        ''' <param name="input"></param>
        ''' <returns></returns>
        Private Iterator Function IndivisualTags(input As String) As IEnumerable(Of String)
            Dim s As New StringBuilder(input)

            ' 首先需要移除掉CSS的注释文本
            For Each block As Match In r.Matches(input, CommentBlock, RegexICSng)
                Call s.Replace(block.Value, "")
            Next

            If input.Contains("{"c) AndAlso input.Contains("}"c) Then
                For Each m As Match In r.Matches(s.ToString, IndivisualTagsPattern, RegexICSng, New TimeSpan(0, 0, 1))
                    Yield m.Value.StripBlank
                Next
            Else
                ' 直接从html tag之中解析出来的style值字符串？
                Yield $"* {{ {input} }}"
            End If
        End Function

        ''' <summary>
        ''' 将CSS字符串解析为键值对集合
        ''' </summary>
        ''' <param name="input"></param>
        ''' <returns></returns>
        <Extension>
        Public Iterator Function GetProperty(input As String) As IEnumerable(Of (key$, value$))
            Dim s As String() = r.Split(input, "[;]")

            For Each b As String In s
                If b.StringEmpty Then
                    Continue For
                End If

                Dim t As String() = r.Split(b, "[:]")

                If t.Length = 2 Then
                    Dim propertyName$ = Nothing, propertyValue$ = Nothing

                    If t(0) <> "" Then
                        propertyName = RemoveWhitespace(t(0)).ToLower
                    End If
                    If t(1) <> "" Then
                        propertyValue = RemoveWitespaceFormStartAndEnd(t(1))
                    End If

                    Yield (propertyName, propertyValue)
                End If
            Next
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Function RemoveWhitespace(input As String) As String
            Return New String(input.ToCharArray().Where(Function(c) Not Char.IsWhiteSpace(c)).ToArray())
        End Function

        Private Function RemoveWitespaceFormStartAndEnd(input As String) As String
            input = input.Trim()
            input = input.TrimEnd()
            Return input
        End Function
    End Module
End Namespace
