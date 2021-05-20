#Region "Microsoft.VisualBasic::c6649073129a2a36d43cdb9e9a35c731, mime\text%html\MarkDown\HTMLGenerator.vb"

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

    '     Module HTMLGenerator
    ' 
    '         Function: ToHTML
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices

Public Module HTMLGenerator

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function Markdown2HTML(markdown$, Optional opt As MarkdownOptions = Nothing) As String
        Return New MarkdownHTML(opt Or MarkdownOptions.DefaultOption).Transform(text:=markdown)
    End Function
End Module
