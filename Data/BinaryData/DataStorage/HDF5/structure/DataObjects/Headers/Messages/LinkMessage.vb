#Region "Microsoft.VisualBasic::7367c84468d034f0a25c838897013588, Data\BinaryData\DataStorage\HDF5\structure\DataObjects\Headers\Messages\LinkMessage.vb"

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

    '     Class LinkMessage
    ' 
    '         Properties: creationOrder, encoding, flags, link, linkAddress
    '                     linkName, linkType, version
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


Imports Microsoft.VisualBasic.Data.IO.HDF5.IO

Namespace HDF5.[Structure]

    Public Class LinkMessage : Inherits Message

        Private m_version As Integer
        Private m_flags As Byte
        Private m_encoding As Byte

        ''' <summary>
        ''' This is the link class type and can be one of the following values:
        ''' 
        ''' +  0=hard
        ''' +  1=soft
        ''' + 2-63   Reserved for future HDF5 internal use.
        ''' + 64=external
        ''' + 65-255 Reserved, but available for user-defined link types.
        ''' </summary>
        Private m_linkType As Integer
        ''' <summary>
        ''' This 64-bit value is an index of the link’s creation time within the group. 
        ''' Values start at 0 when the group is created an increment by one for each 
        ''' link added to the group. Removing a link from a group does not change 
        ''' existing links’ creation order field.
        '''
        ''' This field Is present If bit 2 Of Flags Is Set.
        ''' </summary>
        Private m_creationOrder As Long
        Private m_linkName As String
        Private m_link As String
        Private m_linkAddress As Long

        Public Sub New([in] As BinaryReader, sb As Superblock, address As Long)
            Call MyBase.New(address)

            [in].offset = address

            Me.m_version = [in].readByte()
            Me.m_flags = [in].readByte()

            If (Me.m_flags And &H8) <> 0 Then
                Me.m_linkType = [in].readByte()
            End If

            If (Me.m_flags And &H4) <> 0 Then
                Me.m_creationOrder = [in].readLong()
            End If

            If (Me.m_flags And &H10) <> 0 Then
                Me.m_encoding = [in].readByte()
            End If

            Dim linkNameLength As Integer = CInt(ReadHelper.readVariableSizeFactor([in], (Me.m_flags And &H3)))
            Me.m_linkName = [in].readASCIIString(linkNameLength)

            If Me.m_linkType = 0 Then
                ' hard link
                Me.m_linkAddress = ReadHelper.readO([in], sb)
            ElseIf Me.m_linkType = 1 Then
                ' soft link
                Dim len As Integer = [in].readShort()
                Me.m_link = [in].readASCIIString(len)
            ElseIf Me.m_linkType = 64 Then
                ' external
                Dim len As Integer = [in].readShort()
                Me.m_link = [in].readASCIIString(len)
            End If
        End Sub

        Public Overridable ReadOnly Property version() As Integer
            Get
                Return Me.m_version
            End Get
        End Property

        Public Overridable ReadOnly Property flags() As Byte
            Get
                Return Me.m_flags
            End Get
        End Property

        Public Overridable ReadOnly Property encoding() As Byte
            Get
                Return Me.m_encoding
            End Get
        End Property

        Public Overridable ReadOnly Property linkType() As Integer
            Get
                Return Me.m_linkType
            End Get
        End Property

        Public Overridable ReadOnly Property creationOrder() As Long
            Get
                Return Me.m_creationOrder
            End Get
        End Property

        Public Overridable ReadOnly Property linkName() As String
            Get
                Return Me.m_linkName
            End Get
        End Property

        Public Overridable ReadOnly Property link() As String
            Get
                Return Me.m_link
            End Get
        End Property

        Public Overridable ReadOnly Property linkAddress() As Long
            Get
                Return Me.m_linkAddress
            End Get
        End Property

        Public Overridable Sub printValues()
            Console.WriteLine("LinkMessage >>>")

            Console.WriteLine("address : " & Me.m_address)
            Console.WriteLine("version : " & Me.m_version)
            Console.WriteLine("flags : " & Me.m_flags)
            Console.WriteLine("encoding : " & Me.m_encoding)
            Console.WriteLine("link type : " & Me.m_linkType)
            Console.WriteLine("creation order : " & Me.m_creationOrder)
            Console.WriteLine("link name : " & Me.m_linkName)
            Console.WriteLine("link : " & Me.m_link)
            Console.WriteLine("link address : " & Me.m_linkAddress)

            Console.WriteLine("LinkMessage <<<")
        End Sub
    End Class

End Namespace
