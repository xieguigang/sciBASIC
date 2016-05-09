Imports System.Runtime.CompilerServices
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

    ''' <summary>
    ''' Default is string type if property value of <see cref="EntityProperty.dataType"/> is null or empty
    ''' </summary>
    ''' <param name="x"></param>
    ''' <returns></returns>
    <Extension>
    Public Function SchemaDataType(x As EntityProperty) As Type
        If String.IsNullOrEmpty(x.dataType) Then
            Return GetType(String)
        Else
            Return DataTypes.GetType(x.dataType)
        End If
    End Function

    <Extension>
    Public Function SchemaDataType(type As Type) As String
        Return __types(type)
    End Function

    ReadOnly __types As Dictionary(Of Type, String) =
        New Dictionary(Of Type, String) From {
            {GetType(String), dtString},
            {GetType(Integer), dtInteger},
            {GetType(Double), dtDouble}
    }
End Module
