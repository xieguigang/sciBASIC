#Region "Microsoft.VisualBasic::6b0582f0d57d18c3f9dc593b2cbf3c72, Data\BinaryData\HDF5\structure\DataObjects\Headers\Messages\DataTypeMessage.vb"

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

    '   Total Lines: 221
    '    Code Lines: 158
    ' Comment Lines: 30
    '   Blank Lines: 33
    '     File Size: 9.38 KB


    '     Class DataTypeMessage
    ' 
    '         Properties: byteOrder, byteSize, reader, structureMembers, type
    '                     version
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: ToString
    ' 
    '         Sub: printValues
    ' 
    ' 
    ' /********************************************************************************/

#End Region

'
' * Mostly copied from NETCDF4 source code.
' * refer : http://www.unidata.ucar.edu
' * 
' * Modified by iychoi@email.arizona.edu
' 

Imports System.IO
Imports System.Text
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.Data.IO.HDF5.device
Imports Microsoft.VisualBasic.Data.IO.HDF5.type
Imports Microsoft.VisualBasic.Linq
Imports BinaryReader = Microsoft.VisualBasic.Data.IO.HDF5.device.BinaryReader

Namespace struct.messages

    ''' <summary>
    ''' The datatype message defines the datatype for each element of a dataset or 
    ''' a common datatype for sharing between multiple datasets. A datatype can 
    ''' describe an atomic type like a fixed- or floating-point type or more complex 
    ''' types like a C struct (compound datatype), array (array datatype), or C++ 
    ''' vector (variable-length datatype).
    '''
    ''' Datatype messages that are part Of a dataset Object Do Not describe how 
    ''' elements are related To one another; the dataspace message Is used For that 
    ''' purpose. Datatype messages that are part Of a committed datatype (formerly 
    ''' named datatype) message describe a common datatype that can be Shared by 
    ''' multiple datasets In the file.
    ''' </summary>
    Public Class DataTypeMessage : Inherits Message

        Dim m_flags As Byte()
        Dim m_unsigned As Boolean
        Dim m_timeTypeByteSize As Integer
        Dim m_opaqueDesc As String
        Dim m_referenceType As Integer
        Dim m_isOK As Boolean
        Dim m_base As DataTypeMessage
        Dim m_classBits As BitSet

        Dim encoding As Encoding

        Public ReadOnly Property version As Integer
        Public ReadOnly Property byteOrder As ByteOrder
        Public ReadOnly Property type As DataTypes
        Public ReadOnly Property byteSize As Integer
        Public ReadOnly Property structureMembers As List(Of StructureMember)

        Public ReadOnly Property reader As DataType

        Public Sub New(sb As Superblock, address As Long)
            Call MyBase.New(address)

            Dim [in] As BinaryReader = sb.FileReader(address)

            ' common base constructor
            Dim tandv As Byte = [in].readByte()

            Me.type = CType(tandv And &HF, DataTypes)
            Me.version = ((tandv And &HF0) >> 4)

            Me.m_flags = [in].readBytes(3)
            Me.m_classBits = BitSet.ValueOf(m_flags.Take(2).ToArray)
            Me.byteSize = [in].readInt()
            Me.byteOrder = If((Me.m_flags(0) And &H1) = 0, ByteOrder.LittleEndian, ByteOrder.BigEndian)
            Me.m_timeTypeByteSize = 4
            Me.m_isOK = True

            If Me.type = DataTypes.DATATYPE_FIXED_POINT Then
                Me.m_unsigned = ((Me.m_flags(0) And &H8) = 0)

                Dim bitOffset As Short = [in].readShort()
                Dim bitPrecision As Short = [in].readShort()

                Me.m_isOK = (bitOffset = 0) AndAlso (bitPrecision Mod 8 = 0)
                Me.reader = New FixedPoint With {
                    .bitOffset = bitOffset,
                    .bitPrecision = bitPrecision,
                    .[class] = DataTypes.DATATYPE_FIXED_POINT,
                    .byteOrder = byteOrder,
                    .signed = Not m_unsigned,
                    .size = byteSize,
                    .version = version,
                    .lowPadding = m_classBits.Get(1),
                    .highPadding = m_classBits.Get(2)
                }
            ElseIf Me.type = DataTypes.DATATYPE_FLOATING_POINT Then
                Dim bitOffset As Short = [in].readShort()
                Dim bitPrecision As Short = [in].readShort()
                Dim expLocation As Byte = [in].readByte()
                Dim expSize As Byte = [in].readByte()
                Dim manLocation As Byte = [in].readByte()
                Dim manSize As Byte = [in].readByte()
                Dim expBias As Integer = [in].readInt()

                Me.reader = New FloatingPoint With {
                    .version = version,
                    .bitOffset = bitOffset,
                    .bitPrecision = bitPrecision,
                    .byteOrder = byteOrder,
                    .[class] = DataTypes.DATATYPE_FLOATING_POINT,
                    .exponentBias = expBias,
                    .exponentLocation = expLocation,
                    .exponentSize = expSize,
                    .mantissaLocation = manLocation,
                    .mantissaSize = manSize,
                    .size = byteSize
                }
            ElseIf Me.type = DataTypes.DATATYPE_TIME Then
                Dim bitPrecision As Short = [in].readShort()
                Me.m_timeTypeByteSize = bitPrecision \ 8
            ElseIf Me.type = DataTypes.DATATYPE_STRING Then
                Dim ptype As Integer = Me.m_flags(0) And &HF
            ElseIf Me.type = DataTypes.DATATYPE_BIT_FIELD Then
                Dim bitOffset As Short = [in].readShort()
                Dim bitPrecision As Short = [in].readShort()
            ElseIf Me.type = DataTypes.DATATYPE_OPAQUE Then
                Dim len As Byte = Me.m_flags(0)
                Me.m_opaqueDesc = If((len > 0), [in].readASCIIString(len).Trim(), Nothing)
            ElseIf Me.type = DataTypes.DATATYPE_COMPOUND Then
                Dim nmembers As Integer = (Me.m_flags(1) * 256) + Me.m_flags(0)
                Me.structureMembers = New List(Of StructureMember)()

                For i As Integer = 0 To nmembers - 1
                    Me.structureMembers.Add(New StructureMember(sb, [in].offset, Me.version, Me.byteSize))
                Next
            ElseIf Me.type = DataTypes.DATATYPE_REFERENCE Then
                Me.m_referenceType = Me.m_flags(0) And &HF
            ElseIf Me.type = DataTypes.DATATYPE_ENUMS Then
                Dim nMembers As Integer = ReadHelper.bytesToUnsignedInt(Me.m_flags(1), Me.m_flags(0))
                Me.m_base = New DataTypeMessage(sb, [in].offset)
                ' base type
                ' read the enums
                Dim enumName As String() = New String(nMembers - 1) {}

                For i As Integer = 0 To nMembers - 1
                    If Me.version < 3 Then
                        enumName(i) = ReadHelper.readString8([in])
                    Else
                        ' padding
                        enumName(i) = [in].readASCIIString()
                        ' no padding
                    End If
                Next

                ' read the values; must switch to base byte order (!)
                If Not Me.m_base.byteOrder = ByteOrder.LittleEndian Then
                    [in].SetByteOrder(ByteOrder.BigEndian)
                End If

                Dim enumValue As Integer() = New Integer(nMembers - 1) {}
                For i As Integer = 0 To nMembers - 1
                    enumValue(i) = CInt(ReadHelper.readVariableSizeUnsigned([in], Me.m_base.byteSize))
                Next
                ' assume size is 1, 2, or 4

                Dim map As New Dictionary(Of Integer?, String)
                For i As Integer = 0 To nMembers - 1
                    map.Add(enumValue(i), enumName(i))
                Next

                [in].SetByteOrder(ByteOrder.LittleEndian)

                Me.reader = New EnumDataType With {
                    .BaseType = m_base.reader,
                    .[class] = DataTypes.DATATYPE_ENUMS,
                    .EnumMapping = map,
                    .size = -1,
                    .version = version
                }

            ElseIf Me.type = DataTypes.DATATYPE_VARIABLE_LENGTH Then
                Dim paddingType = [in].readInt
                Dim charEncoding = [in].readInt

                If charEncoding = 0 Then
                    encoding = Encoding.ASCII
                ElseIf charEncoding = 1 Then
                    encoding = Encoding.UTF8
                Else
                    Throw New NotImplementedException
                End If

                Me.reader = New VariableLength With {
                    .[class] = Me.type,
                    .encoding = encoding,
                    .version = version,
                    .paddingType = paddingType,
                    .size = byteSize
                }

            ElseIf Me.type = DataTypes.DATATYPE_ARRAY Then
                Throw New Exception("data type array is not implemented")
            End If
        End Sub

        ''' <summary>
        ''' Tostring of <see cref="type"/>
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Function ToString() As String
            Return type.ToString
        End Function

        Protected Friend Overrides Sub printValues(console As TextWriter)
            console.WriteLine("DataTypeMessage >>>")
            console.WriteLine("address : " & Me.m_address)
            console.WriteLine("data type : " & Me.type)
            console.WriteLine("byteSize : " & Me.byteSize)

            For Each mem As StructureMember In Me.structureMembers.SafeQuery
                mem.printValues(console)
            Next

            console.WriteLine("DataTypeMessage <<<")
        End Sub

    End Class

End Namespace
