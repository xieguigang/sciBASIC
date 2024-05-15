#Region "Microsoft.VisualBasic::acbd5e08e279dbccb805d2aca62a765c, Data\BinaryData\HDF5\structure\DataObjects\Headers\Messages\LinkMessage.vb"

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

    '   Total Lines: 99
    '    Code Lines: 54
    ' Comment Lines: 26
    '   Blank Lines: 19
    '     File Size: 3.65 KB


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


Imports System.IO
Imports Microsoft.VisualBasic.Data.IO.HDF5.device
Imports BinaryReader = Microsoft.VisualBasic.Data.IO.HDF5.device.BinaryReader

Namespace struct.messages

    Public Class LinkMessage : Inherits Message

        Public ReadOnly Property version As Integer
        Public ReadOnly Property flags As Byte
        Public ReadOnly Property encoding As Byte

        ''' <summary>
        ''' This is the link class type and can be one of the following values:
        ''' 
        ''' +  0=hard
        ''' +  1=soft
        ''' + 2-63   Reserved for future HDF5 internal use.
        ''' + 64=external
        ''' + 65-255 Reserved, but available for user-defined link types.
        ''' </summary>
        Public ReadOnly Property linkType As Integer

        ''' <summary>
        ''' This 64-bit value is an index of the link’s creation time within the group. 
        ''' Values start at 0 when the group is created an increment by one for each 
        ''' link added to the group. Removing a link from a group does not change 
        ''' existing links’ creation order field.
        '''
        ''' This field Is present If bit 2 Of Flags Is Set.
        ''' </summary>
        Public ReadOnly Property creationOrder As Long
        Public ReadOnly Property linkName As String
        Public ReadOnly Property link As String
        Public ReadOnly Property linkAddress As Long

        Public Sub New(sb As Superblock, address As Long)
            Call MyBase.New(address)

            Dim [in] As BinaryReader = sb.FileReader(address)

            Me.version = [in].readByte()
            Me.flags = [in].readByte()

            If (Me.flags And &H8) <> 0 Then
                Me.linkType = [in].readByte()
            End If

            If (Me.flags And &H4) <> 0 Then
                Me.creationOrder = [in].readLong()
            End If

            If (Me.flags And &H10) <> 0 Then
                Me.encoding = [in].readByte()
            End If

            Dim linkNameLength As Integer = CInt(ReadHelper.readVariableSizeFactor([in], (Me.flags And &H3)))
            Me.linkName = [in].readASCIIString(linkNameLength)

            If Me.linkType = 0 Then
                ' hard link
                Me.linkAddress = ReadHelper.readO([in], sb)
            ElseIf Me.linkType = 1 Then
                ' soft link
                Dim len As Integer = [in].readShort()
                Me.link = [in].readASCIIString(len)
            ElseIf Me.linkType = 64 Then
                ' external
                Dim len As Integer = [in].readShort()
                Me.link = [in].readASCIIString(len)
            End If
        End Sub

        Protected Friend Overrides Sub printValues(console As TextWriter)
            console.WriteLine("LinkMessage >>>")

            console.WriteLine("address : " & Me.m_address)
            console.WriteLine("version : " & Me.version)
            console.WriteLine("flags : " & Me.flags)
            console.WriteLine("encoding : " & Me.encoding)
            console.WriteLine("link type : " & Me.linkType)
            console.WriteLine("creation order : " & Me.creationOrder)
            console.WriteLine("link name : " & Me.linkName)
            console.WriteLine("link : " & Me.link)
            console.WriteLine("link address : " & Me.linkAddress)

            console.WriteLine("LinkMessage <<<")
        End Sub
    End Class

End Namespace
