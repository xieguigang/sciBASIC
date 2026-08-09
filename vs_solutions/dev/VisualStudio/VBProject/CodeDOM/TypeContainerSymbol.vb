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
End Namespace