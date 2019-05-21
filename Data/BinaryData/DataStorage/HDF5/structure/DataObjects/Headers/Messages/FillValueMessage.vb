#Region "Microsoft.VisualBasic::c21d8147bcf146e82dd32739a8141a0a, mime\application%netcdf\HDF5\structure\FillValueMessage.vb"

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
    '         Properties: address, fillWriteTime, flags, hasFillValue, size
    '                     spaceAllocateTime, value, version
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

    Public Class FillValueMessage : Inherits Message

        Private m_version As Integer
        Private m_spaceAllocateTime As Integer
        Private m_flags As Integer
        Private m_fillWriteTime As Integer
        Private m_hasFillValue As Boolean
        Private m_size As Integer
        Private m_value As Byte()

        Public Sub New([in] As BinaryReader, sb As Superblock, address As Long)
            Call MyBase.New(address)

            [in].offset = address

            Me.m_version = [in].readByte()

            If Me.m_version < 3 Then
                Me.m_spaceAllocateTime = [in].readByte()
                Me.m_fillWriteTime = [in].readByte()

                Me.m_hasFillValue = ([in].readByte() <> 0)
            Else
                Me.m_flags = [in].readByte()
                Me.m_spaceAllocateTime = CByte(Me.m_flags And 3)
                Me.m_fillWriteTime = CByte((Me.m_flags >> 2) And 3)
                Me.m_hasFillValue = (Me.m_flags And 32) <> 0
            End If

            If Me.m_hasFillValue Then
                Me.m_size = [in].readInt()
                If Me.m_size > 0 Then
                    Me.m_value = [in].readBytes(Me.m_size)
                    Me.m_hasFillValue = True
                Else
                    Me.m_hasFillValue = False
                End If
            End If
        End Sub

        Public Overridable ReadOnly Property version() As Integer
            Get
                Return Me.m_version
            End Get
        End Property

        Public Overridable ReadOnly Property spaceAllocateTime() As Integer
            Get
                Return Me.m_spaceAllocateTime
            End Get
        End Property

        Public Overridable ReadOnly Property flags() As Integer
            Get
                Return Me.m_flags
            End Get
        End Property

        Public Overridable ReadOnly Property fillWriteTime() As Integer
            Get
                Return Me.m_fillWriteTime
            End Get
        End Property

        Public Overridable ReadOnly Property hasFillValue() As Boolean
            Get
                Return Me.m_hasFillValue
            End Get
        End Property

        Public Overridable ReadOnly Property size() As Integer
            Get
                Return Me.m_size
            End Get
        End Property

        Public Overridable ReadOnly Property value() As Byte()
            Get
                Return Me.m_value
            End Get
        End Property

        Public Overridable Sub printValues()
            Console.WriteLine("FillValueMessage >>>")
            Console.WriteLine("address : " & Me.m_address)
            Console.WriteLine("version : " & Me.m_version)
            Console.WriteLine("space allocate time : " & Me.m_spaceAllocateTime)
            Console.WriteLine("flags : " & Me.m_flags)
            Console.WriteLine("fill write time : " & Me.m_fillWriteTime)
            Console.WriteLine("has fill value : " & Me.m_hasFillValue)
            Console.WriteLine("size : " & Me.m_size)

            Console.WriteLine("FillValueMessage <<<")
        End Sub
    End Class

End Namespace
