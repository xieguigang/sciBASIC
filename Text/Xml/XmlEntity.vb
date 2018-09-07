#Region "Microsoft.VisualBasic::31459f33f6f9c714320f72c0603fd935, Microsoft.VisualBasic.Core\Text\Xml\XmlEntity.vb"

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

        <Extension>
        Public Function UnescapeHTML(html As String) As String
            Using writer As New StringWriter()
                ' Decode the encoded string.
                HttpUtility.HtmlDecode(html, writer)
                Return writer.ToString()
            End Using
        End Function
    End Module
End Namespace
