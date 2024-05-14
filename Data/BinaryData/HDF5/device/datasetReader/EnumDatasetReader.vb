#Region "Microsoft.VisualBasic::5e174aace797214793fe0267658a6ecc, Data\BinaryData\HDF5\device\datasetReader\EnumDatasetReader.vb"

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

    '   Total Lines: 131
    '    Code Lines: 93
    ' Comment Lines: 25
    '   Blank Lines: 13
    '     File Size: 6.04 KB


    '     Class EnumDatasetReader
    ' 
    '         Function: readEnumDataset, stripLeadingIndex
    ' 
    '         Sub: fillDataUnsigned, fillDataUnsignedInt32, fillDataUnsignedInt64, fillDataUnsignedShort
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' 
'  This file is part of jHDF. A pure Java library for accessing HDF5 files.
' 
'  http://jhdf.io
' 
'  Copyright (c) 2022 James Mudd
' 
'  MIT License see 'LICENSE' file
' 
Imports Microsoft.VisualBasic.Data.IO.HDF5.type
Imports Microsoft.VisualBasic.Language.Java

Namespace device


    ''' <summary>
    ''' Special case of dataset reader for filling enum datasets, handles converting integer values into the enum strings on
    ''' the fly while filling. In the future might want to consider if this is actually what people want, maybe a way to read
    ''' the integer data directly could be useful or even the option to pass in a Java enum and get back a typed array.
    ''' 
    ''' @author James Mudd
    ''' </summary>
    Public Class EnumDatasetReader

        ''' <summary>
        ''' Removes the zeroth (leading) index. e.g [1,2,3] → [2,3]
        ''' </summary>
        ''' <param name="dims"> the array to strip </param>
        ''' <returns> dims with the zeroth element removed </returns>
        Public Shared Function stripLeadingIndex(dims As Integer()) As Integer()
            Return Arrays.copyOfRange(dims, 1, dims.Length)
        End Function

        Public Shared Function readEnumDataset(enumDataType As EnumDataType, buffer As ByteBuffer, dimensions As Integer()) As Object
            Dim baseType = enumDataType.BaseType
            If TypeOf baseType Is FixedPoint Then
                Dim data As Array = Array.CreateInstance(GetType(String), dimensions)
                Dim fixedPoint = CType(baseType, FixedPoint)

                Select Case fixedPoint.size
                    Case 1
                        fillDataUnsigned(data, dimensions, buffer.order(fixedPoint.byteOrder), enumDataType.EnumMapping)
                    Case 2
                        fillDataUnsignedShort(data, dimensions, buffer.order(fixedPoint.byteOrder), enumDataType.EnumMapping)
                    Case 4
                        fillDataUnsignedInt32(data, dimensions, buffer.order(fixedPoint.byteOrder), enumDataType.EnumMapping)
                    Case 8
                        fillDataUnsignedInt64(data, dimensions, buffer.order(fixedPoint.byteOrder), enumDataType.EnumMapping)
                    Case Else
                        Throw New Exception("Unsupported signed integer type size " & fixedPoint.size & " bytes")
                End Select

                Return data
            Else
                Throw New Exception("Trying to fill enum dataset with non-integer base type: " & baseType.ToString)
            End If
        End Function

        Private Shared Sub fillDataUnsigned(data As Array, dims As Integer(), buffer As ByteBuffer, enumMapping As IDictionary(Of Integer?, String))
            If dims.Length > 1 Then
                For i = 0 To dims(0) - 1
                    Dim newArray As Object = data(i)
                    fillDataUnsigned(newArray, stripLeadingIndex(dims), buffer, enumMapping)
                Next
            Else
                Dim tempBuffer = New Byte(dims(dims.Length - 1) - 1) {}
                buffer.get(tempBuffer)
                ' Convert to enum values
                Dim stringData = CType(data, String())
                For i = 0 To tempBuffer.Length - 1
                    stringData(i) = enumMapping(tempBuffer(i))
                Next
            End If
        End Sub

        Private Shared Sub fillDataUnsignedShort(data As Array, dims As Integer(), buffer As ByteBuffer, enumMapping As IDictionary(Of Integer?, String))
            If dims.Length > 1 Then
                For i = 0 To dims(0) - 1
                    Dim newArray As Object = data(i)
                    fillDataUnsigned(newArray, stripLeadingIndex(dims), buffer, enumMapping)
                Next
            Else
                Dim tempBuffer = New Short(dims(dims.Length - 1) - 1) {}
                buffer.get(tempBuffer)
                ' Convert to enum values
                Dim stringData = CType(data, String())
                For i = 0 To tempBuffer.Length - 1
                    stringData(i) = enumMapping((tempBuffer(i)))
                Next
            End If
        End Sub

        Private Shared Sub fillDataUnsignedInt32(data As Array, dims As Integer(), buffer As ByteBuffer, enumMapping As IDictionary(Of Integer?, String))
            If dims.Length > 1 Then
                For i = 0 To dims(0) - 1
                    Dim newArray As Object = data(i)
                    fillDataUnsigned(newArray, stripLeadingIndex(dims), buffer, enumMapping)
                Next
            Else
                Dim tempBuffer = New Integer(dims(dims.Length - 1) - 1) {}
                buffer.get(tempBuffer)
                ' Convert to enum values
                Dim stringData = CType(data, String())
                For i = 0 To tempBuffer.Length - 1
                    stringData(i) = enumMapping(tempBuffer(i))
                Next
            End If
        End Sub

        Private Shared Sub fillDataUnsignedInt64(data As Array, dims As Integer(), buffer As ByteBuffer, enumMapping As IDictionary(Of Integer?, String))
            If dims.Length > 1 Then
                For i = 0 To dims(0) - 1
                    Dim newArray As Object = data(i)
                    fillDataUnsigned(newArray, stripLeadingIndex(dims), buffer, enumMapping)
                Next
            Else
                Dim tempBuffer = New Long(dims(dims.Length - 1) - 1) {}
                Dim tempByteBuffer = ByteBuffer.allocate(8)
                buffer.[get](tempBuffer)
                ' Convert to enum values
                Dim stringData = CType(data, String())
                For i = 0 To tempBuffer.Length - 1
                    tempByteBuffer.putLong(0, tempBuffer(i))
                    stringData(i) = enumMapping(tempBuffer(i))
                Next
            End If
        End Sub

    End Class

End Namespace
