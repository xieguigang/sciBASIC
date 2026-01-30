Imports Microsoft.VisualBasic.ValueTypes

Public Module VectorStream

    Public Sub WriteScalar(w As BinaryDataWriter, x As Object, code As TypeCode)
        Select Case code
            Case TypeCode.Boolean : If CBool(x) Then w.Write(CByte(1)) Else w.Write(CByte(0))
            Case TypeCode.Byte : w.Write(CByte(x))
            Case TypeCode.Char : w.Write(AscW(CChar(x)))
            Case TypeCode.DateTime : w.Write(CDate(x).UnixTimeStamp)
            Case TypeCode.Decimal : w.Write(CDec(x))
            Case TypeCode.Double : w.Write(CDbl(x))
            Case TypeCode.Int16 : w.Write(CShort(x))
            Case TypeCode.Int32 : w.Write(CInt(x))
            Case TypeCode.Int64 : w.Write(CLng(x))
            Case TypeCode.SByte : w.Write(CSByte(x))
            Case TypeCode.Single : w.Write(CSng(x))
            Case TypeCode.String : w.Write(CStr(x), BinaryStringFormat.DwordLengthPrefix)
            Case TypeCode.UInt16 : w.Write(CUShort(x))
            Case TypeCode.UInt32 : w.Write(CUInt(x))
            Case TypeCode.UInt64 : w.Write(CULng(x))

            Case Else
                Throw New NotImplementedException(code.ToString)
        End Select
    End Sub

    Public Sub WriteVector(w As BinaryDataWriter, v As Array, code As TypeCode)
        Select Case code
            Case TypeCode.Boolean : w.Write(DirectCast(v, Boolean()).Select(Function(f) CByte(If(f, 1, 0))).ToArray)
            Case TypeCode.Byte : w.Write(DirectCast(v, Byte()))
            Case TypeCode.Char : w.Write(DirectCast(v, Char()).Select(AddressOf AscW).ToArray)
            Case TypeCode.DateTime : w.Write(DirectCast(v, Date()).Select(Function(d) d.UnixTimeStamp).ToArray)
            Case TypeCode.Decimal : w.Write(DirectCast(v, Decimal()))
            Case TypeCode.Double : w.Write(DirectCast(v, Double()))
            Case TypeCode.Int16 : w.Write(DirectCast(v, Int16()))
            Case TypeCode.Int32 : w.Write(DirectCast(v, Int32()))
            Case TypeCode.Int64 : w.Write(DirectCast(v, Int64()))
            Case TypeCode.SByte : w.Write(DirectCast(v, SByte()).Select(Function(b) CByte(b)).ToArray)
            Case TypeCode.Single : w.Write(DirectCast(v, Single()))
            Case TypeCode.UInt16 : w.Write(DirectCast(v, UInt16()))
            Case TypeCode.UInt32 : w.Write(DirectCast(v, UInt32()))
            Case TypeCode.UInt64 : w.Write(DirectCast(v, UInt64()))
            Case TypeCode.String

                For Each str As String In DirectCast(v, String())
                    Call w.Write(str, BinaryStringFormat.DwordLengthPrefix)
                Next

            Case Else
                Throw New NotImplementedException(code.ToString)
        End Select
    End Sub

    Public Function ReadVector(r As BinaryDataReader, type As TypeCode, len As Integer) As Array
        Select Case type
            Case TypeCode.Boolean : Return r.ReadBytes(len).Select(Function(b) b <> 0).ToArray
            Case TypeCode.Byte : Return r.ReadBytes(len)
            Case TypeCode.Char : Return r.ReadInt32s(len).Select(Function(i) ChrW(i)).ToArray
            Case TypeCode.DateTime : Return r.ReadDoubles(len).Select(Function(d) DateTimeHelper.FromUnixTimeStamp(d)).ToArray
            Case TypeCode.Decimal : Return r.ReadDecimals(len)
            Case TypeCode.Double : Return r.ReadDoubles(len)
            Case TypeCode.Int16 : Return r.ReadInt16s(len)
            Case TypeCode.Int32 : Return r.ReadInt32s(len)
            Case TypeCode.Int64 : Return r.ReadInt64s(len)
            Case TypeCode.SByte : Return r.ReadBytes(len).Select(Function(b) CSByte(b)).ToArray
            Case TypeCode.Single : Return r.ReadSingles(len)
            Case TypeCode.String

                Dim strs As String() = New String(len - 1) {}

                For i As Integer = 0 To strs.Length - 1
                    strs(i) = r.ReadString(BinaryStringFormat.DwordLengthPrefix)
                Next

                Return strs

            Case TypeCode.UInt16 : Return r.ReadUInt16s(len)
            Case TypeCode.UInt32 : Return r.ReadUInt32s(len)
            Case TypeCode.UInt64 : Return r.ReadUInt64s(len)

            Case Else
                Throw New NotImplementedException(type.ToString)
        End Select
    End Function

    Public Function ReadScalar(r As BinaryDataReader, type As TypeCode) As Object
        Select Case type
            Case TypeCode.Boolean : Return r.ReadByte() <> 0
            Case TypeCode.Byte : Return r.ReadByte()
            Case TypeCode.Char : Return ChrW(r.ReadInt32)
            Case TypeCode.DateTime : Return DateTimeHelper.FromUnixTimeStamp(r.ReadDouble)
            Case TypeCode.Decimal : Return r.ReadDecimal()
            Case TypeCode.Double : Return r.ReadDouble()
            Case TypeCode.Int16 : Return r.ReadInt16()
            Case TypeCode.Int32 : Return r.ReadInt32()
            Case TypeCode.Int64 : Return r.ReadInt64()
            Case TypeCode.SByte : Return r.ReadSByte()
            Case TypeCode.Single : Return r.ReadSingle()
            Case TypeCode.String : Return r.ReadString(BinaryStringFormat.DwordLengthPrefix)
            Case TypeCode.UInt16 : Return r.ReadUInt16()
            Case TypeCode.UInt32 : Return r.ReadUInt32()
            Case TypeCode.UInt64 : Return r.ReadUInt64()

            Case Else
                Throw New NotImplementedException(type.ToString)
        End Select
    End Function
End Module
