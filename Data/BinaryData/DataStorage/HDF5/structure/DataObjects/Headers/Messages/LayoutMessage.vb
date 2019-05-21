#Region "Microsoft.VisualBasic::08f2172644131e16fc9182fe72e593c9, mime\application%netcdf\HDF5\structure\LayoutMessage.vb"

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

'     Class LayoutMessage
' 
'         Properties: address, chunkSize, continuousSize, dataAddress, dataSize
'                     numberOfDimensions, version
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

    ''' <summary>
    ''' 
    ''' </summary>
    Public Class LayoutMessage : Inherits Message

        Private m_version As Integer
        Private m_numberOfDimensions As Integer
        Private m_type As LayoutClass
        Private m_dataAddress As Long
        Private m_continuousSize As Long
        Private m_chunkSize As Integer()
        Private m_dataSize As Integer

        Public Sub New([in] As BinaryReader, sb As Superblock, address As Long)
            Call MyBase.New(address)

            [in].offset = address

            Me.m_version = [in].readByte()

            If Me.m_version < 3 Then
                Me.m_numberOfDimensions = [in].readByte()
                Me.m_type = [in].readByte()

                [in].skipBytes(5)

                Dim isCompact As Boolean = (Me.m_type = 0)
                If Not isCompact Then
                    Me.m_dataAddress = ReadHelper.readO([in], sb)
                End If

                Me.m_chunkSize = New Integer(Me.m_numberOfDimensions - 1) {}
                For i As Integer = 0 To Me.m_numberOfDimensions - 1
                    Me.m_chunkSize(i) = [in].readInt()
                Next

                If isCompact Then
                    Me.m_dataSize = [in].readInt()
                    Me.m_dataAddress = [in].offset
                End If
            Else
                Me.m_type = CType(CInt([in].readByte), LayoutClass)

                If Me.m_type = LayoutClass.CompactStorage Then
                    Me.m_dataSize = [in].readShort()
                    Me.m_dataAddress = [in].offset
                ElseIf Me.m_type = LayoutClass.ContiguousStorage Then
                    Me.m_dataAddress = ReadHelper.readO([in], sb)
                    Me.m_continuousSize = ReadHelper.readL([in], sb)
                ElseIf Me.m_type = LayoutClass.ChunkedStorage Then
                    Me.m_numberOfDimensions = [in].readByte()
                    Me.m_dataAddress = ReadHelper.readO([in], sb)
                    Me.m_chunkSize = New Integer(Me.m_numberOfDimensions - 1) {}

                    For i As Integer = 0 To Me.m_numberOfDimensions - 1
                        Me.m_chunkSize(i) = [in].readInt()
                    Next
                End If
            End If
        End Sub

        Public Overridable ReadOnly Property version() As Integer
            Get
                Return Me.m_version
            End Get
        End Property

        Public Overridable ReadOnly Property numberOfDimensions() As Integer
            Get
                Return Me.m_numberOfDimensions
            End Get
        End Property

        Public Overridable ReadOnly Property dataAddress() As Long
            Get
                Return Me.m_dataAddress
            End Get
        End Property

        Public Overridable ReadOnly Property continuousSize() As Long
            Get
                Return Me.m_continuousSize
            End Get
        End Property

        Public Overridable ReadOnly Property chunkSize() As Integer()
            Get
                Return Me.m_chunkSize
            End Get
        End Property

        Public Overridable ReadOnly Property dataSize() As Integer
            Get
                Return Me.m_dataSize
            End Get
        End Property

        Public Overridable Sub printValues()
            Console.WriteLine("LayoutMessage >>>")

            Console.WriteLine("address : " & Me.m_address)
            Console.WriteLine("version : " & Me.m_version)
            Console.WriteLine("number of dimensions : " & Me.m_numberOfDimensions)
            Console.WriteLine("type : " & Me.m_type)
            Console.WriteLine("data address : " & Me.m_dataAddress)
            Console.WriteLine("continuous size : " & Me.m_continuousSize)
            Console.WriteLine("data size : " & Me.m_dataSize)

            For i As Integer = 0 To Me.m_chunkSize.Length - 1
                Console.WriteLine("chunk size [" & i & "] : " & Me.m_chunkSize(i))
            Next

            Console.WriteLine("LayoutMessage <<<")
        End Sub
    End Class

End Namespace
