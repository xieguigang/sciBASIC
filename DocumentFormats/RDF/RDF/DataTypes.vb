''' <summary>
''' http://www.w3.org/2001/XMLSchema
''' </summary>
Public Module DataTypes

    Public Const dtString As String = "http://www.w3.org/2001/XMLSchema#string"
    Public Const dtInteger As String = "http://www.w3.org/2001/XMLSchema#int"
    Public Const dtDouble As String = "http://www.w3.org/2001/XMLSchema#float"

    ''' <summary>
    ''' RDF data type
    ''' </summary>
    ''' <returns></returns>
    Public Function [GetType](schema As String) As Type
        If schema.TextEquals(DataTypes.dtDouble) Then
            Return GetType(Double)
        ElseIf schema.TextEquals(DataTypes.dtInteger) Then
            Return GetType(Integer)
        ElseIf schema.TextEquals(DataTypes.dtString) Then
            Return GetType(String)
        Else
            Throw New NotSupportedException(schema)
        End If
    End Function
End Module
