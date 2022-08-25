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
        ''' <paramname="dims"> the array to strip </param>
        ''' <returns> dims with the zeroth element removed </returns>
        Public Shared Function stripLeadingIndex(ByVal dims As Integer()) As Integer()
            Return Arrays.copyOfRange(dims, 1, dims.Length)
        End Function

        Public Shared Function readEnumDataset(ByVal enumDataType As EnumDataType, ByVal buffer As ByteBuffer, ByVal dimensions As Integer()) As Object
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

        Private Shared Sub fillDataUnsigned(ByVal data As Array, ByVal dims As Integer(), ByVal buffer As ByteBuffer, ByVal enumMapping As IDictionary(Of Integer?, String))
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

        Private Shared Sub fillDataUnsignedShort(ByVal data As Array, ByVal dims As Integer(), ByVal buffer As ByteBuffer, ByVal enumMapping As IDictionary(Of Integer?, String))
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

        Private Shared Sub fillDataUnsignedInt32(ByVal data As Array, ByVal dims As Integer(), ByVal buffer As ByteBuffer, ByVal enumMapping As IDictionary(Of Integer?, String))
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

        Private Shared Sub fillDataUnsignedInt64(ByVal data As Array, ByVal dims As Integer(), ByVal buffer As ByteBuffer, ByVal enumMapping As IDictionary(Of Integer?, String))
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
