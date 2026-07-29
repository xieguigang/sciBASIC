#Region "Microsoft.VisualBasic::f7b03b7cfc7334e11c579f58e3a8ec91, vs_solutions\dev\VisualStudio\VBProject\SymbolType.vb"

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
    '    Code Lines: 18 (81.82%)
    ' Comment Lines: 3 (13.64%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 1 (4.55%)
    '     File Size: 962 B


    '     Enum SymbolType
    ' 
    ' 
    '  
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace VBProj

    Public Enum SymbolType
        ' --- Type containers: declarations that can nest types and members ---
        [Namespace] ' - Namespace XXX
        [Class]     ' - Class XXX
        [Module]    ' - Module XXX
        [Structure] ' - Structure XXX
        [Enum]      ' - Enum XXX
        [Interface] ' - Interface XXX
        ' --- Type members: declarations that live inside a type ---
        [New]       ' - Sub New()
        [Function]  ' - Function AAA(x As XX) As XXX
        [Sub]       ' - Sub AAA(x As XX)
        [Operator]  ' - Operator +(x As X, y As Y) As XX
        [Property]  ' - Property X As XX
        [Event]     ' - Event AAA(x As XX)
        [Delegate]  ' - Public Delegate Function AAA(x As XX) As XXX
        ' --- Variables: fields and local variables (Field is intentionally merged) ---
        Variable    ' - Dim X As XX / Public X As XX (field or local variable)
    End Enum
End Namespace
