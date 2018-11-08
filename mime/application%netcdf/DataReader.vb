Imports System.IO
Imports Microsoft.VisualBasic.Data.IO
Imports Microsoft.VisualBasic.MIME.application.netCDF.Components

''' <summary>
''' Data reader methods for a given variable data value.
''' </summary>
Module DataReader

    ''' <summary>
    ''' Read data for the given non-record variable
    ''' </summary>
    ''' <param name="buffer">Buffer for the file data</param>
    ''' <param name="variable">Variable metadata</param>
    ''' <returns>Data of the element</returns>
    Public Function nonRecord(buffer, variable) As Object()
        ' variable type
        Dim type = TypeExtensions.str2num(variable.type)
        ' size of the data
        Dim size = variable.size / sizeof(type)
        ' iterates over the data
        Dim data As Object() = New Object(size - 1) {}

        For i As Integer = 0 To size - 1
            data(i) = TypeExtensions.readType(buffer, type, 1)
        Next

        Return data
    End Function

    ''' <summary>
    ''' Read data for the given record variable
    ''' </summary>
    ''' <param name="buffer">Buffer for the file data</param>
    ''' <param name="variable">Variable metadata</param>
    ''' <param name="recordDimension">Record dimension metadata</param>
    ''' <returns>Data of the element</returns>
    Public Function record(buffer As BinaryDataReader, variable As variable, recordDimension As recordDimension) As Object()
        ' variable type
        Dim type As CDFDataTypes = TypeExtensions.str2num(variable.type)
        Dim width% = If(variable.size, variable.size / sizeof(type), 1)

        ' size of the data
        ' TODO streaming data
        Dim size = recordDimension.length

        ' iterates over the data
        Dim data As Object() = New Object(size - 1) {}
        Dim [step] = recordDimension.recordStep

        For i As Integer = 0 To size - 1
            Dim currentOffset& = buffer.Position
            Dim nextOffset = currentOffset + [step]

            If buffer.EndOfStream Then
                data(i) = Nothing
            Else
                data(i) = TypeExtensions.readType(buffer, type, width)
                buffer.Seek(nextOffset, SeekOrigin.Begin)
            End If
        Next

        Return data
    End Function
End Module
