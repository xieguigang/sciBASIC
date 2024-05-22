#Region "Microsoft.VisualBasic::9d7dee6749c4b68a8f095eff0b5f0985, Data\BinaryData\HDF5\structure\DataObjects\Headers\Messages\DataspaceMessage.vb"

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

    '   Total Lines: 116
    '    Code Lines: 63 (54.31%)
    ' Comment Lines: 34 (29.31%)
    '    - Xml Docs: 79.41%
    ' 
    '   Blank Lines: 19 (16.38%)
    '     File Size: 4.72 KB


    '     Class DataspaceMessage
    ' 
    '         Properties: dimensionLength, flags, maxDimensionLength, numberOfDimensions, totalLength
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
Imports Microsoft.VisualBasic.Data.IO.HDF5.device
Imports Microsoft.VisualBasic.Math
Imports BinaryReader = Microsoft.VisualBasic.Data.IO.HDF5.device.BinaryReader

Namespace struct.messages

    ''' <summary>
    ''' The dataspace message describes the number of dimensions (in other words, “rank”) and size of 
    ''' each dimension that the data object has. This message is only used for datasets which have a 
    ''' simple, rectilinear, array-like layout; datasets requiring a more complex layout are not yet 
    ''' supported.
    ''' </summary>
    Public Class DataspaceMessage : Inherits Message

        ''' <summary>
        ''' This value is used to determine the format of the Dataspace Message. When the format of 
        ''' the information in the message is changed, the version number is incremented and can be 
        ''' used to determine how the information in the object header is formatted. This document 
        ''' describes version one (1) (there was no version zero (0)).
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property version As Integer
        ''' <summary>
        ''' Dimensionality,	this value is the number of dimensions that the data object has.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property numberOfDimensions As Integer
        ''' <summary>
        ''' This field is used to store flags to indicate the presence of parts of this message. 
        ''' Bit 0 (the least significant bit) is used to indicate that maximum dimensions are present. 
        ''' Bit 1 is used to indicate that permutation indices are present.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property flags As Byte
        Public ReadOnly Property type As Integer
        Public ReadOnly Property dimensionLength As Integer()
        Public ReadOnly Property maxDimensionLength As Integer()
        ''' <summary>
        ''' 数据块单位元素的总数量，这个还需要乘以每一个数据块的大小才能够得到总大小值
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property totalLength As Long

        Public Sub New(sb As Superblock, address As Long)
            Call MyBase.New(address)

            Dim [in] As BinaryReader = sb.FileReader(address)

            Me.version = [in].readByte()

            If Me.version = 1 Then
                Me.numberOfDimensions = [in].readByte()
                Me.flags = [in].readByte()
                Me.type = If(Me.numberOfDimensions = 0, 0, 1)

                ' 1 + 4 reserved zero
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

            If type = 2 Then
                totalLength = 0
            Else
                totalLength = dimensionLength _
                    .Select(Function(x) CLng(x)) _
                    .ProductALL
            End If
        End Sub

        Protected Friend Overrides Sub printValues(console As TextWriter)
            console.WriteLine("DataspaceMessage >>>")
            console.WriteLine("address : " & Me.m_address)
            console.WriteLine("version : " & Me.version)
            console.WriteLine("number of dimensions : " & Me.numberOfDimensions)
            console.WriteLine("flags : " & Me.flags)
            console.WriteLine("type : " & Me.type)

            console.WriteLine("DataspaceMessage <<<")
        End Sub
    End Class

End Namespace
