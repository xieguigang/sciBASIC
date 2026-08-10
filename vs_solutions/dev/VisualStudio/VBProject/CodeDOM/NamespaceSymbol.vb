#Region "Microsoft.VisualBasic::95ca19c3bf3947c7c1b3f7824270e49d, vs_solutions\dev\VisualStudio\VBProject\CodeDOM\NamespaceSymbol.vb"

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

    '   Total Lines: 17
    '    Code Lines: 14 (82.35%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 3 (17.65%)
    '     File Size: 703 B


    '     Class NamespaceSymbol
    ' 
    '         Properties: Type
    ' 
    '     Class ClassSymbol
    ' 
    '         Properties: Type
    ' 
    '     Class ModuleSymbol
    ' 
    '         Properties: Type
    ' 
    '     Class StructureSymbol
    ' 
    '         Properties: Type
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace VBProj.CodeDOM
    Public Class NamespaceSymbol : Inherits TypeContainerSymbol
        Public Overrides ReadOnly Property Type As SymbolType = SymbolType.[Namespace]
    End Class

    Public Class ClassSymbol : Inherits TypeContainerSymbol
        Public Overrides ReadOnly Property Type As SymbolType = SymbolType.[Class]
    End Class

    Public Class ModuleSymbol : Inherits TypeContainerSymbol
        Public Overrides ReadOnly Property Type As SymbolType = SymbolType.[Module]
    End Class

    Public Class StructureSymbol : Inherits TypeContainerSymbol
        Public Overrides ReadOnly Property Type As SymbolType = SymbolType.[Structure]
    End Class
End Namespace
