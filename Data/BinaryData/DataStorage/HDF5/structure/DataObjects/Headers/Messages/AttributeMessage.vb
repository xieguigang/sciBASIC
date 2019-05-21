#Region "Microsoft.VisualBasic::bc6994f13851c1b403c96876958e3854, mime\application%netcdf\HDF5\structure\AttributeMessage.vb"

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

    '     Class AttributeMessage
    ' 
    '         Properties: address, dataPos, dataSpace, dataType, name
    '                     version
    ' 
    '         Constructor: (+1 Overloads) Sub New
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
Imports Microsoft.VisualBasic.Data.IO.HDF5.IO
Imports BinaryReader = Microsoft.VisualBasic.Data.IO.HDF5.IO.BinaryReader


Namespace HDF5.[Structure]

    Public Class AttributeMessage : Inherits HDF5Ptr

        Private m_version As Integer
        Private m_name As String
        Private m_dataTypeMessage As DataTypeMessage
        Private m_dataspaceMessage As DataspaceMessage
        Private m_dataPos As Long

        Public Sub New([in] As BinaryReader, sb As Superblock, address As Long)
            Call MyBase.New(address)

            [in].offset = address

            Dim nameSize As Short, typeSize As Short, spaceSize As Short
            Dim flags As Byte = 0
            Dim encoding As Byte = 0
            ' 0 = ascii, 1 = UTF-8
            Me.m_version = [in].readByte()

            If Me.m_version = 1 Then
                [in].skipBytes(1)

                nameSize = [in].readShort()
                typeSize = [in].readShort()
                spaceSize = [in].readShort()
            ElseIf (Me.m_version = 2) OrElse (Me.m_version = 3) Then
                flags = [in].readByte()
                nameSize = [in].readShort()
                typeSize = [in].readShort()
                spaceSize = [in].readShort()

                If Me.m_version = 3 Then
                    encoding = [in].readByte()
                End If
            Else
                Throw New IOException("version error")
            End If

            ' read the attribute name
            Dim filePos As Long = [in].offset
            Me.m_name = [in].readASCIIString(nameSize)
            ' read at current pos
            If Me.m_version = 1 Then
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
                Me.m_dataTypeMessage = New DataTypeMessage([in], sb, [in].offset)
                If Me.m_version = 1 Then
                    typeSize += CShort(ReadHelper.padding(typeSize, 8))
                End If
            End If

            [in].offset = filePos + typeSize
            ' make it more robust for errors
            ' read the dataspace
            filePos = [in].offset
            Me.m_dataspaceMessage = New DataspaceMessage([in], sb, [in].offset)

            If Me.m_version = 1 Then
                spaceSize += CShort(ReadHelper.padding(spaceSize, 8))
            End If
            [in].offset = filePos + spaceSize
            ' make it more robust for errors
            ' the data starts immediately afterward - ie in the message
            ' note this is absolute position (no
            Me.m_dataPos = [in].offset
        End Sub

        Public Overridable ReadOnly Property version() As Integer
            Get
                Return Me.m_version
            End Get
        End Property

        Public Overridable ReadOnly Property name() As String
            Get
                Return Me.m_name
            End Get
        End Property

        Public Overridable ReadOnly Property dataPos() As Long
            Get
                Return Me.m_dataPos
            End Get
        End Property

        Public Overridable ReadOnly Property dataType() As DataTypeMessage
            Get
                Return Me.m_dataTypeMessage
            End Get
        End Property

        Public Overridable ReadOnly Property dataSpace() As DataspaceMessage
            Get
                Return Me.m_dataspaceMessage
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return $"[{MyBase.ToString}] Dim {name} As {dataType} = &{dataPos}"
        End Function

        Public Overridable Sub printValues()
            Console.WriteLine("AttributeMessage >>>")
            Console.WriteLine("address : " & Me.m_address)
            Console.WriteLine("version : " & Me.m_version)
            Console.WriteLine("name : " & Me.m_name)

            If Me.m_dataTypeMessage IsNot Nothing Then
                Me.m_dataTypeMessage.printValues()
            End If

            If Me.m_dataspaceMessage IsNot Nothing Then
                Me.m_dataspaceMessage.printValues()
            End If

            Console.WriteLine("data pos : " & Me.m_dataPos)

            Console.WriteLine("AttributeMessage <<<")
        End Sub
    End Class

End Namespace
