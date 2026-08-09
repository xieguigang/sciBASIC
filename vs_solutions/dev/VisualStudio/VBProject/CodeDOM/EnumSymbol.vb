Imports Microsoft.VisualBasic.Scripting.MetaData

Namespace VBProj.CodeDOM

    Public Class EnumSymbol : Inherits TypeContainerSymbol
        Public Overrides ReadOnly Property Type As SymbolType = SymbolType.[Enum]
        ''' <summary>
        ''' the underlying base type of an enum, e.g. Enum X As Long
        ''' </summary>
        Public Property EnumBaseType As TypeInfo
    End Class
End Namespace