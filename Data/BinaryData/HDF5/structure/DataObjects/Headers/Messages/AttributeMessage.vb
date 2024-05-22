#Region "Microsoft.VisualBasic::4f201a4684cccb42f1f8c49e0f2f4061, Data\BinaryData\HDF5\structure\DataObjects\Headers\Messages\AttributeMessage.vb"

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

    '   Total Lines: 156
    '    Code Lines: 104 (66.67%)
    ' Comment Lines: 17 (10.90%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 35 (22.44%)
    '     File Size: 5.67 KB


    '     Class AttributeMessage
    ' 
    '         Properties: dataPos, dataSpace, dataType, name, reader
    '                     version
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: ReadAttrValue, ToString
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
Imports Microsoft.VisualBasic.Data.IO.HDF5.device
Imports Microsoft.VisualBasic.Data.IO.HDF5.type
Imports BinaryReader = Microsoft.VisualBasic.Data.IO.HDF5.device.BinaryReader


Namespace struct.messages

    Public Class AttributeMessage : Inherits Message

        Public ReadOnly Property version As Integer
        Public ReadOnly Property name As String
        Public ReadOnly Property dataPos As Long
        Public ReadOnly Property dataType As DataTypeMessage
        Public ReadOnly Property dataSpace As DataspaceMessage

        Public ReadOnly Property reader As DataType
            Get
                Return dataType.reader
            End Get
        End Property

        Public Sub New([in] As BinaryReader, sb As Superblock, address As Long)
            Call MyBase.New(address)

            [in].offset = address

            Dim nameSize As Short, typeSize As Short, spaceSize As Short
            Dim flags As Byte = 0
            Dim encoding As Byte = 0
            ' 0 = ascii, 1 = UTF-8
            Me.version = [in].readByte()

            If Me.version = 1 Then
                [in].skipBytes(1)

                nameSize = [in].readShort()
                typeSize = [in].readShort()
                spaceSize = [in].readShort()
            ElseIf (Me.version = 2) OrElse (Me.version = 3) Then
                flags = [in].readByte()
                nameSize = [in].readShort()
                typeSize = [in].readShort()
                spaceSize = [in].readShort()

                If Me.version = 3 Then
                    encoding = [in].readByte()
                End If
            Else
                Throw New IOException("version error")
            End If

            ' read the attribute name
            Dim filePos As Long = [in].offset

            Me.name = [in].readASCIIString(nameSize)

            ' read at current pos
            If Me.version = 1 Then
                nameSize += CShort(ReadHelper.padding(nameSize, 8))
            End If

            [in].offset = filePos + nameSize

            ' read the datatype
            filePos = [in].offset

            Dim isShared As Boolean = (flags And 1) <> 0

            If isShared Then
                'mdt = getSharedDataObject(MessageType.Datatype).mdt;
                Throw New IOException("shared data object is not implemented")
            Else
                Me.dataType = New DataTypeMessage(sb, [in].offset)

                If Me.version = 1 Then
                    typeSize += CShort(ReadHelper.padding(typeSize, 8))
                End If
            End If

            [in].offset = filePos + typeSize
            ' make it more robust for errors
            ' read the dataspace
            filePos = [in].offset
            dataSpace = New DataspaceMessage(sb, [in].offset)

            If Me.version = 1 Then
                spaceSize += CShort(ReadHelper.padding(spaceSize, 8))
            End If

            [in].offset = filePos + spaceSize
            ' make it more robust for errors
            ' the data starts immediately afterward - ie in the message
            ' note this is absolute position (no
            Me.dataPos = [in].offset
        End Sub

        Public Shared Function ReadAttrValue(msg As AttributeMessage, sb As Superblock) As Object
            Dim type As DataTypeMessage = msg.dataType
            Dim dims As Integer() = msg.dataSpace.dimensionLength
            Dim dataType As DataTypes = type.type

            ' 需要在这里移动文件的读取指针到dataPosition才能够正确的读取globalheap数据
            Call sb.FileReader(msg.dataPos)

            Select Case dataType
                Case DataTypes.DATATYPE_VARIABLE_LENGTH
                    Return VariableLengthDatasetReader.readDataSet(msg.reader, dims, sb, msg.dataPos)
                Case DataTypes.DATATYPE_FIXED_POINT, DataTypes.DATATYPE_FLOATING_POINT
                    Return DatasetReader.readDataset(msg.reader, msg.dataPos, msg.dataSpace, sb, dims)
                Case DataTypes.DATATYPE_ENUMS
                    Dim file As BinaryReader = sb.FileReader(msg.dataPos)
                    Dim buffer As ByteBuffer = file.getBuffer

                    If dims.Length = 0 Then
                        dims = {1}
                    End If

                    Return EnumDatasetReader.readEnumDataset(msg.reader, buffer, dims)
                Case Else
                    Throw New NotImplementedException(dataType.ToString)
            End Select
        End Function

        Public Overrides Function ToString() As String
            Return $"[{MyBase.ToString}] Dim {name} As {dataType} = &{dataPos}"
        End Function

        Protected Friend Overrides Sub printValues(console As TextWriter)
            console.WriteLine("AttributeMessage >>>")
            console.WriteLine("address : " & Me.m_address)
            console.WriteLine("version : " & Me.version)
            console.WriteLine("name : " & Me.name)

            If Me.dataType IsNot Nothing Then
                Me.dataType.printValues(console)
            End If

            If Me.dataSpace IsNot Nothing Then
                Me.dataSpace.printValues(console)
            End If

            console.WriteLine("data pos : " & Me.dataPos)

            console.WriteLine("AttributeMessage <<<")
        End Sub
    End Class

End Namespace
