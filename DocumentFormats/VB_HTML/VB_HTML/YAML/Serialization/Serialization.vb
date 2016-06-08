Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.SchemaMaps
Imports Microsoft.VisualBasic.MarkupLanguage.YAML.Grammar
Imports Microsoft.VisualBasic.MarkupLanguage.YAML.Syntax

Namespace YAML

    Public Module Serialization

        <Extension>
        Public Function LoadYAML(Of T)(path As String) As T
            Dim input As New TextInput(path.GET)
            Dim success As Boolean
            Dim parser As New YamlParser()
            Dim yamlStream As YamlStream = parser.ParseYamlStream(input, success)

            If success Then
                Return yamlStream.Load(Of T)
            Else
                Dim ex As New Exception(parser.GetEorrorMessages())
                Throw New Exception(path.ToFileURL, ex)
            End If
        End Function

        <Extension>
        Public Function Load(Of T)(yaml As YamlStream) As T
            Dim type As Type = GetType(T)
            Dim obj As Object = Activator.CreateInstance(type)
            Dim schema = DataFrameColumnAttribute.LoadMapping(type, mapsAll:=True)

            For Each doc As YamlDocument In yaml.Documents

            Next

            Return DirectCast(obj, T)
        End Function

        <Extension>
        Public Function WriteYAML(Of T)(obj As T, path As String, Optional encoding As Encodings = Encodings.Unicode) As Boolean

        End Function
    End Module
End Namespace