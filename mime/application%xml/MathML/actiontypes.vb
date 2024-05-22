#Region "Microsoft.VisualBasic::7f19812dbb6df6eb1b99f0a2c5d65ec6, mime\application%xml\MathML\actiontypes.vb"

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

    '   Total Lines: 12
    '    Code Lines: 7 (58.33%)
    ' Comment Lines: 4 (33.33%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 1 (8.33%)
    '     File Size: 398 B


    '     Enum actiontypes
    ' 
    '         statusline, toggle, tooltip
    ' 
    '  
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace MathML

    Public Enum actiontypes
        statusline
        toggle
        ''' <summary>
        ''' When the pointer moves over the expression, a tooltip box with a message is displayed near the expression.
        ''' The syntax Is: ``&lt;maction actiontype = "tooltip"> expression message &lt;/maction>``.
        ''' </summary>
        tooltip
    End Enum
End Namespace
