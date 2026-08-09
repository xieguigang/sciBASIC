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