#Region "Microsoft.VisualBasic::0a7a67f7b133aaf0b487024ed4ffd12f, vs_solutions\dev\VisualStudio\VBProject\LanguageSymbolType.vb"

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

    '   Total Lines: 198
    '    Code Lines: 71 (35.86%)
    ' Comment Lines: 81 (40.91%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 46 (23.23%)
    '     File Size: 7.27 KB


    '     Class LanguageSymbolType
    ' 
    '         Properties: Attributes, GenericTypeArguments, Modifiers, Name, Parent
    '                     XmlDoc
    ' 
    '     Class TypeContainerSymbol
    ' 
    '         Properties: ImplementsInterfaces, InheritsType, InternalNested, Members
    ' 
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
    '     Class InterfaceSymbol
    ' 
    '         Properties: Type
    ' 
    '     Class EnumSymbol
    ' 
    '         Properties: EnumBaseType, Type
    ' 
    '     Class MemberSymbol
    ' 
    ' 
    ' 
    '     Class CallableMemberSymbol
    ' 
    '         Properties: Locals, Parameters, ReturnType
    ' 
    '     Class MethodSymbol
    ' 
    '         Properties: Type
    ' 
    '         Constructor: (+1 Overloads) Sub New
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
    '     Class VariableSymbol
    ' 
    '         Properties: Type, ValueType
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Scripting.MetaData

Namespace VBProj

    ''' <summary>
    ''' the base class of all code symbol types in a VB language project.
    ''' the concrete <see cref="Type"/> is fixed by the derived class so that
    ''' symbol categories (container / member / variable) can never be confused.
    ''' </summary>
    Public MustInherit Class LanguageSymbolType

        ''' <summary>
        ''' the exact kind of this symbol, fixed by the derived class.
        ''' </summary>
        Public MustOverride ReadOnly Property Type As SymbolType

        ''' <summary>
        ''' the symbol name
        ''' </summary>
        Public Property Name As String

        ''' <summary>
        ''' the parent symbol that owns this symbol (a type container or a member).
        ''' </summary>
        Public Property Parent As LanguageSymbolType

        ''' <summary>
        ''' generic type argument for XXX(Of T)
        ''' </summary>
        Public Property GenericTypeArguments As TypeInfo()

        ''' <summary>
        ''' access and custom modifiers, e.g. "Public Shared Overloads"
        ''' </summary>
        Public Property Modifiers As String

        ''' <summary>
        ''' attribute declaration blocks applied on this symbol, e.g. &lt;ExportAPI()&gt;
        ''' </summary>
        Public Property Attributes As List(Of String)

        ''' <summary>
        ''' the xml documentation comment lines (''') that precedes this symbol
        ''' </summary>
        Public Property XmlDoc As String

    End Class

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

    Public Class InterfaceSymbol : Inherits TypeContainerSymbol
        Public Overrides ReadOnly Property Type As SymbolType = SymbolType.[Interface]
    End Class

    Public Class EnumSymbol : Inherits TypeContainerSymbol
        Public Overrides ReadOnly Property Type As SymbolType = SymbolType.[Enum]
        ''' <summary>
        ''' the underlying base type of an enum, e.g. Enum X As Long
        ''' </summary>
        Public Property EnumBaseType As TypeInfo
    End Class

    ''' <summary>
    ''' category base: a declaration that lives inside a <see cref="TypeContainerSymbol"/>,
    ''' e.g. method / property / event / delegate / variable (field or local).
    ''' </summary>
    Public MustInherit Class MemberSymbol : Inherits LanguageSymbolType
    End Class

    ''' <summary>
    ''' base for callable members that own parameters, a return type and a body
    ''' with local variables: <see cref="MethodSymbol"/> and <see cref="PropertySymbol"/>.
    ''' </summary>
    Public MustInherit Class CallableMemberSymbol : Inherits MemberSymbol
        Public Property Parameters As Dictionary(Of String, TypeInfo)
        ''' <summary>
        ''' return type; Nothing for Sub / Sub New / Property without an explicit As clause.
        ''' </summary>
        Public Property ReturnType As TypeInfo
        ''' <summary>
        ''' local variables declared inside the member body (Dim XXX As XXX).
        ''' </summary>
        Public Property Locals As Dictionary(Of String, VariableSymbol)
    End Class

    ''' <summary>
    ''' function / sub / operator / sub new (constructor).
    ''' </summary>
    Public Class MethodSymbol : Inherits CallableMemberSymbol

        Private _type As SymbolType

        Public Overrides ReadOnly Property Type As SymbolType
            Get
                Return _type
            End Get
        End Property

        Public Sub New(type As SymbolType)
            _type = type
        End Sub

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
