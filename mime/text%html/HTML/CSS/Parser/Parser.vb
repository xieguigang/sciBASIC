#Region "Microsoft.VisualBasic::1727d1dc9c3fa71b74ce85d4c6a583f9, ..\sciBASIC#\mime\text%html\HTML\CSS\Parser\Parser.vb"

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

Imports System.Runtime.CompilerServices
Imports System.Text
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.Language
Imports r = System.Text.RegularExpressions.Regex

Namespace HTML.CSS.Parser

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

        ''' <summary>
        ''' ��Ҫ��CSS��������
        ''' </summary>
        ''' <param name="CSS"></param>
        ''' <returns></returns>
        Public Function GetTagWithCSS(CSS As String) As CSSFile
            Dim TagWithCSSList As New List(Of Selector)
            Dim IndivisualTag As List(Of String) = IndivisualTags(CSS)

            For Each tag As String In IndivisualTag
                Dim tagname$() = r.Split(tag, "[{]")

                If RemoveWhitespace(tagname(0)) <> "" Then
                    Dim selector$ = RemoveWitespaceFormStartAndEnd(tagname(0))
                    Dim properties = tag _
                        .GetBetween("{", "}") _
                        .GetProperty() _
                        .ToDictionary(Function(prop) prop.key,
                                      Function(prop) prop.value)

                    TagWithCSSList += New Selector With {
                        .Selector = selector,
                        .Properties = properties
                    }
                End If
            Next

            Return New CSSFile With {
                .Selectors = TagWithCSSList.ToDictionary
            }
        End Function

        Const IndivisualTagsPattern$ = "(?<selector>(?:(?:[^,{]+),?)*?)\{(?:(?<name>[^}:]+):?(?<value>[^};]+);?)*?\}"

        ''' <summary>
        ''' CSS��ע��������/*��ʼ����*/������
        ''' </summary>
        Const CommentBlock$ = "/\*.+?\*/"

        ''' <summary>
        ''' ###### 2017-10-1
        ''' 
        ''' ԭ�����������������û�п��ǵ�ע�͵�����
        ''' ��CSS֮�д���ע�͵�ʱ����޷�����������
        ''' </summary>
        ''' <param name="input"></param>
        ''' <returns></returns>
        Private Function IndivisualTags(input As String) As List(Of String)
            Dim b As New List(Of String)()
            Dim s As New StringBuilder(input)

            ' ������Ҫ�Ƴ���CSS��ע���ı�
            For Each block As Match In r.Matches(input, CommentBlock, RegexICSng)
                Call s.Replace(block.Value, "")
            Next

            For Each m As Match In r.Matches(s.ToString, IndivisualTagsPattern)
                b += m.Value.StripBlank
            Next

            Return b
        End Function

        <Extension>
        Private Iterator Function GetProperty(input As String) As IEnumerable(Of (key$, value$))
            Dim s As String() = r.Split(input, "[;]")

            For Each b As String In s
                If b.StringEmpty Then
                    Continue For
                End If

                Dim t As String() = r.Split(b, "[:]")

                If t.Length = 2 Then
                    Dim propertyName$ = Nothing, propertyValue$ = Nothing

                    If t(0) <> "" Then
                        propertyName = RemoveWhitespace(t(0))
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
            Return New String(input.ToCharArray().Where(Function(c) Not [Char].IsWhiteSpace(c)).ToArray())
        End Function

        Private Function RemoveWitespaceFormStartAndEnd(input As String) As String
            input = input.Trim()
            input = input.TrimEnd()
            Return input
        End Function
    End Module
End Namespace
