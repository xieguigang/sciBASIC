#Region "Microsoft.VisualBasic::76e77e59f68a200b40c3454bb8c8a7d1, mime\text%html\Parser\HtmlTokens.vb"

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

    '   Total Lines: 22
    '    Code Lines: 9 (40.91%)
    ' Comment Lines: 12 (54.55%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 1 (4.55%)
    '     File Size: 415 B


    '     Enum HtmlTokens
    ' 
    '         closeTag, equalsSymbol, openTag, splash, text
    ' 
    '  
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Language

    Public Enum HtmlTokens
        text
        ''' <summary>
        ''' &lt;
        ''' </summary>
        openTag
        ''' <summary>
        ''' >
        ''' </summary>
        closeTag
        ''' <summary>
        ''' =
        ''' </summary>
        equalsSymbol
        ''' <summary>
        ''' /
        ''' </summary>
        splash
    End Enum
End Namespace
