Namespace Xdr

    Public Class Unpacker : Implements IByteReader

        ReadOnly data As BinaryDataReader

        Sub New(data As BinaryDataReader)
            Me.data = data
        End Sub

        Public Sub set_position(position As Integer)
            data.Position = position
        End Sub

        Public Function get_position() As Integer
            Return data.Position
        End Function

        Public Function unpack_int() As Object
            Return XdrEncoding.DecodeInt32(Me)
        End Function

        Public Function unpack_double() As Object
            Return XdrEncoding.DecodeDouble(Me)
        End Function

        Public Function Read(count As UInteger) As Byte() Implements IByteReader.Read
            Return data.ReadBytes(count)
        End Function

        Public Function Read() As Byte Implements IByteReader.Read
            Return data.ReadByte
        End Function
    End Class
End Namespace