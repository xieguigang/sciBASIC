Imports System.IO
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.IO
Imports Microsoft.VisualBasic.Data.IO.MessagePack
Imports Microsoft.VisualBasic.DataStorage.HDSPack.FileSystem
Imports Microsoft.VisualBasic.Linq

Module PackAttributeData

    <Extension>
    Public Function GetTypeCodes(registry As Index(Of String)) As Byte()
        Using ms As New MemoryStream, bin As New BinaryDataWriter(ms)
            Call bin.Write(registry.Count)

            For Each code As SeqValue(Of String) In registry
                Call bin.Write(code.i)
                Call bin.Write(code.value, BinaryStringFormat.ZeroTerminated)
            Next

            Call bin.Flush()

            Return ms.ToArray
        End Using
    End Function

    <Extension>
    Public Iterator Function GetTypeRegistry(buffer As Stream) As IEnumerable(Of NamedValue(Of Integer))
        Using bin As New BinaryDataReader(buffer)
            Dim n As Integer = bin.ReadInt32
            Dim code As Integer
            Dim type As String

            For i As Integer = 0 To n - 1
                code = bin.ReadInt32
                type = bin.ReadString(BinaryStringFormat.ZeroTerminated)

                Yield New NamedValue(Of Integer) With {
                    .Name = type,
                    .Value = code
                }
            Next
        End Using
    End Function

    <Extension>
    Public Function Pack(file As StreamObject, type As Index(Of String)) As Byte()
        Dim attrs = file.attributes.SafeQuery.ToArray
        Dim size As Integer
        Dim valType As Type
        Dim typeCode As Integer
        Dim buf As Byte()

        Using ms As New MemoryStream, bin As New BinaryDataWriter(ms)
            Call bin.Write(attrs.Length)
            Call bin.Write(If(file.description, ""), BinaryStringFormat.ZeroTerminated)

            For Each tuple As KeyValuePair(Of String, Object) In attrs
                Call bin.Write(tuple.Key, BinaryStringFormat.ZeroTerminated)

                If tuple.Value Is Nothing Then
                    ' null has no type code
                    Call bin.Write(-1)
                    ' null has no data size
                    Call bin.Write(0)
                Else
                    valType = tuple.Value.GetType
                    typeCode = type(valType.FullName)

                    ' write type code
                    bin.Write(typeCode)
                    ' pack via messagepack
                    buf = MsgPackSerializer.SerializeObject(tuple.Value)
                    size = buf.Length
                    bin.Write(size)
                    bin.Write(buf)
                End If
            Next

            Call bin.Flush()

            Return ms.ToArray
        End Using
    End Function

    <Extension>
    Public Function UnPack(buf As Stream, ByRef desc As String, registry As Dictionary(Of String, String)) As Dictionary(Of String, Object)
        Using bin As New BinaryDataReader(buf)
            Dim n As Integer = bin.ReadInt32
            Dim attrs As New Dictionary(Of String, Object)
            Dim type As Type

            desc = bin.ReadString(BinaryStringFormat.ZeroTerminated)

            For i As Integer = 0 To n - 1
                Dim key As String = bin.ReadString(BinaryStringFormat.ZeroTerminated)
                Dim code As Integer = bin.ReadInt32
                Dim buffer As Byte()
                Dim size As Integer = bin.ReadInt32
                Dim value As Object

                If code < 0 Then
                    attrs.Add(key, Nothing)
                Else
                    buffer = bin.ReadBytes(size)
                    type = Type.GetType(registry(code.ToString))
                    value = MsgPackSerializer.Deserialize(type, buffer)
                    attrs.Add(key, value)
                End If
            Next

            Return attrs
        End Using
    End Function
End Module
