Namespace Language

    ' Some advanced features are not supported in .NET 4.0

#If NET_40 = 1 Then

    <AttributeUsage(AttributeTargets.Parameter, AllowMultiple:=False, Inherited:=True)>
    Public Class CallerMemberName : Inherits Attribute
    End Class

    Public Interface IReadOnlyDictionary(Of K, V) : Inherits IDictionary(Of K, V)
    End Interface

    Public Interface IReadOnlyCollection(Of T) : Inherits ICollection(Of T)
    End Interface
#End If
End Namespace