#Region "Microsoft.VisualBasic::01b2330f227cbcaff173d9480966c533, Data\BinaryData\HDSPack\BinaryStream\PackAttributeData.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 133
    '    Code Lines: 106
    ' Comment Lines: 3
    '   Blank Lines: 24
    '     File Size: 4.84 KB


    ' Module PackAttributeData
    ' 
    '     Function: GetTypeCodes, GetTypeRegistry, (+3 Overloads) Pack, UnPack
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.IO
Imports Microsoft.VisualBasic.DataStorage.HDSPack.FileSystem
Imports Microsoft.VisualBasic.Linq

Module PackAttributeData

    <Extension>
    Public Function GetTypeCodes(registry As Index(Of String)) As Byte()
        Using ms As New MemoryStream, bin As New BinaryDataWriter(ms) With {.ByteOrder = ByteOrder.BigEndian}
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
        Using bin As New BinaryDataReader(buffer) With {.ByteOrder = ByteOrder.BigEndian}
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
        Dim desc As String = file.description
        Dim attrs As AttributeMetadata() = file.attributes.ToArray

        Return attrs.Pack(desc, type)
    End Function

    <Extension>
    Public Function Pack(attrs As LazyAttribute, type As Index(Of String)) As Byte()
        Return attrs.ToArray.Pack("", type)
    End Function

    <Extension>
    Public Function Pack(attrs As AttributeMetadata(), description As String, type As Index(Of String)) As Byte()
        Dim size As Integer
        Dim typeCode As Integer
        Dim buf As Byte()

        Using ms As New MemoryStream, bin As New BinaryDataWriter(ms) With {.ByteOrder = ByteOrder.BigEndian}
            Call bin.Write(attrs.Length)
            Call bin.Write(If(description, ""), BinaryStringFormat.ZeroTerminated)

            For Each tuple As AttributeMetadata In attrs
                Call bin.Write(tuple.name, BinaryStringFormat.ZeroTerminated)

                If tuple.data Is Nothing Then
                    ' null has no type code
                    Call bin.Write(-1)
                    ' null has no data size
                    Call bin.Write(0)
                Else
                    typeCode = type(tuple.type)

                    ' write type code
                    bin.Write(typeCode)
                    buf = tuple.data
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
    Public Function UnPack(buf As Stream, ByRef desc As String, registry As Dictionary(Of String, String)) As LazyAttribute
        Using bin As New BinaryDataReader(buf) With {.ByteOrder = ByteOrder.BigEndian}
            Dim n As Integer = bin.ReadInt32
            Dim attrs As New Dictionary(Of String, AttributeMetadata)
            Dim typeName As String

            desc = bin.ReadString(BinaryStringFormat.ZeroTerminated)

            For i As Integer = 0 To n - 1
                Dim key As String = bin.ReadString(BinaryStringFormat.ZeroTerminated)
                Dim code As Integer = bin.ReadInt32
                Dim buffer As Byte()
                Dim size As Integer = bin.ReadInt32
                Dim value As AttributeMetadata

                If code < 0 Then
                    value = New AttributeMetadata With {
                        .name = key,
                        .type = Nothing,
                        .data = Nothing
                    }
                Else
                    buffer = bin.ReadBytes(size)
                    typeName = registry(code.ToString)
                    value = New AttributeMetadata With {
                        .data = buffer,
                        .type = typeName,
                        .name = key
                    }
                End If

                attrs.Add(key, value)
            Next

            Return New LazyAttribute With {.attributes = attrs}
        End Using
    End Function
End Module
