#Region "Microsoft.VisualBasic::ebc08c7215ce057a4b1d3326309dfea7, Microsoft.VisualBasic.Core\Text\Xml\Extensions.vb"

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

    '     Module Extensions
    ' 
    '         Function: CodePage, FormatHTML, GetXmlAttrValue, RemoveXmlComments, SetXmlEncoding
    '                   SetXmlStandalone, TextEncoding
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Text
Imports System.Text.RegularExpressions
Imports System.Xml
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Text
Imports Microsoft.VisualBasic.Text.Xml
Imports r = System.Text.RegularExpressions.Regex

Namespace Text.Xml

    Public Module Extensions

        ''' <summary>
        ''' 使用这个函数将html文档进行格式化，请注意，目标html文档应该是符合xml语法的
        ''' </summary>
        ''' <param name="html$"></param>
        ''' <returns></returns>
        <Extension> Public Function FormatHTML(html$) As String
            Dim xml$ = "<?xml version=""1.0""?>" & html
            Dim doc As New XmlDocument
            Dim ms As New MemoryStream

            Using writer As New StreamWriter(ms, UTF8WithoutBOM)
                Call doc.LoadXml(xml)
                Call doc.Save(writer)
                Call writer.Flush()
            End Using

            Dim out$ = UTF8WithoutBOM _
                .GetString(ms.ToArray) _
                .Trim("?"c)  ' 很奇怪，生成的字符串的开始的位置有一个问号

            Return out
        End Function

        ''' <summary>
        ''' 这个函数可以将Xml/Html文本之中的注释数据进行删除
        ''' </summary>
        ''' <param name="xhtml"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function RemoveXmlComments(xhtml As String) As String
            Return r.Replace(xhtml, "<![-][-].*[-][-]>", "")
        End Function

        ''' <summary>
        ''' Xml encoding to text encoding
        ''' </summary>
        ''' <param name="xmlEncoding"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function CodePage(xmlEncoding As XmlEncodings) As Encoding
            Select Case xmlEncoding
                Case XmlEncodings.GB2312
                    Return Encodings.GB2312.CodePage
                Case XmlEncodings.UTF8
                    Return UTF8WithoutBOM
                Case Else
                    Return Encodings.UTF16.CodePage
            End Select
        End Function

        ''' <summary>
        ''' Xml encoding to text encoding
        ''' </summary>
        ''' <param name="xmlEncoding"></param>
        ''' <returns></returns>
        <Extension>
        Public Function TextEncoding(xmlEncoding As XmlEncodings) As Encodings
            Select Case xmlEncoding
                Case XmlEncodings.GB2312
                    Return Encodings.GB2312
                Case XmlEncodings.UTF16
                    Return Encodings.UTF16
                Case XmlEncodings.UTF8
                    Return Encodings.UTF8WithoutBOM
                Case Else
                    Throw New NotImplementedException
            End Select
        End Function

        <ExportAPI("Xml.GetAttribute")>
        <Extension>
        Public Function GetXmlAttrValue(str As String, Name As String) As String
            Dim m As Match = r.Match(str, Name & "\s*=\s*(("".+?"")|[^ ]*)")

            If Not m.Success Then
                Return ""
            Else
                str = m.Value.GetTagValue("=", trim:=True).Value
            End If

            If str.First = """"c AndAlso str.Last = """"c Then
                str = Mid(str, 2, Len(str) - 2)
            End If

            Return str
        End Function

        <Extension>
        Public Function SetXmlEncoding(xml As String, encoding As XmlEncodings) As String
            Dim xmlEncoding As String = encoding.Description
            Dim head As String = r.Match(xml, XmlDoc.XmlDeclares, RegexICSng).Value
            Dim enc As String = r.Match(head, "encoding=""\S+""", RegexICSng).Value

            If String.IsNullOrEmpty(enc) Then
                enc = head.Replace("?>", $" encoding=""{xmlEncoding}""?>")
            Else
                enc = head.Replace(enc, $"encoding=""{xmlEncoding}""")
            End If

            xml = xml.Replace(head, enc)

            Return xml
        End Function

        <Extension>
        Public Function SetXmlStandalone(xml As String, standalone As Boolean) As String
            Dim opt As String = XmlDeclaration.XmlStandaloneString(standalone)
            Dim head As String = r.Match(xml, XmlDoc.XmlDeclares, RegexICSng).Value
            Dim enc As String = r.Match(head, "standalone=""\S+""", RegexICSng).Value

            If String.IsNullOrEmpty(enc) Then
                enc = head.Replace("?>", $" standalone=""{opt}""?>")
            Else
                enc = head.Replace(enc, $"standalone=""{opt}""")
            End If

            xml = xml.Replace(head, enc)

            Return xml
        End Function
    End Module
End Namespace
