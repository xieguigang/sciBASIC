#Region "Microsoft.VisualBasic::134cd04d8fe5e0bfb735046fad059463, mime\application%netcdf\HDF5\structure\DataObjects\Headers\Messages\DataspaceMessage.vb"

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

    '     Class DataspaceMessage
    ' 
    '         Properties: address, dimensionLength, flags, maxDimensionLength, numberOfDimensions
    '                     type, version
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

    Public Class DataspaceMessage : Inherits Message

        Public Overridable ReadOnly Property version As Integer
        Public Overridable ReadOnly Property numberOfDimensions As Integer
        Public Overridable ReadOnly Property flags As Byte
        Public Overridable ReadOnly Property type As Integer
        Public Overridable ReadOnly Property dimensionLength As Integer()
        Public Overridable ReadOnly Property maxDimensionLength As Integer()

        Public Sub New([in] As BinaryReader, sb As Superblock, address As Long)
            Call MyBase.New(address)

            [in].offset = address

            Me.version = [in].readByte()

            If Me.version = 1 Then
                Me.numberOfDimensions = [in].readByte()
                Me.flags = [in].readByte()
                Me.type = If(Me.numberOfDimensions = 0, 0, 1)
                [in].skipBytes(5)
            ElseIf Me.version = 2 Then
                Me.numberOfDimensions = [in].readByte()
                Me.flags = [in].readByte()
                Me.type = [in].readByte()
            Else
                Throw New IOException("unknown version")
            End If

            Me.dimensionLength = New Integer(Me.numberOfDimensions - 1) {}
            For i As Integer = 0 To Me.numberOfDimensions - 1
                Me.dimensionLength(i) = CInt(ReadHelper.readL([in], sb))
            Next

            Dim hasMax As Boolean = ((Me.flags And &H1) <> 0)

            Me.maxDimensionLength = New Integer(Me.numberOfDimensions - 1) {}

            If hasMax Then
                For i As Integer = 0 To Me.numberOfDimensions - 1
                    Me.maxDimensionLength(i) = CInt(ReadHelper.readL([in], sb))
                Next
            Else
                For i As Integer = 0 To Me.numberOfDimensions - 1
                    Me.maxDimensionLength(i) = Me.dimensionLength(i)
                Next
            End If
        End Sub

        Public Overridable Sub printValues()
            Console.WriteLine("DataspaceMessage >>>")
            Console.WriteLine("address : " & Me.m_address)
            Console.WriteLine("version : " & Me.version)
            Console.WriteLine("number of dimensions : " & Me.numberOfDimensions)
            Console.WriteLine("flags : " & Me.flags)
            Console.WriteLine("type : " & Me.type)

            Console.WriteLine("DataspaceMessage <<<")
        End Sub
    End Class

End Namespace
