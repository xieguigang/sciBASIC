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