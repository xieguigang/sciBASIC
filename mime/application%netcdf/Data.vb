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
End Module
