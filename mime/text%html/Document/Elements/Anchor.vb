#Region "Microsoft.VisualBasic::44711ee60f2d96e62006107500f6e2dd, mime\text%html\Document\Elements\Anchor.vb"

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

    '   Total Lines: 42
    '    Code Lines: 32 (76.19%)
    ' Comment Lines: 4 (9.52%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 6 (14.29%)
    '     File Size: 1.36 KB


    '     Class Anchor
    ' 
    '         Properties: download, href, hreflang, media, rel
    '                     target, text, title, type
    ' 
    '         Function: FromElement
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.MIME.Html.XmlMeta

Namespace Document

    <XmlType("a")>
    Public Class Anchor : Inherits Node

        Public Property href As String
        Public Property target As String
        Public Property rel As String
        Public Property title As String
        Public Property download As String
        Public Property hreflang As String
        Public Property type As String
        Public Property media As String

        ''' <summary>
        ''' the inner plant/html text of current anchor
        ''' </summary>
        ''' <returns></returns>
        Public Property text As String

        Public Shared Function FromElement(element As HtmlElement) As Anchor
            Return New Anchor With {
                .id = element.id,
                .[class] = element.class,
                .href = element!href,
                .download = element!download,
                .hreflang = element!hreflang,
                .media = element!media,
                .rel = element!rel,
                .style = element!style,
                .target = element!target,
                .title = element!title,
                .type = element!type,
                .text = element.GetPlantText
            }
        End Function

    End Class
End Namespace
