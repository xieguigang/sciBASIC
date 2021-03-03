Namespace Xdr

    Public Module XdrEncoding

        ''' <summary>
        ''' Decodes the Int32.
        ''' http://tools.ietf.org/html/rfc4506#section-4.1
        ''' </summary>
        Public Function DecodeInt32(r As IByteReader) As Integer
            Return (r.Read() << &H18) Or (r.Read() << &H10) Or (r.Read() << &H8) Or r.Read()
        End Function

        ''' <summary>
        ''' Encodes the Int32.
        ''' http://tools.ietf.org/html/rfc4506#section-4.1
        ''' </summary>
        Public Sub EncodeInt32(v As Integer, w As IByteWriter)
            w.Write(v >> &H18 And &HFF)
            w.Write(v >> &H10 And &HFF)
            w.Write(v >> 8 And &HFF)
            w.Write(v And &HFF)
        End Sub

        ''' <summary>
        ''' Decodes the UInt32.
        ''' http://tools.ietf.org/html/rfc4506#section-4.2
        ''' </summary>
        Public Function DecodeUInt32(r As IByteReader) As UInteger
            Return (CUInt(r.Read()) << &H18) Or (CUInt(r.Read()) << &H10) Or (CUInt(r.Read()) << &H8) Or CUInt(r.Read())
        End Function

        ''' <summary>
        ''' Encodes the UInt32.
        ''' http://tools.ietf.org/html/rfc4506#section-4.2
        ''' </summary>
        Public Sub EncodeUInt32(v As UInteger, w As IByteWriter)
            w.Write(v >> &H18 And &HFF)
            w.Write(v >> &H10 And &HFF)
            w.Write(v >> 8 And &HFF)
            w.Write(v And &HFF)
        End Sub

        ''' <summary>
        ''' Decodes the Int64.
        ''' http://tools.ietf.org/html/rfc4506#section-4.5
        ''' </summary>
        Public Function DecodeInt64(r As IByteReader) As Long
            Return (CLng(r.Read()) << 56) Or (CLng(r.Read()) << 48) Or (CLng(r.Read()) << 40) Or (CLng(r.Read()) << 32) Or (CLng(r.Read()) << 24) Or (CLng(r.Read()) << 16) Or (CLng(r.Read()) << 8) Or CLng(r.Read())
        End Function

        ''' <summary>
        ''' Encodes the Int64.
        ''' http://tools.ietf.org/html/rfc4506#section-4.5
        ''' </summary>
        Public Sub EncodeInt64(v As Long, w As IByteWriter)
            w.Write(v >> 56 And &HFF)
            w.Write(v >> 48 And &HFF)
            w.Write(v >> 40 And &HFF)
            w.Write(v >> 32 And &HFF)
            w.Write(v >> 24 And &HFF)
            w.Write(v >> 16 And &HFF)
            w.Write(v >> 8 And &HFF)
            w.Write(v And &HFF)
        End Sub

        ''' <summary>
        ''' Decodes the UInt64.
        ''' http://tools.ietf.org/html/rfc4506#section-4.5
        ''' </summary>
        Public Function DecodeUInt64(r As IByteReader) As ULong
            Return (CULng(r.Read()) << 56) Or (CULng(r.Read()) << 48) Or (CULng(r.Read()) << 40) Or (CULng(r.Read()) << 32) Or (CULng(r.Read()) << 24) Or (CULng(r.Read()) << 16) Or (CULng(r.Read()) << 8) Or CULng(r.Read())
        End Function

        ''' <summary>
        ''' Encodes the UInt64.
        ''' http://tools.ietf.org/html/rfc4506#section-4.5
        ''' </summary>
        Public Sub EncodeUInt64(v As ULong, w As IByteWriter)
            w.Write(v >> 56 And &HFF)
            w.Write(v >> 48 And &HFF)
            w.Write(v >> 40 And &HFF)
            w.Write(v >> 32 And &HFF)
            w.Write(v >> 24 And &HFF)
            w.Write(v >> 16 And &HFF)
            w.Write(v >> 8 And &HFF)
            w.Write(v And &HFF)
        End Sub

        ''' <summary>
        ''' Decodes the Single.
        ''' http://tools.ietf.org/html/rfc4506#section-4.6
        ''' </summary>
        Public Function DecodeSingle(r As Xdr.IByteReader) As Single
            Dim num As Integer = Xdr.XdrEncoding.DecodeInt32(r)
            Return unsafeSingle(num)
        End Function

        ''' <summary>
        ''' Encodes the Single.
        ''' http://tools.ietf.org/html/rfc4506#section-4.6
        ''' </summary>
        Public Sub EncodeSingle(v As Single, w As Xdr.IByteWriter)
            Xdr.XdrEncoding.EncodeInt32(unsafeInteger(v), w)
        End Sub

        ''' <summary>
        ''' Decodes the Double.
        ''' http://tools.ietf.org/html/rfc4506#section-4.7
        ''' </summary>
        Public Function DecodeDouble(r As Xdr.IByteReader) As Double
            Dim num As Long = Xdr.XdrEncoding.DecodeInt64(r)
            Return unsafeDouble(num)
        End Function

        ''' <summary>
        ''' Encodes the Double.
        ''' http://tools.ietf.org/html/rfc4506#section-4.7
        ''' </summary>
        Public Sub EncodeDouble(v As Double, w As Xdr.IByteWriter)
            Xdr.XdrEncoding.EncodeInt64(unsafeLong(v), w)
        End Sub

        Private Function unsafeDouble(x As Long) As Double
            Dim bytes = BitConverter.GetBytes(x)
            Dim dbl As Double = BitConverter.ToDouble(bytes, 0)

            Return dbl
        End Function

        Private Function unsafeSingle(x As Integer) As Single
            Dim bytes = BitConverter.GetBytes(x)
            Dim dbl As Single = BitConverter.ToSingle(bytes, 0)

            Return dbl
        End Function

        Private Function unsafeInteger(x As Single) As Integer
            Dim bytes = BitConverter.GetBytes(x)
            Dim uint As Integer = BitConverter.ToInt32(bytes, 0)

            Return uint
        End Function

        Private Function unsafeLong(x As Double) As ULong
            Dim bytes = BitConverter.GetBytes(x)
            Dim ulng As ULong = BitConverter.ToUInt64(bytes, 0)

            Return ulng
        End Function
    End Module
End Namespace
