#Region "Microsoft.VisualBasic::a9f7994422a79eae387ae40ab63a50ed, Data\GraphQuery\TextParser\BaseInvoke.vb"

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

    '   Total Lines: 218
    '    Code Lines: 135
    ' Comment Lines: 54
    '   Blank Lines: 29
    '     File Size: 9.10 KB


    '     Module BaseInvoke
    ' 
    '         Function: filter, getText, match, regexp, regexpParseText
    '                   regexpReplace, replace, split, strip, tagValue
    '                   text, (+2 Overloads) trim
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Language.C
Imports Microsoft.VisualBasic.MIME.Html.Document
Imports Microsoft.VisualBasic.MIME.Html.Language.CSS
Imports Microsoft.VisualBasic.Net.Http
Imports Microsoft.VisualBasic.Text

Namespace TextParser

    Module BaseInvoke

        ''' <summary>
        ''' Extract the text of the current node
        ''' </summary>
        ''' <param name="document"></param>
        ''' <param name="parameters"></param>
        ''' <param name="isArray"></param>
        ''' <returns></returns>
        <ExportAPI("text")>
        Public Function text(document As InnerPlantText, parameters As String(), isArray As Boolean) As InnerPlantText
            Return ParserFunction.ParseDocument(document, getText, isArray)
        End Function

        Private Function getText() As IParserPipeline
            Return Function(i)
                       If TypeOf i Is HtmlElement Then
                           Return New InnerPlantText With {
                               .InnerText = i.GetPlantText
                           }
                       Else
                           Return i
                       End If
                   End Function
        End Function

        ''' <summary>
        ''' Clear the extra spaces and line breaks internal the text content string
        ''' </summary>
        ''' <param name="document"></param>
        ''' <param name="parameters"></param>
        ''' <param name="isArray"></param>
        ''' <returns></returns>
        <ExportAPI("strip")>
        Public Function strip(document As InnerPlantText, parameters As String(), isArray As Boolean) As InnerPlantText
            Return ParserFunction.ParseDocument(
                document, Function(i)
                              Dim text As String = i.GetPlantText
                              text = text.StringReplace("[\s\r\n\t]+", " ")
                              Return New InnerPlantText With {.InnerText = text}
                          End Function,
                isArray:=isArray
            )
        End Function

        ''' <summary>
        ''' Clear spaces and line breaks before and after the string
        ''' </summary>
        ''' <param name="document"></param>
        ''' <param name="parameters"></param>
        ''' <param name="isArray"></param>
        ''' <returns></returns>
        <ExportAPI("trim")>
        Public Function trim(document As InnerPlantText, parameters As String(), isArray As Boolean) As InnerPlantText
            Return ParserFunction.ParseDocument(document, trim(parameters), isArray)
        End Function

        Private Function trim(parameters As String()) As IParserPipeline
            Dim chars As Char()

            If parameters.IsNullOrEmpty Then
                chars = {" "c, ASCII.TAB, ASCII.CR, ASCII.LF}
            Else
                chars = parameters _
                    .JoinBy("") _
                    .ReplaceMetaChars _
                    .ToArray
            End If

            Return Function(i)
                       If i Is Nothing Then
                           Return New InnerPlantText With {.InnerText = ""}
                       Else
                           Dim text As String = i.GetPlantText

                           text = Strings.Trim(text)
                           text = text.Trim(chars)

                           Return New InnerPlantText With {.InnerText = text}
                       End If
                   End Function
        End Function

        ''' <summary>
        ''' split text by a given list of chars
        ''' </summary>
        ''' <param name="document"></param>
        ''' <param name="parameters"></param>
        ''' <param name="isArray"></param>
        ''' <returns></returns>
        <ExportAPI("split")>
        Public Function split(document As InnerPlantText, parameters As String(), isArray As Boolean) As InnerPlantText
            Dim chars As Char() = parameters(Scan0).ReplaceMetaChars.ToArray
            Dim n As Integer = Integer.Parse(parameters.ElementAtOrDefault(1, "0"))

            Return ParserFunction.ParseDocument(
                document:=document,
                pip:=Function(i)
                         Dim text As String = i.GetPlantText
                         Dim parts As String() = text.Split(chars)

                         Return New InnerPlantText With {.InnerText = parts.ElementAtOrDefault(n)}
                     End Function,
                isArray:=isArray
            )
        End Function

        ''' <summary>
        ''' removes of the text string that matched the pattern of given regexp list
        ''' </summary>
        ''' <param name="document"></param>
        ''' <param name="parameters"></param>
        ''' <param name="isArray"></param>
        ''' <returns></returns>
        <ExportAPI("filter")>
        Public Function filter(document As InnerPlantText, parameters As String(), isArray As Boolean) As InnerPlantText
            Dim patterns As Regex() = parameters _
                .Select(Function(p) New Regex(p)) _
                .ToArray

        End Function

        ''' <summary>
        ''' match the string collection with given regexp pattern
        ''' </summary>
        ''' <param name="document"></param>
        ''' <param name="parameters"></param>
        ''' <param name="isArray"></param>
        ''' <returns></returns>
        <ExportAPI("match")>
        Public Function match(document As InnerPlantText, parameters As String(), isArray As Boolean) As InnerPlantText

        End Function

        <ExportAPI("replace")>
        Public Function replace(document As InnerPlantText, parameters As String(), isArray As Boolean) As InnerPlantText
            Dim pattern As String = parameters(Scan0)
            Dim replaceAs As String = parameters(1)
            Dim r As New Regex(pattern)

            Return ParserFunction.ParseDocument(document, regexpReplace(r, replaceAs), isArray)
        End Function

        Private Function regexpReplace(r As Regex, replaceAs As String) As IParserPipeline
            Return Function(i)
                       Dim text As String = i.GetPlantText
                       Dim list As String() = r.Matches(text).ToArray

                       For Each str As String In list
                           text = text.Replace(str, replaceAs)
                       Next

                       Return New InnerPlantText With {.InnerText = text}
                   End Function
        End Function

        ''' <summary>
        ''' parse the sub-string with given regexp pattern
        ''' </summary>
        ''' <param name="document"></param>
        ''' <param name="parameters">
        ''' [regexp, opts]
        ''' </param>
        ''' <param name="isArray"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' https://developer.mozilla.org/en-US/docs/Web/JavaScript/Guide/Regular_Expressions#advanced_searching_with_flags
        ''' </remarks>
        <ExportAPI("regexp")>
        Public Function regexp(document As InnerPlantText, parameters As String(), isArray As Boolean) As InnerPlantText
            Dim options As String = parameters.ElementAtOrDefault(1)
            Dim pattern As String = parameters(Scan0)
            Dim opts As RegexOptions = RegexOptions.Compiled
            Dim r As New Regex(pattern, opts)

            Return ParserFunction.ParseDocument(document, regexpParseText(r), isArray)
        End Function

        Private Function regexpParseText(r As Regex) As IParserPipeline
            Return Function(i)
                       Dim text As String = i.GetPlantText
                       text = r.Match(text).Value
                       Return New InnerPlantText With {.InnerText = text}
                   End Function
        End Function

        <ExportAPI("tagValue")>
        Public Function tagValue(document As InnerPlantText, parameters As String(), isArray As Boolean) As InnerPlantText
            Dim delimiter As String = parameters(Scan0)
            Dim partName As String = Strings.LCase(parameters.ElementAtOrDefault(1, "value"))

            Return ParserFunction.ParseDocument(
                document:=document,
                pip:=Function(i)
                         Dim text As String = i.GetPlantText
                         Dim data = text.GetTagValue(delimiter, trim:=True)

                         If partName = "value" Then
                             Return New InnerPlantText With {.InnerText = data.Value}
                         Else
                             Return New InnerPlantText With {.InnerText = data.Name}
                         End If
                     End Function,
                isArray:=isArray
            )
        End Function
    End Module
End Namespace
