#Region "Microsoft.VisualBasic::a249cbfd088eee29283c6b83305092e5, Data\BinaryData\DataStorage\HDF5\structure\DataObjects\Headers\Messages\FillValueMessage.vb"

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

    '     Class FillValueMessage
    ' 
    '         Properties: fillWriteTime, flags, hasFillValue, size, spaceAllocateTime
    '                     value, version
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
Imports BinaryReader = Microsoft.VisualBasic.Data.IO.HDF5.device.BinaryReader

Namespace HDF5.struct.messages

    ''' <summary>
    ''' The fill value message stores a single data value which is returned to the application when 
    ''' an uninitialized data element is read from a dataset. The fill value is interpreted with the 
    ''' same datatype as the dataset.
    ''' </summary>
    Public Class FillValueMessage : Inherits Message

        Public ReadOnly Property version() As Integer
        Public ReadOnly Property spaceAllocateTime() As Integer
        Public ReadOnly Property flags() As Integer
        Public ReadOnly Property fillWriteTime() As Integer
        Public ReadOnly Property hasFillValue() As Boolean
        Public ReadOnly Property size() As Integer
        Public ReadOnly Property value As Byte()

        Public Sub New(sb As Superblock, address As Long)
            Call MyBase.New(address)

            Dim [in] As BinaryReader = sb.FileReader(address)

            Me.version = [in].readByte()

            If Me.version < 3 Then
                Me.spaceAllocateTime = [in].readByte()
                Me.fillWriteTime = [in].readByte()

                Me.hasFillValue = ([in].readByte() <> 0)
            Else
                Me.flags = [in].readByte()
                Me.spaceAllocateTime = CByte(Me.flags And 3)
                Me.fillWriteTime = CByte((Me.flags >> 2) And 3)
                Me.hasFillValue = (Me.flags And 32) <> 0
            End If

            If Me.hasFillValue Then
                Me.size = [in].readInt()

                If Me.size > 0 Then
                    Me.value = [in].readBytes(Me.size)
                    Me.hasFillValue = True
                Else
                    Me.hasFillValue = False
                End If
            End If
        End Sub

        Protected Friend Overrides Sub printValues(console As TextWriter)
            console.WriteLine("FillValueMessage >>>")
            console.WriteLine("address : " & Me.m_address)
            console.WriteLine("version : " & Me.version)
            console.WriteLine("space allocate time : " & Me.spaceAllocateTime)
            console.WriteLine("flags : " & Me.flags)
            console.WriteLine("fill write time : " & Me.fillWriteTime)
            console.WriteLine("has fill value : " & Me.hasFillValue)
            console.WriteLine("size : " & Me.size)

            console.WriteLine("FillValueMessage <<<")
        End Sub
    End Class

End Namespace
