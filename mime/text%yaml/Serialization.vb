Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.MIME.application.json

Public Module Serialization

    ''' <summary>
    ''' De-serialization of the yaml document file as the required .NET CLR object
    ''' </summary>
    ''' <typeparam name="T">the type information of the required .NET CLR object to be deserialization from the YAML document data</typeparam>
    ''' <param name="path">file path to the target yaml document file</param>
    ''' <returns>Target .NET clr object that de-serialized from the given yaml document file, contains the data that read from the yaml document file.</returns>
    <Extension>
    Public Function LoadYAML(Of T As {New, Class})(path As String) As T
        Try
            Return LoadYAMLDocument(Of T)(path.GET)
        Catch ex As Exception
            Throw New InvalidProgramException(path, ex)
        End Try
    End Function

    <Extension>
    Public Function LoadYAMLDocument(Of T As {New, Class})(yaml As String) As T
        Return New YamlParser().Parse(yaml).CreateObject(Of T)(decodeMetachar:=True)
    End Function
End Module
