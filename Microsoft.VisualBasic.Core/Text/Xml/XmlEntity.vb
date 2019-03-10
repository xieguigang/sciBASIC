#Region "Microsoft.VisualBasic::9e4e3602842c44e1963ea436d21283f4, Microsoft.VisualBasic.Core\Text\Xml\XmlEntity.vb"

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
    '         Constructor: (+1 Overloads) Sub New
    '         Function: EscapingXmlEntity, StripInvalidUTF8Code, UnescapeHTML, UnescapingXmlEntity
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

        Sub New()
            invalidUtf8Escapes = invalidUtf8Escapes.AsList + invalidUtf8Escapes.Select(Function(c) c.ToUpper.Replace("X", "x"))
        End Sub

        ''' <summary>
        ''' Removes all of the invalid utf8 xml code from the given string.
        ''' (使用这个函数删除xml文本字符串之中的无效的UTF8转义字符，这个函数对转义字符的大小写不敏感)
        ''' </summary>
        ''' <param name="str"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' http://7is7.com/software/chars.html
        ''' </remarks>
        <Extension>
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
