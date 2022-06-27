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

    End Function

    <Extension>
    Public Iterator Function GetTypeRegistry(buffer As Stream) As IEnumerable(Of NamedValue(Of Integer))

    End Function

    <Extension>
    Public Function Pack(file As StreamObject, type As Index(Of String)) As Byte()
        Dim attrs = file.attributes.SafeQuery.ToArray
        Dim size As Integer
        Dim valType As Type
        Dim typeCode As Integer
        Dim buf As Byte()

        Using ms As New MemoryStream, bin As New BinaryDataWriter(ms)
            Call bin.Write(file.description, BinaryStringFormat.ZeroTerminated)
            Call bin.Write(attrs.Length)

            For Each tuple As KeyValuePair(Of String, Object) In attrs
                Call bin.Write(tuple.Key)

                If tuple.Value Is Nothing Then
                    ' null has no type code
                    Call bin.Write(-1)
                    ' null has no data size
                    Call bin.Write(0)
                Else
                    valType = tuple.Value.GetType
                    typeCode = type(valType.FullName)

                    ' write type code
                    Call bin.Write(typeCode)
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
End Module
