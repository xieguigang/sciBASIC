#Region "Microsoft.VisualBasic::307c19bd81a45b4696fa8c4fa654391c, vs_solutions\dev\VisualStudio\VBProject\CodeDOM\MemberSymbol.vb"

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

    '   Total Lines: 47
    '    Code Lines: 17 (36.17%)
    ' Comment Lines: 16 (34.04%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 14 (29.79%)
    '     File Size: 1.50 KB


    '     Class MemberSymbol
    ' 
    ' 
    ' 
    '     Class PropertySymbol
    ' 
    '         Properties: Type
    ' 
    '     Class EventSymbol
    ' 
    '         Properties: DelegateType, Type
    ' 
    '     Class DelegateSymbol
    ' 
    '         Properties: Parameters, Type, ValueType
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Scripting.MetaData

Namespace VBProj.CodeDOM

    ''' <summary>
    ''' category base: a declaration that lives inside a <see cref="TypeContainerSymbol"/>,
    ''' e.g. method / property / event / delegate / variable (field or local).
    ''' </summary>
    Public MustInherit Class MemberSymbol : Inherits LanguageSymbolType
    End Class

    ''' <summary>
    ''' property declaration, e.g. Property X As XX / Property Item(index As Integer) As XX.
    ''' </summary>
    Public Class PropertySymbol : Inherits CallableMemberSymbol

        Public Overrides ReadOnly Property Type As SymbolType = SymbolType.[Property]

    End Class

    ''' <summary>
    ''' event symbol, e.g. Event XXX(x As XX)
    ''' </summary>
    Public Class EventSymbol : Inherits MemberSymbol

        ''' <summary>
        ''' the delegate type that backs this event
        ''' </summary>
        Public Property DelegateType As TypeInfo

        Public Overrides ReadOnly Property Type As SymbolType = SymbolType.[Event]

    End Class

    ''' <summary>
    ''' delegate declaration, e.g. Public Delegate Function AAA(x As XX) As XXX
    ''' </summary>
    Public Class DelegateSymbol : Inherits MemberSymbol

        Public Overrides ReadOnly Property Type As SymbolType = SymbolType.[Delegate]

        Public Property Parameters As Dictionary(Of String, TypeInfo)
        Public Property ValueType As TypeInfo

    End Class

End Namespace
