#Region "Microsoft.VisualBasic::681a2d753f25c84d641c2616f2cd24e7, Microsoft.VisualBasic.Core\src\ApplicationServices\Terminal\MarkdownRender\Theme.vb"

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

    '   Total Lines: 23
    '    Code Lines: 18 (78.26%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 5 (21.74%)
    '     File Size: 851 B


    '     Class MarkdownTheme
    ' 
    '         Properties: [Global], BlockQuote, Bold, CodeBlock, HeaderSpan
    '                     InlineCodeSpan, Italy, Table, Url
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
        Public Property Table As ConsoleTableBuilderFormat = ConsoleTableBuilderFormat.Minimal

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function

    End Class
End Namespace
