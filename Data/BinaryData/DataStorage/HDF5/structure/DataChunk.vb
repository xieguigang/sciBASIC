#Region "Microsoft.VisualBasic::5ef785fffc50d2c6262d9ce380b7a9a1, Data\BinaryData\DataStorage\HDF5\structure\DataChunk.vb"

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

'     Class DataChunk
' 
'         Properties: filePosition, filterMask, offsets, size
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

    ''' <summary>
    ''' 所存储的数据块
    ''' </summary>
    Public Class DataChunk : Inherits HDF5Ptr

        Public Overridable ReadOnly Property size() As Integer
        Public Overridable ReadOnly Property filterMask() As Integer
        Public Overridable ReadOnly Property offsets() As Integer()
        Public Overridable ReadOnly Property filePosition() As Long

        Friend Sub New([in] As BinaryReader, sb As Superblock, address As Long, numberOfDimensions As Integer, last As Boolean)
            Call MyBase.New(address)

            [in].offset = address

            Me.size = [in].readInt()
            Me.filterMask = [in].readInt()
            Me.offsets = New Integer(numberOfDimensions - 1) {}

            For i As Integer = 0 To numberOfDimensions - 1
                Me.offsets(i) = CInt([in].readLong())
            Next

            Me.filePosition = If(last, -1, ReadHelper.readO([in], sb))
        End Sub

        Protected Friend Overrides Sub printValues(console As TextWriter)
            console.WriteLine("DataChunk >>>")
            console.WriteLine("address : " & Me.m_address)
            console.WriteLine("size : " & Me.size)
            console.WriteLine("filter mask : " & Me.filterMask)

            If Me.offsets IsNot Nothing Then
                For i As Integer = 0 To Me.offsets.Length - 1
                    console.WriteLine("offsets[" & i & "] : " & Me.offsets(i))
                Next
            End If

            console.WriteLine("file position : " & Me.filePosition)

            console.WriteLine("DataChunk <<<")
        End Sub
    End Class

End Namespace
