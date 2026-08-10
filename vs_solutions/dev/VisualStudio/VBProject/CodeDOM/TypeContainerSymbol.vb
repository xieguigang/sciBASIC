#Region "Microsoft.VisualBasic::47924202382610d651c6a32073cbaf71, vs_solutions\dev\VisualStudio\VBProject\CodeDOM\TypeContainerSymbol.vb"

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

    '   Total Lines: 34
    '    Code Lines: 9 (26.47%)
    ' Comment Lines: 18 (52.94%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 7 (20.59%)
    '     File Size: 1.38 KB


    '     Class TypeContainerSymbol
    ' 
    '         Properties: ImplementsInterfaces, InheritsType, InternalNested, Members
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Scripting.MetaData

Namespace VBProj.CodeDOM

    ''' <summary>
    ''' category base: a type declaration that can nest other types and members,
    ''' e.g. Namespace / Class / Module / Structure / Enum / Interface.
    ''' </summary>
    Public MustInherit Class TypeContainerSymbol : Inherits LanguageSymbolType

        ''' <summary>
        ''' nested type declarations owned by this container (class/structure/enum/interface/module/namespace).
        ''' </summary>
        Public Property InternalNested As Dictionary(Of String, LanguageSymbolType)

        ''' <summary>
        ''' type members owned by this container (method/property/field/event/delegate).
        ''' NOTE: local variables inside a method/property body live in <see cref="MethodSymbol.Locals"/>
        ''' /<see cref="PropertySymbol.Locals"/>, NOT here, to avoid the "Members" ambiguity.
        ''' </summary>
        Public Property Members As Dictionary(Of String, LanguageSymbolType)

        ''' <summary>
        ''' the base type from the Inherits clause
        ''' </summary>
        Public Property InheritsType As TypeInfo

        ''' <summary>
        ''' the implemented interfaces from the Implements clause
        ''' </summary>
        Public Property ImplementsInterfaces As TypeInfo()

    End Class
End Namespace
