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
Imports Microsoft.VisualBasic.MIME.application.netCDF.HDF5.IO
Imports BinaryReader = Microsoft.VisualBasic.MIME.application.netCDF.HDF5.IO.BinaryReader

Namespace HDF5.[Structure]

    Public Class DataspaceMessage
        Private m_address As Long
        Private m_version As Integer
        Private m_numberOfDimensions As Integer
        Private m_flags As Byte
        Private m_type As Integer
        Private m_dimensionLength As Integer()
        Private m_maxDimensionLength As Integer()

        Public Sub New([in] As BinaryReader, sb As Superblock, address As Long)
            [in].offset = address

            Me.m_address = address

            Me.m_version = [in].readByte()

            If Me.m_version = 1 Then
                Me.m_numberOfDimensions = [in].readByte()
                Me.m_flags = [in].readByte()
                Me.m_type = If(Me.m_numberOfDimensions = 0, 0, 1)
                [in].skipBytes(5)
            ElseIf Me.m_version = 2 Then
                Me.m_numberOfDimensions = [in].readByte()
                Me.m_flags = [in].readByte()
                Me.m_type = [in].readByte()
            Else
                Throw New IOException("unknown version")
            End If

            Me.m_dimensionLength = New Integer(Me.m_numberOfDimensions - 1) {}
            For i As Integer = 0 To Me.m_numberOfDimensions - 1
                Me.m_dimensionLength(i) = CInt(ReadHelper.readL([in], sb))
            Next

            Dim hasMax As Boolean = ((Me.m_flags And &H1) <> 0)
            Me.m_maxDimensionLength = New Integer(Me.m_numberOfDimensions - 1) {}
            If hasMax Then
                For i As Integer = 0 To Me.m_numberOfDimensions - 1
                    Me.m_maxDimensionLength(i) = CInt(ReadHelper.readL([in], sb))
                Next
            Else
                For i As Integer = 0 To Me.m_numberOfDimensions - 1
                    Me.m_maxDimensionLength(i) = Me.m_dimensionLength(i)
                Next
            End If
        End Sub

        Public Overridable ReadOnly Property address() As Long
            Get
                Return Me.m_address
            End Get
        End Property

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

        Public Overridable ReadOnly Property flags() As Byte
            Get
                Return Me.m_flags
            End Get
        End Property

        Public Overridable ReadOnly Property type() As Integer
            Get
                Return Me.m_type
            End Get
        End Property

        Public Overridable ReadOnly Property dimensionLength() As Integer()
            Get
                Return Me.m_dimensionLength
            End Get
        End Property

        Public Overridable ReadOnly Property maxDimensionLength() As Integer()
            Get
                Return Me.m_maxDimensionLength
            End Get
        End Property

        Public Overridable Sub printValues()
            Console.WriteLine("DataspaceMessage >>>")
            Console.WriteLine("address : " & Me.m_address)
            Console.WriteLine("version : " & Me.m_version)
            Console.WriteLine("number of dimensions : " & Me.m_numberOfDimensions)
            Console.WriteLine("flags : " & Me.m_flags)
            Console.WriteLine("type : " & Me.m_type)

            Console.WriteLine("DataspaceMessage <<<")
        End Sub
    End Class

End Namespace
