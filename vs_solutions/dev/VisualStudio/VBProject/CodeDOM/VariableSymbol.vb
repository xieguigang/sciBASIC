#Region "Microsoft.VisualBasic::e651c40b67ca6e0c829d59fbc49c3a19, vs_solutions\dev\VisualStudio\VBProject\CodeDOM\VariableSymbol.vb"

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

    '   Total Lines: 18
    '    Code Lines: 7 (38.89%)
    ' Comment Lines: 5 (27.78%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 6 (33.33%)
    '     File Size: 662 B


    '     Class VariableSymbol
    ' 
    '         Properties: Type, ValueType
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Scripting.MetaData

Namespace VBProj.CodeDOM


    ''' <summary>
    ''' a variable symbol. Per design, class/structure fields AND local method variables
    ''' share this single type (Field is intentionally not split out). Use <see cref="LanguageSymbolType.Parent"/>
    ''' to tell whether it is a field (Parent is a TypeContainerSymbol) or a local (Parent is a MemberSymbol).
    ''' </summary>
    Public Class VariableSymbol : Inherits MemberSymbol

        Public Property ValueType As TypeInfo

        Public Overrides ReadOnly Property Type As SymbolType = SymbolType.Variable

    End Class
End Namespace
