Namespace VBProj.CodeDOM

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