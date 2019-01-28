Namespace org.renjin.hdf5

    Public Class Flags

        Public Overridable ReadOnly Property value() As SByte

        Public Sub New(value As SByte)
            Me.value = value
        End Sub

        Public Overridable Function isSet(bitIndex As Integer) As Boolean
            Return (value And (1 << bitIndex)) <> 0
        End Function

    End Class

End Namespace