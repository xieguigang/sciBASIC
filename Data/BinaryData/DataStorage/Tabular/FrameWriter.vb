Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.Math.DataFrame
Imports Microsoft.VisualBasic.Scripting.Runtime
Imports Microsoft.VisualBasic.Serialization.JSON
Imports Microsoft.VisualBasic.ValueTypes

Public Module FrameWriter

    Public ReadOnly magic As IReadOnlyCollection(Of Byte) = Encoding.ASCII.GetBytes("dataframe")

    ''' <summary>
    ''' write dataframe object as the binary file
    ''' </summary>
    ''' <param name="df"></param>
    ''' <param name="file"></param>
    ''' <returns></returns>
    <Extension>
    Public Function WriteFrame(df As DataFrame, file As Stream) As Boolean
        Dim wr As New BinaryDataWriter(file) With {.ByteOrder = ByteOrder.BigEndian}
        Dim offset As Long
        Dim metadata As New Schema(df)

        Call wr.Write(DirectCast(magic, Byte()))
        ' offsets for the metadata
        Call wr.Write(0&)

        For Each name As String In metadata.ordinals
            Dim v As FeatureVector = df.features(name)

            offset = wr.Position
            metadata(name).offset = offset

            If v.isScalar Then
                Call WriteScalar(wr, v.GetScalarValue, metadata(name).type)
            End If
        Next

        offset = wr.Position

        ' write metadata offset at the begining and ends of stream
        Call wr.Write(metadata.GetJson)
        Call wr.Write(offset)
        Call wr.Flush()
        Call wr.Seek(magic.Count, SeekOrigin.Begin)
        Call wr.Write(offset)
        Call wr.Flush()

        Return True
    End Function

    Private Sub WriteScalar(wr As BinaryDataWriter, obj As Object, code As TypeCode)
        If obj Is Nothing OrElse code = TypeCode.DBNull OrElse code = TypeCode.Empty Then
            Call wr.Write(0%)
            Return
        Else
            Call wr.Write(1%)
        End If

        Select Case code
            Case TypeCode.Boolean : If CBool(obj) Then wr.Write(CByte(1)) Else wr.Write(CByte(0))
            Case TypeCode.Byte : wr.Write(CByte(obj))
            Case TypeCode.Char : wr.Write(AscW(CChar(obj)))
            Case TypeCode.DateTime : wr.Write(CDate(obj).UnixTimeStamp)
            Case TypeCode.Decimal : wr.Write(CDec(obj))
            Case TypeCode.Double : wr.Write(CDbl(obj))
            Case TypeCode.Int16 : wr.Write(CShort(obj))
            Case TypeCode.Int32 : wr.Write(CInt(obj))
            Case TypeCode.Int64 : wr.Write(CLng(obj))
            Case TypeCode.SByte : wr.Write(CSByte(obj))
            Case TypeCode.Single : wr.Write(CSng(obj))
            Case TypeCode.String : wr.Write(CStr(obj))
            Case TypeCode.UInt16 : wr.Write(CUShort(obj))
            Case TypeCode.UInt32 : wr.Write(CUInt(obj))
            Case TypeCode.UInt64 : wr.Write(CULng(obj))

            Case Else
                Throw New NotImplementedException(code.ToString)
        End Select
    End Sub

End Module
