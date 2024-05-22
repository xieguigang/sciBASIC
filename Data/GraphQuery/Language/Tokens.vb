#Region "Microsoft.VisualBasic::6c00bb058e870dbfd472ef1bd8eff3f7, Data\GraphQuery\Language\Tokens.vb"

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
    '    Code Lines: 13 (56.52%)
    ' Comment Lines: 9 (39.13%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 1 (4.35%)
    '     File Size: 400 B


    '     Enum Tokens
    ' 
    '         close, comma, comment, NA, open
    '         pipeline, symbol, terminator, text
    ' 
    '  
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Language

    Public Enum Tokens
        NA
        symbol
        open
        close
        ''' <summary>
        ''' "
        ''' </summary>
        text
        ''' <summary>
        ''' |
        ''' </summary>
        pipeline
        ''' <summary>
        ''' #
        ''' </summary>
        comment
        comma
        terminator
    End Enum
End Namespace
