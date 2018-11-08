Imports Microsoft.VisualBasic.Data.IO

Public Module Data

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
        Dim size = variable.size / TypeExtensions.num2bytes(type)
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
        Dim type = TypeExtensions.str2num(variable.type)
        Dim width = If(variable.size, variable.size / TypeExtensions.num2bytes(type), 1)

        ' size of the data
        ' TODO streaming data
        Dim size = recordDimension.length

        ' iterates over the data
        Dim data As Object() = New Object(size - 1) {}
        Dim [step] = recordDimension.recordStep

        For i As Integer = 0 To size - 1
            Dim currentOffset& = buffer.Position
            data(i) = TypeExtensions.readType(buffer, type, width)
            buffer.Seek(currentOffset + [step])
        Next

        Return data
    End Function
End Module
