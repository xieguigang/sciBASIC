#Region "Microsoft.VisualBasic::89c8efe00f7b443c2b6bfacd4fe5e40e, Data\BinaryData\DataStorage\HDF5\structure\DataObjects\Headers\Messages\LayoutMessage.vb"

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
'         Properties: chunkSize, continuousSize, dataAddress, dataSize, numberOfDimensions
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

Imports Microsoft.VisualBasic.Data.IO.HDF5.IO
Imports BinaryReader = Microsoft.VisualBasic.Data.IO.HDF5.IO.BinaryReader

Namespace HDF5.[Structure]

    ''' <summary>
    ''' 
    ''' </summary>
    Public Class LayoutMessage : Inherits Message

        Public Overridable ReadOnly Property version() As Integer
        Public ReadOnly Property type As LayoutClass
        Public Overridable ReadOnly Property numberOfDimensions() As Integer
        Public Overridable ReadOnly Property dataAddress() As Long
        Public Overridable ReadOnly Property continuousSize() As Long
        Public Overridable ReadOnly Property chunkSize() As Integer()
        Public Overridable ReadOnly Property dataSize() As Integer

        Public Sub New([in] As BinaryReader, sb As Superblock, address As Long)
            Call MyBase.New(address)

            [in].offset = address

            Me.version = [in].readByte()

            If Me.version < 3 Then
                Me.numberOfDimensions = [in].readByte()
                Me.type = [in].readByte()

                [in].skipBytes(5)

                Dim isCompact As Boolean = Me.type = 0

                If Not isCompact Then
                    Me.dataAddress = ReadHelper.readO([in], sb)
                End If

                Me.chunkSize = New Integer(Me.numberOfDimensions - 1) {}
                For i As Integer = 0 To Me.numberOfDimensions - 1
                    Me.chunkSize(i) = [in].readInt()
                Next

                If isCompact Then
                    Me.dataSize = [in].readInt()
                    Me.dataAddress = [in].offset
                End If
            Else
                Me.type = CType(CInt([in].readByte), LayoutClass)

                If Me.type = LayoutClass.CompactStorage Then
                    Me.dataSize = [in].readShort()
                    Me.dataAddress = [in].offset
                ElseIf Me.type = LayoutClass.ContiguousStorage Then
                    Me.dataAddress = ReadHelper.readO([in], sb)
                    Me.continuousSize = ReadHelper.readL([in], sb)
                ElseIf Me.type = LayoutClass.ChunkedStorage Then
                    Me.numberOfDimensions = [in].readByte()
                    Me.dataAddress = ReadHelper.readO([in], sb)
                    Me.chunkSize = New Integer(Me.numberOfDimensions - 1) {}

                    For i As Integer = 0 To Me.numberOfDimensions - 1
                        Me.chunkSize(i) = [in].readInt()
                    Next
                End If
            End If
        End Sub

        Protected Friend Overrides Sub printValues(console As System.IO.StringWriter)
            console.WriteLine("LayoutMessage >>>")

            console.WriteLine("address : " & Me.m_address)
            console.WriteLine("version : " & Me.version)
            console.WriteLine("number of dimensions : " & Me.numberOfDimensions)
            console.WriteLine("type : " & Me.type)
            console.WriteLine("data address : " & Me.dataAddress)
            console.WriteLine("continuous size : " & Me.continuousSize)
            console.WriteLine("data size : " & Me.dataSize)

            For i As Integer = 0 To Me.chunkSize.Length - 1
                console.WriteLine("chunk size [" & i & "] : " & Me.chunkSize(i))
            Next

            console.WriteLine("LayoutMessage <<<")
        End Sub
    End Class

End Namespace
