''' <summary>
''' Mimic the full CLI namespace and naming so that this library can be used
''' as a drop-in replacement and/or linked file with both frameworks as needed.
''' </summary>

Namespace Serialization

    Public Enum NilImplication
        MemberDefault
        Null
        Prohibit
    End Enum
End Namespace
