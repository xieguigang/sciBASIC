#Region "Microsoft.VisualBasic::ce075806382293939df3455aa5debe0c, Microsoft.VisualBasic.Core\Text\Xml\XmlEntity.vb"

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

    '     Module XmlEntity
    ' 
    '         Function: EscapingXmlEntity, UnescapeHTML, UnescapingXmlEntity
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Text
Imports System.Web

Namespace Text.Xml

    ''' <summary>
    ''' https://en.wikipedia.org/wiki/List_of_XML_and_HTML_character_entity_references
    ''' </summary>
    Public Module XmlEntity

        Public Function EscapingXmlEntity(str As String) As String
            Return New StringBuilder(str) _
                .Replace("&", "&amp;") _
                .Replace("""", "&quot;") _
                .Replace("'", "&apos;") _
                .Replace("<", "&lt;") _
                .Replace(">", "&gt;") _
                .ToString
        End Function

        Public Function UnescapingXmlEntity(str As String) As String
            Return New StringBuilder(str) _
                .Replace("&quot;", """") _
                .Replace("&apos;", "'") _
                .Replace("&lt;", "<") _
                .Replace("&gt;", ">") _
                .Replace("&amp;", "&") _
                .ToString
        End Function

        ReadOnly invalidUtf8Escapes$() = {
            "&#x0;", "&#x1;", "&#x2;", "&#x3;", "&#x4;", "&#x5;", "&#x6;", "&#x7;", "&#x8;", "&#x9;",
            "&#xa;", "&#xb;", "&#xc;", "&#xd;", "&#xe;", "&#xf;",
            "&#x10;", "&#x11;", "&#x12;", "&#x13;", "&#x14;", "&#x15;", "&#x16;", "&#x17;", "&#x18;", "&#x19;",
            "&#x1a;", "&#x1b;", "&#x1c;", "&#x1d;", "&#x1e;", "&#x1f;"
        }

        ''' <summary>
        ''' Removes all of the invalid utf8 xml code from the given string.
        ''' </summary>
        ''' <param name="str"></param>
        ''' <returns></returns>
        Public Function StripInvalidUTF8Code(str As String) As String
            With New StringBuilder(str)
                For Each c As String In invalidUtf8Escapes
                    Call .Replace(c, "")
                Next

                Return .ToString
            End With
        End Function

        ''' <summary>
        ''' 处理HTML之中的特殊符号的转义
        ''' </summary>
        ''' <param name="html"></param>
        ''' <returns></returns>
        <Extension> Public Function UnescapeHTML(html As String) As String
            Using writer As New StringWriter()
                ' Decode the encoded string.
                HttpUtility.HtmlDecode(html, writer)
                Return writer.ToString()
            End Using
        End Function
    End Module
End Namespace
