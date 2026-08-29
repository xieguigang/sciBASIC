#Region "Microsoft.VisualBasic::763f5659e80a30531a6ed2ec1f10ab32, Microsoft.VisualBasic.Core\src\ApplicationServices\Terminal\MarkdownRender\Theme.vb"

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

    '   Total Lines: 44
    '    Code Lines: 22 (50.00%)
    ' Comment Lines: 17 (38.64%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 5 (11.36%)
    '     File Size: 1.74 KB


    '     Class MarkdownTheme
    ' 
    '         Properties: [Global], BlockQuote, Bold, CodeBlock, HeaderSpan
    '                     HorizontalRule, InlineCodeSpan, Italy, LinkText, ListMarker
    '                     StrikeThrough, Table, Url
    ' 
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ApplicationServices.Terminal.TablePrinter.Flags
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace ApplicationServices.Terminal

    Public Class MarkdownTheme

        Public Property Url As ConsoleFormat
        Public Property InlineCodeSpan As ConsoleFormat
        Public Property CodeBlock As ConsoleFormat
        Public Property BlockQuote As ConsoleFormat
        Public Property [Global] As ConsoleFormat
        Public Property Bold As ConsoleFormat
        Public Property Italy As ConsoleFormat
        Public Property HeaderSpan As ConsoleFormat
        ''' <summary>
        ''' the ``~~deleted~~`` inline span, the strike-through text decoration
        ''' is applied via the ``SGR 9`` ansi escape code.
        ''' </summary>
        ''' <returns></returns>
        Public Property StrikeThrough As ConsoleFormat
        ''' <summary>
        ''' the display text of the ``[text](url)`` link span
        ''' </summary>
        ''' <returns></returns>
        Public Property LinkText As ConsoleFormat
        ''' <summary>
        ''' the bullet marker of the list item, e.g. the ``-``, ``+`` or ``*`` symbol
        ''' </summary>
        ''' <returns></returns>
        Public Property ListMarker As ConsoleFormat
        ''' <summary>
        ''' the ``---``/``***``/``___`` horizontal rule line
        ''' </summary>
        ''' <returns></returns>
        Public Property HorizontalRule As ConsoleFormat
        Public Property Table As ConsoleTableBuilderFormat = ConsoleTableBuilderFormat.Minimal

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function

    End Class
End Namespace
