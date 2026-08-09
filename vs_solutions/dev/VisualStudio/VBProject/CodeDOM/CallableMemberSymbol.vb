Imports Microsoft.VisualBasic.Scripting.MetaData

Namespace VBProj.CodeDOM

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
End Namespace