Namespace Xdr

    Public Class Unpacker

        ReadOnly data As BinaryDataReader

        Sub New(data As BinaryDataReader)
            Me.data = data
        End Sub

        Public Sub set_position(position As Integer)
            Throw New NotImplementedException()
        End Sub

        Public Function get_position() As Integer
            Throw New NotImplementedException()
        End Function

        Public Function unpack_int() As Object
            Throw New NotImplementedException()
        End Function

        Public Function unpack_double() As Object
            Throw New NotImplementedException()
        End Function
    End Class
End Namespace